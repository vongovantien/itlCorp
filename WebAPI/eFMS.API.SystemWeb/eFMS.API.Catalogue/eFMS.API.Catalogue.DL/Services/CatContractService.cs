﻿using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatContractService : RepositoryBaseCache<CatContract, CatContractModel>, ICatContractService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<SysOffice> sysOfficeRepository;
        private readonly IContextBase<SysCompany> sysCompanyRepository;
        private readonly IContextBase<SysImage> sysImageRepository;
        private readonly IOptions<WebUrl> webUrl;

        public CatContractService(
            IContextBase<CatContract> repository,
            IMapper mapper,
            IStringLocalizer<CatalogueLanguageSub> localizer,
            ICurrentUser user,
            IContextBase<SysUser> sysUserRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<SysOffice> sysOfficeRepo,
            IContextBase<SysCompany> sysCompanyRepo,
             IContextBase<SysImage> sysImageRepo,
            ICacheServiceBase<CatContract> cacheService, IOptions<WebUrl> url) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            sysUserRepository = sysUserRepo;
            sysOfficeRepository = sysOfficeRepo;
            sysCompanyRepository = sysCompanyRepo;
            catPartnerRepository = partnerRepo;
            webUrl = url;
            sysImageRepository = sysImageRepo;
        }

        public IQueryable<CatContract> GetContracts()
        {
            return DataContext.Get();
        }

        public List<CatContractModel> GetBy(string partnerId)
        {
            IQueryable<CatContract> data = DataContext.Get().Where(x => x.PartnerId.Trim() == partnerId);
            IQueryable<SysUser> sysUser = sysUserRepository.Get();
            var query = from sale in data
                        join user in sysUser on sale.SaleManId equals user.Id
                        select new { sale, user };


            List<CatContractModel> results = new List<CatContractModel>();
            if (data.Count() == 0) return null;

            foreach (var item in query)
            {
                CatContractModel saleman = mapper.Map<CatContractModel>(item.sale);
                SysCompany company = sysCompanyRepository.Get(x => x.Id == saleman.CompanyId)?.FirstOrDefault();
                SysOffice office = sysOfficeRepository.Get(x => x.Id == saleman.OfficeId)?.FirstOrDefault();
                if (company != null)
                {
                    saleman.CompanyNameAbbr = company.BunameAbbr;
                    saleman.CompanyNameEn = company.BunameEn;
                    saleman.CompanyNameVn = company.BunameVn;
                }

                if (office != null)
                {
                    saleman.OfficeNameEn = office.BranchNameEn;
                    saleman.OfficeNameAbbr = office.ShortName;
                    saleman.OfficeNameVn = office.BranchNameVn;
                }

                saleman.Username = item.user.Username;
                results.Add(saleman);
            }
            return results;
        }

        public Guid? GetContractIdByPartnerId(string partnerId)
        {
            var data = DataContext.Get().Where(x => x.PartnerId == partnerId).OrderBy(x => x.DatetimeCreated).Select(x => x.SaleManId).FirstOrDefault();
            if (data == null) return null;
            Guid? salemanId = new Guid(data);
            return salemanId;
        }

        #region CRUD
        public override HandleState Add(CatContractModel entity)
        {
            entity.Id = Guid.NewGuid();
            var contract = mapper.Map<CatContract>(entity);
            contract.DatetimeCreated = DateTime.Now;
            contract.UserCreated = contract.UserModified = currentUser.UserID;
            contract.Active = false;
            var hs = DataContext.Add(contract);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }

        public HandleState Update(CatContractModel model)
        {
            var entity = mapper.Map<CatContract>(model);
            entity.UserModified = currentUser.UserID;
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        public HandleState Delete(Guid id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion

        public IQueryable<CatContractViewModel> Query(CatContractCriteria criteria)
        {
            IQueryable<CatContract> catContracts = DataContext.Get().Where(x => x.PartnerId == criteria.PartnerId);
            IQueryable<SysUser> sysUser = sysUserRepository.Get();

            var query = from contract in catContracts
                        join users in sysUser on contract.SaleManId equals users.Id
                        select new { contract, users };
            if (criteria.All == null)
            {
                query = query.Where(x =>
                           (x.contract.CompanyId == criteria.Company || criteria.Company == Guid.Empty)
                           && (x.contract.OfficeId == criteria.Office || criteria.Office == Guid.Empty)
                           && (x.contract.Active == criteria.Status || criteria.Status == null)
                           );
            }
            else
            {
                query = query.Where(x =>
                            (x.contract.CompanyId == criteria.Company || criteria.Company == Guid.Empty)
                            || (x.contract.OfficeId == criteria.Office || criteria.Office == Guid.Empty)
                            || (x.contract.Active == criteria.Status || criteria.Status == null)
                            || (x.contract.PartnerId == criteria.PartnerId)
                            );
            }
            if (query.Count() == 0) return null;
            List<CatContractViewModel> results = new List<CatContractViewModel>();
            foreach (var item in query)
            {

                CatContractViewModel saleman = mapper.Map<CatContractViewModel>(item.contract);
                saleman.Username = item.users.Username;

                if (saleman.Office != null)
                {
                    SysOffice office = sysOfficeRepository.Get(x => x.Id == saleman.Office)?.FirstOrDefault();
                    saleman.OfficeName = office.ShortName;
                }
                if (saleman.Company != null)
                {
                    SysCompany company = sysCompanyRepository.Get(x => x.Id == saleman.Company)?.FirstOrDefault();
                    saleman.CompanyName = company.BunameEn;
                }
                results.Add(saleman);
            }
            return results?.OrderBy(x => x.CreateDate).AsQueryable();
        }

        public List<CatContractViewModel> Paging(CatContractCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CatContractViewModel> results = null;
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0;
                return results;
            }
            rowsCount = list.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = list.OrderByDescending(x => x.ModifiedDate).Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }


        public async Task<ResultHandle> UploadContractFile(ContractFileUploadModel model)
        {
            return await WriteFile(model);
        }

        private async Task<ResultHandle> WriteFile(ContractFileUploadModel model)
        {
            string fileName = "";
            //string folderName = "images";
            string path = this.webUrl.Value.Url;
            try
            {
                var list = new List<SysImage>();
                /* Kiểm tra các thư mục có tồn tại */
                var hs = new HandleState();
                ImageHelper.CreateDirectoryFile(model.FolderName, model.PartnerId);
                List<SysImage> resultUrls = new List<SysImage>();
                foreach (var file in model.Files)
                {
                    fileName = file.FileName;
                    string objectId = model.PartnerId;
                    await ImageHelper.SaveFile(fileName, model.FolderName, objectId, file);
                    string urlImage = path + "/" + model.FolderName + "files/" + objectId + "/" + fileName;
                    var sysImage = new SysImage
                    {
                        Id = Guid.NewGuid(),
                        Url = urlImage,
                        Name = fileName,
                        Folder = model.FolderName ?? "Partner",
                        ObjectId = model.PartnerId.ToString(),
                        UserCreated = currentUser.UserName, //admin.
                        UserModified = currentUser.UserName,
                        DateTimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now
                    };
                    resultUrls.Add(sysImage);
                    if (!sysImageRepository.Any(x => x.ObjectId == objectId && x.Url == urlImage))
                    {
                        list.Add(sysImage);
                    }
                }
                if (list.Count > 0)
                {
                    list.ForEach(x => x.IsTemp = model.IsTemp);
                    hs = await sysImageRepository.AddAsync(list);
                }
                return new ResultHandle { Data = resultUrls, Status = hs.Success, Message = hs.Message?.ToString() };

            }
            catch (Exception ex)
            {
                return new ResultHandle { Data = null, Status = false, Message = ex.Message };
            }

        }
    }
}
