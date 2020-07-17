using AutoMapper;
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
        private readonly IContextBase<SysEmployee> sysEmployeeRepository;

        private readonly IOptions<WebUrl> webUrl;
        private readonly IOptions<ApiUrl> ApiUrl;


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
                      IContextBase<SysEmployee> sysEmployeeRepo,
            ICacheServiceBase<CatContract> cacheService, IOptions<WebUrl> url, IOptions<ApiUrl> apiurl) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            sysUserRepository = sysUserRepo;
            sysOfficeRepository = sysOfficeRepo;
            sysCompanyRepository = sysCompanyRepo;
            catPartnerRepository = partnerRepo;
            webUrl = url;
            sysImageRepository = sysImageRepo;
            sysEmployeeRepository = sysEmployeeRepo;
            ApiUrl = apiurl;
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
                //var arrToQuery = saleman.OfficeId.ToArray();
                //SysOffice office = sysOfficeRepository.Get(x => arrToQuery.Contains(x.Id))?.FirstOrDefault();
                if (company != null)
                {
                    saleman.CompanyNameAbbr = company.BunameAbbr;
                    saleman.CompanyNameEn = company.BunameEn;
                    saleman.CompanyNameVn = company.BunameVn;
                }

                //if (office != null)
                //{
                //    saleman.OfficeNameEn = office.BranchNameEn;
                //    saleman.OfficeNameAbbr = office.ShortName;
                //    saleman.OfficeNameVn = office.BranchNameVn;
                //}

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
            var contract = mapper.Map<CatContract>(entity);
            contract.DatetimeCreated = contract.DatetimeModified = DateTime.Now;
            contract.UserCreated = contract.UserModified = currentUser.UserID;
            contract.Active = false;
            var hs = DataContext.Add(contract,false);
            if (hs.Success)
            {
                DataContext.SubmitChanges();
                if(entity.isRequestApproval == true)
                {
                    var ObjPartner = catPartnerRepository.Get(x => x.Id == entity.PartnerId).FirstOrDefault();
                    CatPartnerModel model = mapper.Map<CatPartnerModel>(ObjPartner);
                    model.ContractService = entity.SaleService;
                    model.ContractType = entity.ContractType;
                    model.ContractNo = entity.ContractNo;
                    SendMailActiveSuccess(model, string.Empty);
                }
                ClearCache();
                Get();
            }
            return hs;
        }

        public HandleState Update(CatContractModel model)
        {
            var entity = mapper.Map<CatContract>(model);
            entity.UserModified = currentUser.UserID;
            entity.DatetimeModified = DateTime.Now;
            var currentContract = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
            entity.DatetimeCreated = currentContract.DatetimeCreated;
            entity.UserCreated = currentContract.UserCreated;
            var hs = DataContext.Update(entity, x => x.Id == model.Id,false);
            if (hs.Success)
            {
                DataContext.SubmitChanges();
                if (model.isRequestApproval == true)
                {
                    var ObjPartner = catPartnerRepository.Get(x => x.Id == entity.PartnerId).FirstOrDefault();
                    CatPartnerModel modelPartner = mapper.Map<CatPartnerModel>(ObjPartner);
                    modelPartner.ContractService = entity.SaleService;
                    modelPartner.ContractType = entity.ContractType;
                    modelPartner.ContractNo = entity.ContractNo;
                    SendMailActiveSuccess(modelPartner, string.Empty);
                }
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
                           //&& (x.contract.OfficeId == criteria.Office || criteria.Office == Guid.Empty)
                           && (x.contract.Active == criteria.Status || criteria.Status == null)
                           );
            }
            else
            {
                query = query.Where(x =>
                            (x.contract.CompanyId == criteria.Company || criteria.Company == Guid.Empty)
                            //|| (x.contract.OfficeId == criteria.Office || criteria.Office == Guid.Empty)
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

        public CatContractModel GetById(Guid id)
        {
            var result = DataContext.First(x => x.Id == id);
            CatContractModel data = mapper.Map<CatContractModel>(result);
            if (data != null)
            {
                data.UserCreatedName = sysUserRepository.Get(x => x.Id == result.UserCreated).Select(t => t.Username).FirstOrDefault();
                data.UserModifiedName = sysUserRepository.Get(x => x.Id == result.UserModified).Select(t => t.Username).FirstOrDefault();
            }
            return data;
        }

        public HandleState ActiveInActiveContract(Guid id, string partnerId,SalesmanCreditModel credit)
        {
            var isUpdateDone = new HandleState();
            var objUpdate = DataContext.First(x => x.Id == id);
            if (objUpdate != null)
            {
                objUpdate.Active = objUpdate.Active == true ? false : true;
                if(credit.CreditLimit != null)
                {
                    objUpdate.CreditLimit = credit.CreditLimit;
                }
                if (credit.CreditRate != null)
                {
                    objUpdate.CreditLimitRate = credit.CreditRate;
                }
                isUpdateDone = DataContext.Update(objUpdate, x => x.Id == objUpdate.Id, false);
            }
            if (isUpdateDone.Success)
            {
                DataContext.SubmitChanges();
                if (objUpdate.Active == true)
                {
                    // send email
                    var ObjPartner = catPartnerRepository.Get(x => x.Id == partnerId).FirstOrDefault();
                    CatPartnerModel model = mapper.Map<CatPartnerModel>(ObjPartner);
                    model.ContractService = objUpdate.SaleService;
                    model.ContractType = objUpdate.ContractType;
                    SendMailActiveSuccess(model,"active");
                }
            }
            return isUpdateDone;
        }

        private void SendMailActiveSuccess(CatPartnerModel partner,string type)
        {
            string employeeId = sysUserRepository.Get(x => x.Id == currentUser.UserID).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoCreator = sysEmployeeRepository.Get(e => e.Id == employeeId)?.FirstOrDefault();
            string FullNameCreatetor = objInfoCreator?.EmployeeNameVn;
            string EnNameCreatetor = objInfoCreator?.EmployeeNameEn;
            string url = string.Empty;
            // info send to and cc
            List<string> lstCc = new List<string>
            {

            };
            string emailCreator = objInfoCreator.Email;

            switch (partner.PartnerType)
            {
                case "Customer":
                    url = "home/commercial/customer/";
                    break;
                case "Agent":
                    url = "home/commercial/agent/";
                    break;
                default:
                    url = "home/catalogue/partner-data/detail/";
                    break;
            }

            string linkVn = string.Empty;
            string linkEn = string.Empty;
            string subject = string.Empty;
            string body = string.Empty;
            string address = webUrl.Value.Url + "/en/#/" + url + partner.Id;
            if(type == "active")
            {
                linkEn = "View more detail, please you <a href='" + address + "'> click here </a>" + "to view detail.";
                linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
                subject = "Actived Agent - " + partner.ShortName;
                body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'> Dear " + EnNameCreatetor + ", </br> </br>" +

                    "<i> You Agent - " + partner.PartnerNameVn + " is active with info below </i> </br>" +
                    "<i> Khách hàng - " + partner.PartnerNameVn + " đã được duyệt với thông tin như sau: </i> </br> </br>" +

                    "\t  Agent Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
                    "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +
                    "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + partner.ContractService + "</b>" + "</br>" +
                    "\t  Contract type  / <i> Loại hợp đồng: </i> " + "<b>" + partner.ContractType + "</b>" + "</br> </br>"
                    + linkEn + "</br>" + linkVn + "</br> </br>" +
                    "<i> Thanks and Regards </i>" + "</br> </br>" +
                    "eFMS System </div>");
            } else
            {
                linkEn = "You can <a href='" + address + "'> click here </a>" + "to view detail.";
                linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
                subject = "eFMS - Customer Approval Request From " + EnNameCreatetor;

                body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'> Dear Accountant/AR Team, "  + " </br> </br>" +

                  "<i> You have a Customer Approval request from " + EnNameCreatetor + " as info below </i> </br>" +
                  "<i> Bạn có một yêu cầu xác duyệt khách hàng từ " + EnNameCreatetor + " với thông tin như sau: </i> </br> </br>" +

                  "\t  Customer ID  / <i> Mã Agent:</i> " + "<b>" + partner.AccountNo + "</b>" + "</br>" +
                  "\t  Customer Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
                  "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +

                  "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + partner.ContractService + "</b>" + "</br>" +
                  "\t  Contract type  / <i> Loại hợp đồng: </i> " + "<b>" + partner.ContractType + "</b>" + "</br>"+
                  "\t  Contract No  / <i> Số hợp đồng: </i> " + "<b>" + partner.ContractNo + "</b>" + "</br>" + 
                  "\t  Requestor  / <i> Người yêu cầu: </i> " + "<b>" + EnNameCreatetor + "</b>" + "</br> </br>"

                  + linkEn + "</br>" + linkVn + "</br> </br>" +
                  "<i> Thanks and Regards </i>" + "</br> </br>" +
                  "eFMS System </div>");
            }
    
            SendMail.Send(subject, body, new List<string> { "samuel.an@logtechub.com","luis.quang@itlvn.com" }, null, null);
        }

        public SysImage GetFileContract(string partnerId, string contractId)
        {
            var result = sysImageRepository.Get(x => x.ObjectId == partnerId && x.ChildId == contractId).OrderByDescending(x => x.DateTimeCreated).FirstOrDefault();
            return result;
        }

        public HandleState UpdateFileToContract(List<SysImage> files)
        {

            var isUpdateDone = new HandleState();
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in files)
                    {
                        item.IsTemp = null;
                        item.DateTimeCreated = item.DatetimeModified = DateTime.Now;
                        isUpdateDone = sysImageRepository.Update(item, x => x.Id == item.Id);
                    }
                    trans.Commit();
                    return isUpdateDone;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public async Task<ResultHandle> UploadMoreContractFile(List<ContractFileUploadModel> model)
        {
            var result = new ResultHandle();
            foreach (var item in model)
            {
                if (item.Files != null)
                {
                    result = await WriteFile(item);
                }
            }
            return result;
        }

        public async Task<ResultHandle> UploadContractFile(ContractFileUploadModel model)
        {
            return await WriteFile(model);
        }

        private async Task<ResultHandle> WriteFile(ContractFileUploadModel model)
        {
            string fileName = "";
            //string folderName = "images";
            string path = this.ApiUrl.Value.Url + "/Catalogue";
            try
            {
                var list = new List<SysImage>();
                /* Kiểm tra các thư mục có tồn tại */
                var hs = new HandleState();
                ImageHelper.CreateDirectoryFile(model.FolderName, model.PartnerId);
                List<SysImage> resultUrls = new List<SysImage>();
                fileName = model.Files.FileName;
                string objectId = model.PartnerId;
                await ImageHelper.SaveFile(fileName, model.FolderName, objectId, model.Files);
                string urlImage = path + "/" + model.FolderName + "files/" + objectId + "/" + fileName;
                var sysImage = new SysImage
                {
                    Id = Guid.NewGuid(),
                    Url = urlImage,
                    Name = fileName,
                    Folder = model.FolderName ?? "Partner",
                    ObjectId = model.PartnerId.ToString(),
                    ChildId = model.ChildId.ToString(),
                    UserCreated = currentUser.UserName, //admin.
                    UserModified = currentUser.UserName,
                    DateTimeCreated = DateTime.Now,
                    DatetimeModified = DateTime.Now
                };
                resultUrls.Add(sysImage);
                if (!sysImageRepository.Any(x => x.ObjectId == objectId && x.Url == urlImage && x.ChildId == model.ChildId))
                {
                    list.Add(sysImage);
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

        public async Task<HandleState> DeleteFileContract(Guid id)
        {
            var item = sysImageRepository.Get(x => x.Id == id).FirstOrDefault();
            if (item == null) return new HandleState("Not found data");
            var result = sysImageRepository.Delete(x => x.Id == id);
            if (result.Success)
            {
                var hs = await ImageHelper.DeleteFile(item.Name, item.ObjectId);
            }
            return result;
        }
    }
}
