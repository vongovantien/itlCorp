using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatSalemanService : RepositoryBaseCache<CatSaleman, CatSaleManModel>, ICatSaleManService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<SysOffice> sysOfficeRepository;
        private readonly IContextBase<SysCompany> sysCompanyRepository;

        public CatSalemanService(
            IContextBase<CatSaleman> repository, 
            IMapper mapper, 
            IStringLocalizer<CatalogueLanguageSub> localizer, 
            ICurrentUser user, 
            IContextBase<SysUser> sysUserRepo, 
            IContextBase<CatPartner> partnerRepo, 
            IContextBase<SysOffice> sysOfficeRepo,
            IContextBase<SysCompany> sysCompanyRepo,
            ICacheServiceBase<CatSaleman> cacheService) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            sysUserRepository = sysUserRepo;
            sysOfficeRepository = sysOfficeRepo;
            sysCompanyRepository = sysCompanyRepo;
            catPartnerRepository = partnerRepo;
        }

        public IQueryable<CatSaleman> GetSaleMan()
        {
            return DataContext.Get();
        }

        public List<CatSaleManModel> GetBy(string partnerId)
        {
            IQueryable<CatSaleman> data = DataContext.Get().Where(x => x.PartnerId.Trim() == partnerId);
            IQueryable<SysUser> sysUser = sysUserRepository.Get();
            var query = from sale in data
                        join user in sysUser on sale.SaleManId equals user.Id
                        select new { sale, user };


            List<CatSaleManModel> results = new List<CatSaleManModel>();
            if (data.Count() == 0) return null;

            foreach (var item in query)
            {
                CatSaleManModel saleman = mapper.Map<CatSaleManModel>(item.sale);
                SysCompany company = sysCompanyRepository.Get(x => x.Id == saleman.Company)?.FirstOrDefault();
                SysOffice office = sysOfficeRepository.Get(x => x.Id == saleman.Office)?.FirstOrDefault();
                if (company != null)
                {
                    saleman.CompanyNameAbbr = company.BunameAbbr;
                    saleman.CompanyNameEn = company.BunameEn;
                    saleman.CompanyNameVn = company.BunameVn;
                }

                if(office != null)
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

        public Guid? GetSalemanIdByPartnerId(string partnerId)
        {
            var data = DataContext.Get().Where(x => x.PartnerId == partnerId).OrderBy(x=>x.CreateDate).Select(x=>x.SaleManId).FirstOrDefault();
            if (data == null) return null;
            Guid? salemanId = new Guid(data);
            return salemanId;
        }

        #region CRUD
        public override HandleState Add(CatSaleManModel entity)
        {
            entity.Id = Guid.NewGuid();
            var saleMan = mapper.Map<CatSaleman>(entity);
            saleMan.CreateDate = DateTime.Now;
            saleMan.UserCreated = currentUser.UserID;

            var hs = DataContext.Add(saleMan);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }

        public HandleState Update(CatSaleManModel model)
        {
            var entity = mapper.Map<CatSaleman>(model);
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

        public IQueryable<CatSaleManViewModel> Query(CatSalemanCriteria criteria)
        {
            var salesMan = DataContext.Get().Where(x => x.PartnerId == criteria.PartnerId);
            var sysUser = sysUserRepository.Get();
            var query = from saleman in salesMan
                        join users in sysUser on saleman.SaleManId equals users.Id
                        select new { saleman, users };
            if (criteria.All == null)
            {
                query = query.Where(x =>
                           (x.saleman.Company == criteria.Company || criteria.Company == Guid.Empty)
                           && (x.saleman.Office == criteria.Office || criteria.Office == Guid.Empty)
                           && (x.saleman.Status == criteria.Status || criteria.Status == null)
                           );
            }
            else
            {
                query = query.Where(x =>
                            (x.saleman.Company == criteria.Company || criteria.Company == Guid.Empty)
                            || (x.saleman.Office == criteria.Office || criteria.Office == Guid.Empty)
                            || (x.saleman.Status == criteria.Status || criteria.Status == null)
                            || (x.saleman.PartnerId == criteria.PartnerId)
                            );
            }
            if (query.Count() == 0) return null;
            List<CatSaleManViewModel> results = new List<CatSaleManViewModel>();
            foreach (var item in query)
            {

                var saleman = mapper.Map<CatSaleManViewModel>(item.saleman);
                saleman.Username = item.users.Username;
                results.Add(saleman);
            }
            return results?.OrderBy(x=>x.CreateDate).AsQueryable();
        }

        public List<CatSaleManViewModel> Paging(CatSalemanCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CatSaleManViewModel> results = null;
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

    }
}
