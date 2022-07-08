using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IContextBase<CatDepartment> catDepartmentRepository;
        private readonly IContextBase<CsTransaction> transactionRepository;
        private readonly IContextBase<OpsTransaction> opsRepository;
        private readonly IContextBase<SysSentEmailHistory> sendEmailHistoryRepository;
        readonly IContextBase<SysUserLevel> userlevelRepository;
        private readonly IContextBase<SysEmailSetting> sysEmailSettingRepository;
        private readonly IContextBase<SysEmailTemplate> sysEmailTemplateRepository;
        private readonly IOptions<WebUrl> webUrl;
        private readonly IOptions<ApiUrl> ApiUrl;
        readonly IUserPermissionService permissionService;



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
            IContextBase<CatDepartment> catDepartmentRepo,
            IContextBase<CsTransaction> transactionRepo,
            IContextBase<OpsTransaction> opsRepo,
            IContextBase<SysSentEmailHistory> sendEmailHistoryRepo,
            IContextBase<SysUserLevel> userlevelRepo,
            IContextBase<SysEmailSetting> sysEmailSettingRepo,
            IContextBase<SysEmailTemplate> sysEmailTemplateRepo,
            IUserPermissionService perService,
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
            catDepartmentRepository = catDepartmentRepo;
            transactionRepository = transactionRepo;
            opsRepository = opsRepo;
            ApiUrl = apiurl;
            sendEmailHistoryRepository = sendEmailHistoryRepo;
            userlevelRepository = userlevelRepo;
            permissionService = perService;
            sysEmailSettingRepository = sysEmailSettingRepo;
            sysEmailTemplateRepository = sysEmailTemplateRepo;
        }

        public IQueryable<CatContract> GetContracts()
        {
            return DataContext.Get();
        }


        public List<CatContractModel> GetBy(string partnerId, bool? all)
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
                if (company != null)
                {
                    saleman.CompanyNameAbbr = company.BunameAbbr;
                    saleman.CompanyNameEn = company.BunameEn;
                    saleman.CompanyNameVn = company.BunameVn;
                }
                var officeIds = saleman.OfficeId?.Split(";").ToList();
                if (officeIds != null)
                {
                    if (officeIds.Count() > 0)
                    {
                        foreach (var officeId in officeIds)
                        {
                            saleman.OfficeNameAbbr += sysOfficeRepository.Get(x => x.Id == new Guid(officeId)).Select(t => t.ShortName).FirstOrDefault() + "; ";
                        }
                    }
                }
                if (saleman.OfficeNameAbbr != null)
                {
                    if (saleman.OfficeNameAbbr.Length > 0)
                    {
                        saleman.OfficeNameAbbr = saleman.OfficeNameAbbr.Remove(saleman.OfficeNameAbbr.Length - 2);
                    }
                }
                saleman.SaleServiceName = GetContractServicesName(saleman.SaleService);
                saleman.Username = item.user.Username;
                saleman.CreatorCompanyId = userlevelRepository.Get(x => x.UserId == saleman.UserCreated && x.CompanyId == currentUser.CompanyID).Select(t => t.CompanyId).FirstOrDefault();
                saleman.CreatorOfficeId = userlevelRepository.Get(x => x.UserId == saleman.UserCreated && x.OfficeId == currentUser.OfficeID).Select(t => t.OfficeId).FirstOrDefault();
                saleman.CreatorDepartmentId = userlevelRepository.Get(x => x.UserId == saleman.UserCreated && x.DepartmentId == currentUser.DepartmentId).Select(t => t.DepartmentId).FirstOrDefault();
                saleman.CreatorGroupId = userlevelRepository.Get(x => x.UserId == saleman.UserCreated && x.DepartmentId == currentUser.GroupId).Select(t => t.GroupId).FirstOrDefault();
                results.Add(saleman);
            }
            if (all == true) return results;

            string partnerType = catPartnerRepository.Get(x => x.Id == partnerId).Select(t => t.PartnerType).FirstOrDefault();
            ICurrentUser _user = null;
            switch (partnerType)
            {
                case "Customer":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialCustomer);
                    break;
                case "Agent":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialAgent);
                    break;
                default:
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);
                    break;
            }

            PermissionRange rangeSearch = 0;
            rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);

            switch (rangeSearch)
            {
                case PermissionRange.None:
                    results = null;
                    break;
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    results = results.Where(x => x.UserCreated == currentUser.UserID || x.SaleManId == currentUser.UserID).ToList();
                    break;
                case PermissionRange.Group:
                    var dataUserLevel = userlevelRepository.Get(x => x.GroupId == currentUser.GroupId).Select(t => t.UserId).ToList();
                    results = results.Where(x => (x.CreatorGroupId == currentUser.GroupId && x.CreatorDepartmentId == currentUser.DepartmentId && x.CreatorOfficeId == currentUser.OfficeID && x.CreatorCompanyId == currentUser.CompanyID)
                        || x.UserCreated == currentUser.UserID || x.SaleManId == currentUser.UserID || dataUserLevel.Contains(x.SaleManId)).ToList();
                    break;
                case PermissionRange.Department:
                    var dataUserLevelDepartment = userlevelRepository.Get(x => x.DepartmentId == currentUser.DepartmentId).Select(t => t.UserId).ToList();
                    results = results.Where(x => (x.CreatorDepartmentId == currentUser.DepartmentId && x.CreatorOfficeId == currentUser.OfficeID && x.CreatorCompanyId == currentUser.CompanyID)
                      || x.UserCreated == currentUser.UserID || x.SaleManId == currentUser.UserID || dataUserLevelDepartment.Contains(x.SaleManId)).ToList();
                    break;
                case PermissionRange.Office:
                    results = results.Where(x => (x.CreatorOfficeId == currentUser.OfficeID && x.CreatorCompanyId == currentUser.CompanyID)
                    || x.UserCreated == currentUser.UserID || x.SaleManId == currentUser.UserID).ToList();
                    break;
                case PermissionRange.Company:
                    results = results.Where(x => x.CreatorCompanyId == currentUser.CompanyID
                    || x.UserCreated == currentUser.UserID || x.SaleManId == currentUser.UserID).ToList();
                    break;
            }
            return results;
        }

        public object GetContractIdByPartnerId(string partnerId, string jobId)
        {
            jobId = jobId == "null" || jobId == "undefined" ? null : jobId;
            var DataShipment = !string.IsNullOrEmpty(jobId) ? transactionRepository.Get(x => x.Id == new Guid(jobId)).FirstOrDefault() : null;
            var DataContract = DataContext.Get();
            string OfficeNameAbbr = string.Empty;
            string data = string.Empty;
            // truong hop custom logistic
            if (string.IsNullOrEmpty(jobId))
            {
                data = DataContract.Where(x => x.PartnerId == partnerId && x.OfficeId.Contains(currentUser.OfficeID.ToString()) && x.SaleService.Contains("CL") && x.Active == true).Select(x => x.SaleManId).FirstOrDefault();
                if (string.IsNullOrEmpty(data))
                {
                    string IdAcRefPartner = catPartnerRepository.Get(x => x.Id == partnerId).Select(t => t.ParentId).FirstOrDefault();
                    data = DataContract.Where(x => x.PartnerId == IdAcRefPartner && IdAcRefPartner != null && x.OfficeId.Contains(currentUser.OfficeID.ToString()) && x.SaleService.Contains("CL") && x.Active == true).Select(x => x.SaleManId).FirstOrDefault();
                    OfficeNameAbbr = sysOfficeRepository.Get(x => x.Id == currentUser.OfficeID).Select(t => t.ShortName).FirstOrDefault();

                }
            }
            else
            {
                data = DataContract.Where(x => x.PartnerId == partnerId && x.OfficeId.Contains(DataShipment.OfficeId.ToString()) && x.SaleService.Contains(DataShipment.TransactionType) && x.Active == true).Select(x => x.SaleManId).FirstOrDefault();
                if (string.IsNullOrEmpty(data))
                {
                    string IdAcRefPartner = catPartnerRepository.Get(x => x.Id == partnerId).Select(t => t.ParentId).FirstOrDefault();
                    data = DataContract.Where(x => x.PartnerId == IdAcRefPartner && IdAcRefPartner != null && x.OfficeId.Contains(DataShipment.OfficeId.ToString()) && x.SaleService.Contains(DataShipment.TransactionType) && x.Active == true).Select(x => x.SaleManId).FirstOrDefault();
                    OfficeNameAbbr = sysOfficeRepository.Get(x => x.Id == DataShipment.OfficeId).Select(t => t.ShortName).FirstOrDefault();

                }
            }

            if (data == null) return new { OfficeNameAbbr };
            Guid? salemanId = new Guid(data);
            return new { salemanId };
        }

        #region CRUD
        public override HandleState Add(CatContractModel entity)
        {
            var contract = mapper.Map<CatContract>(entity);
            contract.DatetimeCreated = contract.DatetimeModified = DateTime.Now;
            contract.UserCreated = contract.UserModified = currentUser.UserID;
            contract.Active = false;
            if (contract.ContractType == "Guarantee") // Default các giá trị khi hđ type Guarantee
            {
                contract.PaymentTerm = 1;
                contract.CreditLimit = 20000000;
                contract.CreditLimitRate = 120;
            }
            var hs = DataContext.Add(contract, false);
            DataContext.SubmitChanges();
            if (hs.Success)
            {
                if (entity.IsRequestApproval == true)
                {
                    var ObjPartner = catPartnerRepository.Get(x => x.Id == entity.PartnerId).FirstOrDefault();
                    CatPartnerModel model = mapper.Map<CatPartnerModel>(ObjPartner);
                    model.ContractService = entity.SaleService;

                    model.ContractService = GetContractServicesName(model.ContractService);

                    model.ContractType = entity.ContractType;
                    model.ContractNo = entity.ContractNo;
                    model.SalesmanId = entity.SaleManId;
                    model.UserCreatedContract = contract.UserCreated;
                    model.UserCreated = entity.UserCreated;
                    model.OfficeIdContract = entity.OfficeId;
                    SendMailActiveSuccess(model, string.Empty);
                    ClearCache();
                    Get();
                }

            }
            return hs;
        }

        public string CheckExistedContract(CatContractModel model)
        {
            ClearCache();
            Get();
            string messageDuplicate = string.Empty;
            int LengthService = model.SaleService.Split(";").ToArray().Length;
            int LengthOffice = model.OfficeId.Split(";").ToArray().Length;
            var contractPartner = DataContext.Get(x => x.PartnerId == model.PartnerId && x.Active == true);
            if (model.ContractType == "Official") // [CR:17/05/22] Chỉ check trùng contract no type official khi add/update
            {
                if (!string.IsNullOrEmpty(model.ContractNo))
                {
                    if (contractPartner.Any(x => !string.IsNullOrEmpty(x.ContractNo) && x.ContractNo.Trim() == model.ContractNo.Trim() && (model.Id == Guid.Empty || x.Id != model.Id)))
                    {
                        messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACT_NO_EXISTED], model.ContractNo);
                        return messageDuplicate;
                    }
                }
            }
            #region delete theo CR 17/05/22
            //if (model.Id == Guid.Empty)
            //{
            //    if (model.ContractType == "Official") // [CR:17/05/22] Chỉ check trùng contract no type official khi add/update
            //    {
            //        if (!string.IsNullOrEmpty(model.ContractNo))
            //        {
            //            if (contractPartner.Any(x => !string.IsNullOrEmpty(x.ContractNo) && x.ContractNo.Trim() == model.ContractNo.Trim()))
            //            {
            //                messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACT_NO_EXISTED], model.ContractNo);
            //                return messageDuplicate;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //var DataCheck = DataContext.Get(x => x.PartnerId == model.PartnerId);
            //        if (!contractPartner.Any(x => x.SaleManId == model.SaleManId))
            //        {
            //            if (contractPartner.Any(x => (LengthService == 1 ? x.SaleService.Contains(model.SaleService) : x.SaleService.Intersect(model.SaleService).Any()) && x.ContractType == model.ContractType && (LengthOffice == 1 ? x.OfficeId.Contains(model.OfficeId) : x.OfficeId.Intersect(model.OfficeId).Any())))
            //            {
            //                messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_DUPLICATE_SERVICE]);
            //            }
            //        }
            //        else
            //        {
            //            //var data = contractPartner.Where(x => x.Active == false || x.Active == null);
            //            if (contractPartner.Any(x => (LengthService == 1 ? x.SaleService.Contains(model.SaleService) : x.SaleService.Intersect(model.SaleService).Any()) && x.ContractType == model.ContractType && (LengthOffice == 1 ? x.OfficeId.Contains(model.OfficeId) : x.OfficeId.Intersect(model.OfficeId).Any())))
            //            {
            //                messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_DUPLICATE_SERVICE]);
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    if (model.ContractType == "Official") // [CR:17/05/22] Chỉ check trùng contract no type official khi add/update
            //    {
            //        if (!string.IsNullOrEmpty(model.ContractNo))
            //        {

            //            if (contractPartner.Any(x => !string.IsNullOrEmpty(x.ContractNo) && x.ContractNo.Trim() == model.ContractNo.Trim() && x.Id != model.Id && model.Active == true))
            //            {
            //                return messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACT_NO_EXISTED], model.ContractNo);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //var DataCheck = DataContext.Get(x => x.PartnerId == model.PartnerId);
            //        if (!contractPartner.Any(x => x.SaleManId == model.SaleManId))
            //        {
            //            if (contractPartner.Any(x => (LengthService == 1 ? x.SaleService.Contains(model.SaleService) : x.SaleService.Intersect(model.SaleService).Any()) && x.ContractType == model.ContractType && (LengthOffice == 1 ? x.OfficeId.Contains(model.OfficeId) : x.OfficeId.Intersect(model.OfficeId).Any()) && x.Id != model.Id && model.Active == true))
            //            {
            //                messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_DUPLICATE_SERVICE]);
            //            }
            //        }
            //        else
            //        {
            //            //var data = contractPartner.Where(x => x.Active == false || x.Active == null);
            //            if (contractPartner.Any(x => (LengthService == 1 ? x.SaleService.Contains(model.SaleService) : x.SaleService.Intersect(model.SaleService).Any()) && x.ContractType == model.ContractType && (LengthOffice == 1 ? x.OfficeId.Contains(model.OfficeId) : x.OfficeId.Intersect(model.OfficeId).Any()) && x.Id != model.Id && model.Active == true))
            //            {
            //                messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_DUPLICATE_SERVICE]);
            //            }
            //        }
            //    }
            //}
            #endregion
            return messageDuplicate;
        }


        private string GetContractServicesName(string ContractService)
        {
            string ContractServicesName = string.Empty;
            var ContractServiceArr = ContractService.Split(";").ToArray();
            if (ContractServiceArr.Any())
            {
                foreach (var item in ContractServiceArr)
                {
                    switch (item)
                    {
                        case "AE":
                            ContractServicesName += "Air Export; ";
                            break;
                        case "AI":
                            ContractServicesName += "Air Import; ";
                            break;
                        case "SCE":
                            ContractServicesName += "Sea Consol Export; ";
                            break;
                        case "SCI":
                            ContractServicesName += "Sea Consol Import; ";
                            break;
                        case "SFE":
                            ContractServicesName += "Sea FCL Export; ";
                            break;
                        case "SLE":
                            ContractServicesName += "Sea LCL Export; ";
                            break;
                        case "SLI":
                            ContractServicesName += "Sea LCL Import; ";
                            break;
                        case "CL":
                            ContractServicesName += "Custom Logistic; ";
                            break;
                        case "IT":
                            ContractServicesName += "Trucking; ";
                            break;
                        case "SFI":
                            ContractServicesName += "Sea FCL Import; ";
                            break;
                        default:
                            ContractServicesName = "Air Export; Air Import; Sea Consol Export; Sea Consol Import; Sea FCL Export; Sea LCL Export; Sea LCL Import; Custom Logistic; Trucking  ";
                            break;
                    }
                }

            }
            if (!string.IsNullOrEmpty(ContractServicesName))
            {
                ContractServicesName = ContractServicesName.Remove(ContractServicesName.Length - 2);
            }
            return ContractServicesName;
        }

        public HandleState Update(CatContractModel model, out bool isChangeAgrmentType)
        {
            var entity = mapper.Map<CatContract>(model);
            entity.UserModified = currentUser.UserID;
            entity.DatetimeModified = DateTime.Now;
            var currentContract = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
            isChangeAgrmentType = model.PaymentTerm != currentContract.PaymentTerm;
            entity.DatetimeCreated = currentContract.DatetimeCreated;
            entity.UserCreated = currentContract.UserCreated;
            var hs = DataContext.Update(entity, x => x.Id == model.Id, false);
            if (hs.Success)
            {
                DataContext.SubmitChanges();
                if (model.IsRequestApproval == true)
                {
                    var ObjPartner = catPartnerRepository.Get(x => x.Id == entity.PartnerId).FirstOrDefault();
                    CatPartnerModel modelPartner = mapper.Map<CatPartnerModel>(ObjPartner);
                    modelPartner.ContractService = entity.SaleService;
                    modelPartner.ContractService = GetContractServicesName(modelPartner.ContractService);
                    modelPartner.ContractType = entity.ContractType;
                    modelPartner.ContractNo = entity.ContractNo;
                    modelPartner.SalesmanId = entity.SaleManId;
                    modelPartner.UserCreatedContract = entity.UserCreated;
                    modelPartner.OfficeIdContract = entity.OfficeId;
                    ClearCache();
                    Get();
                    SendMailActiveSuccess(modelPartner, string.Empty);
                }
            }
            return hs;
        }

        public HandleState CustomerRequest(CatContractModel entity)
        {
            var contract = mapper.Map<CatContract>(entity);
            contract.DatetimeCreated = contract.DatetimeModified = DateTime.Now;
            contract.UserCreated = contract.UserModified = currentUser.UserID;
            contract.Active = false;
            var hs = DataContext.Add(contract, false);
            DataContext.SubmitChanges();
            var hsPartner = new HandleState();
            if (hs.Success)
            {
                var ObjPartner = catPartnerRepository.Get(x => x.Id == contract.PartnerId).FirstOrDefault();
                ObjPartner.PartnerGroup += (ObjPartner.PartnerGroup.Contains(DataEnums.CustomerPartner) ? string.Empty : (";" + DataEnums.CustomerPartner));
                ObjPartner.UserModified = currentUser.UserID;
                ObjPartner.DatetimeModified = DateTime.Now;
                ObjPartner.PartnerType = "Customer";
                hsPartner = catPartnerRepository.Update(ObjPartner, x => x.Id == contract.PartnerId, false);
                catPartnerRepository.SubmitChanges();
                if (entity.IsRequestApproval == true && hsPartner.Success)
                {
                    CatPartnerModel modelPartner = mapper.Map<CatPartnerModel>(ObjPartner);
                    modelPartner.ContractService = entity.SaleService;
                    modelPartner.ContractService = GetContractServicesName(modelPartner.ContractService);
                    modelPartner.ContractType = contract.ContractType;
                    modelPartner.ContractNo = contract.ContractNo;
                    modelPartner.SalesmanId = contract.SaleManId;
                    modelPartner.UserCreatedContract = contract.UserCreated;
                    modelPartner.ContractType = contract.ContractType;
                    modelPartner.OfficeIdContract = contract.OfficeId;
                    ClearCache();
                    Get();
                    SendMailActiveSuccess(modelPartner, string.Empty);
                }

            }
            return hsPartner;
        }
        public HandleState Delete(Guid id)
        {
            var contract = DataContext.First(x => x.Id == id);
            if(contract == null)
            {
                return new HandleState(LanguageSub.MSG_DATA_NOT_FOUND);
            }
            if(contract.Active == true)
            {
                return new HandleState((object)string.Format("The Contract is active"));
            }
            if (contract.DebitAmount != null && contract.DebitAmount != 0)
            {
                return new HandleState((object)string.Format("The contract is recording the receivable"));
            }
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
                           && (x.contract.Active == criteria.Status || criteria.Status == null)
                           );
            }
            else
            {
                query = query.Where(x =>
                            (x.contract.CompanyId == criteria.Company || criteria.Company == Guid.Empty)
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
            string PartnerType = catPartnerRepository.Get(x => x.Id == result.PartnerId).Select(t => t.PartnerType).FirstOrDefault();

            ICurrentUser _user = null;
            switch (PartnerType)
            {
                case "Customer":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialCustomer);
                    break;
                case "Agent":
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialAgent);
                    break;
                default:
                    _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
                    break;
            }
            var specialActions = _user.UserMenuPermission.SpecialActions;
            if (specialActions.Count > 0)
            {
                if (specialActions.FirstOrDefault(x => x.Action == "DetailAgreement")?.IsAllow == true || result.UserCreated == currentUser.UserID || result.SaleManId == currentUser.UserID)
                {
                    data.ViewDetail = true;
                }
            }
            if (data != null)
            {
                data.UserCreatedName = sysUserRepository.Get(x => x.Id == result.UserCreated).Select(t => t.Username).FirstOrDefault();
                data.UserModifiedName = sysUserRepository.Get(x => x.Id == result.UserModified).Select(t => t.Username).FirstOrDefault();
            }
            return data;
        }

        public HandleState ActiveInActiveContract(Guid id, string partnerId, SalesmanCreditModel credit, out bool active)
        {
            active = false;
            var isUpdateDone = new HandleState();
            var objUpdate = DataContext.First(x => x.Id == id);
            var DataCheckExisted = CheckExistedContractActive(id, partnerId);
            if (DataCheckExisted != null && DataCheckExisted.Count() > 0 && objUpdate.Active == false)
            {
                foreach (var item in DataCheckExisted)
                {
                    item.UserModified = currentUser.UserID;
                    item.DatetimeModified = DateTime.Now;
                    item.Active = false;
                    var isUpdateAgreementActive = DataContext.Update(item, x => x.Id == item.Id, false);
                }
            }
            if (objUpdate != null)
            {
                objUpdate.Active = objUpdate.Active == true ? false : true;
                active = objUpdate.Active ?? false;
                if (credit.CreditLimit != null)
                {
                    objUpdate.CreditLimit = credit.CreditLimit;
                }
                if (credit.CreditRate != null)
                {
                    objUpdate.CreditLimitRate = credit.CreditRate;
                }
                objUpdate.DatetimeModified = DateTime.Now;
                objUpdate.UserModified = currentUser.UserID;
                isUpdateDone = DataContext.Update(objUpdate, x => x.Id == objUpdate.Id, false);
            }
            if (isUpdateDone.Success)
            {
                DataContext.SubmitChanges();
                if (objUpdate.Active == true)
                {
                    // send email
                    var ObjPartner = catPartnerRepository.Get(x => x.Id == partnerId).FirstOrDefault();
                    ObjPartner.Active = true;
                    catPartnerRepository.Update(ObjPartner, x => x.Id == partnerId);
                    CatPartnerModel model = mapper.Map<CatPartnerModel>(ObjPartner);
                    model.ContractService = GetContractServicesName(objUpdate.SaleService);
                    model.ContractType = objUpdate.ContractType;
                    model.SalesmanId = objUpdate.SaleManId;
                    model.UserCreatedContract = objUpdate.UserCreated;
                    model.OfficeIdContract = objUpdate.OfficeId;
                    SendMailActiveSuccess(model, "active");
                }
            }
            return isUpdateDone;
        }

        public IQueryable<CatContract> CheckExistedContractActive(Guid id, string partnerId)
        {
            var contract = DataContext.Get(x => x.Id == id).FirstOrDefault();
            var ContractActive = DataContext.Where(x => x.Active == true && x.PartnerId == partnerId && x.SaleManId == contract.SaleManId);
            if (ContractActive.Count() == 0)
            {
                return null;
            }
            var IsExisted = ContractActive
                .Any(x => x.SaleManId == contract.SaleManId && x.OfficeId.Intersect(contract.OfficeId).Any() && x.SaleService.Intersect(contract.SaleService).Any());
            if (IsExisted)
            {
                return ContractActive;
            }
            return null;
        }

        public CatContract CheckExistedContractInActive(Guid id, string partnerId, out List<ServiceOfficeGroup> serviceOfficeGrps)
        {
            serviceOfficeGrps = new List<ServiceOfficeGroup>();
            var contract = DataContext.Get(x => x.Id == id).FirstOrDefault();
            var contractOffices = contract.OfficeId.Split(";").ToList();
            var contractServices = contract.SaleService.Split(";").ToList();

            var contractInacActive = DataContext.First(x => x.Active == false && x.PartnerId == partnerId && x.Id != id && x.SaleManId == contract.SaleManId);

            if (contractInacActive == null)
            {
                return null;
            }

            var offices = contractInacActive.OfficeId.Split(";").ToList();
            var services = contractInacActive.SaleService.Split(";").ToList();

            var officeInterset = contractOffices.Intersect(offices);
            var serviceInterset = contractServices.Intersect(services);
            var isExisted = (contractInacActive.SaleManId == contract.SaleManId && officeInterset.Any() && serviceInterset.Any());
            if (isExisted)
            {
                foreach (string service in serviceInterset)
                {
                    foreach (string office in officeInterset)
                    {
                        serviceOfficeGrps.Add(new ServiceOfficeGroup { Office = office, Service = service });
                    }
                }
                return contractInacActive;
            }
            return null;
        }

        public HandleState Import(List<CatContractImportModel> data)
        {
            try
            {
                var contracts = new List<CatContract>();
                foreach (var item in data)
                {
                    DateTime? inactiveDate = DateTime.Now;
                    var contract = mapper.Map<CatContract>(item);
                    contract.UserCreated = contract.UserModified = currentUser.UserID;
                    contract.DatetimeModified = DateTime.Now;
                    contract.DatetimeCreated = DateTime.Now;
                    contract.EffectiveDate = item.EffectDate;
                    contract.ExpiredDate = item.ExpireDate;
                    contract.TrialCreditDays = !string.IsNullOrEmpty(item.PaymentTermTrialDay) ? Convert.ToInt16(item.PaymentTermTrialDay) : (int?)null;
                    contract.PaymentTerm = contract.TrialCreditDays;
                    contract.CompanyId = sysCompanyRepository.Get(x => x.Code == item.Company).Select(t => t.Id)?.FirstOrDefault();
                    var sale = item.SaleService.Split(";").ToArray();
                    contract.SaleService = string.Empty;
                    foreach (var it in sale)
                    {
                        if (it.Trim() == "Air Import")
                        {
                            contract.SaleService += "AI;";
                        }
                        if (it.Trim() == "Air Export")
                        {
                            contract.SaleService += "AE;";
                        }
                        if (it.Trim() == "Sea Consol Export")
                        {
                            contract.SaleService += "SCE;";
                        }
                        if (it.Trim() == "Sea Consol Import")
                        {
                            contract.SaleService += "SCI;";
                        }
                        if (it.Trim() == "Sea FCL Export")
                        {
                            contract.SaleService += "SFE;";
                        }
                        if (it.Trim() == "Sea LCL Export")
                        {
                            contract.SaleService += "SLE;";
                        }
                        if (it.Trim() == "Sea FCL Import")
                        {
                            contract.SaleService += "SFI;";
                        }
                        if (it.Trim() == "Sea LCL Import")
                        {
                            contract.SaleService += "SLI;";
                        }
                        if (it.Trim() == "Custom Logistic")
                        {
                            contract.SaleService += "CL;";
                        }
                        if (it.Trim() == "Trucking")
                        {
                            contract.SaleService += "IT;";
                        }
                        if (it.Trim() == "All")
                        {
                            contract.SaleService = "AI;AE;SCE;SCI;SFE;SLE;SFI;SLI;CL;IT ";
                        }
                    }
                    var vas = item.Vas?.Split(";").ToArray();
                    contract.Vas = string.Empty;
                    if (vas != null)
                    {
                        foreach (var it in vas)
                        {
                            if (it.Trim() == "Air Import")
                            {
                                contract.Vas += "AI;";
                            }
                            if (it.Trim() == "Air Export")
                            {
                                contract.Vas += "AE;";
                            }
                            if (it.Trim() == "Sea Consol Export")
                            {
                                contract.Vas += "SCE;";
                            }
                            if (it.Trim() == "Sea Consol Import")
                            {
                                contract.Vas += "SCI;";
                            }
                            if (it.Trim() == "Sea FCL Export")
                            {
                                contract.Vas += "SFE;";
                            }
                            if (it.Trim() == "Sea LCL Export")
                            {
                                contract.Vas += "SLE;";
                            }
                            if (it.Trim() == "Sea FCL Import")
                            {
                                contract.Vas += "SFI;";
                            }
                            if (it.Trim() == "Sea LCL Import")
                            {
                                contract.Vas += "SLI;";
                            }
                            if (it.Trim() == "Custom Logistic")
                            {
                                contract.Vas += "CL;";
                            }
                            if (it.Trim() == "Trucking")
                            {
                                contract.Vas += "IT;";
                            }
                            if (it.Trim() == "All")
                            {
                                contract.Vas = "AI;AE;SCE;SCI;SFE;SLE;SFI;SLI;CL;IT ";
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(contract.Vas))
                    {
                        if (contract.Vas.Length > 0)
                        {
                            contract.Vas = contract.Vas.Remove(contract.Vas.Length - 1);
                        }
                    }

                    if (!string.IsNullOrEmpty(contract.SaleService))
                    {
                        if (contract.SaleService.Length > 0)
                        {
                            contract.SaleService = contract.SaleService.Remove(contract.SaleService.Length - 1);
                        }
                    }

                    var arrOffice = item.Office.Split(";").ToArray();
                    string officeStr = string.Empty;
                    if (arrOffice.Length > 1)
                    {
                        var dataOffice = sysOfficeRepository.Get().ToList();
                        foreach (var office in dataOffice.GroupBy(x => x.Code))
                        {
                            foreach (var o in arrOffice)
                            {
                                if (o == office.Key)
                                {
                                    officeStr += office.Select(t => t.Id)?.FirstOrDefault() + ";";
                                }
                            }
                        }
                    }
                    contract.OfficeId = arrOffice.Length > 1 ? officeStr.TrimEnd(';') : sysOfficeRepository.Get(x => x.Code == arrOffice[0].ToString()).Select(t => t.Id.ToString())?.FirstOrDefault();
                    contract.SaleManId = sysUserRepository.Get(x => x.Username == item.Salesman).Select(t => t.Id)?.FirstOrDefault();
                    contract.CreditLimit = !string.IsNullOrEmpty(item.CreditLimited) ? Convert.ToDecimal(item.CreditLimited) : (Decimal?)null;
                    if (contract.ContractType == "Trial")
                    {
                        contract.TrialCreditLimited = contract.CreditLimit;
                        contract.TrialEffectDate = contract.EffectiveDate;
                        contract.TrialExpiredDate = contract.ExpiredDate;
                        contract.TrialCreditDays = contract.PaymentTerm;
                    }
                    contract.CreditLimitRate = !string.IsNullOrEmpty(item.CreditLimitedRated) ? Convert.ToInt32(item.CreditLimitedRated) : (int?)null;
                    contract.Active = true;
                    contract.PartnerId = catPartnerRepository.Get(x => x.AccountNo == item.CustomerId).Select(t => t.Id)?.FirstOrDefault();
                    contract.Id = Guid.NewGuid();
                    contracts.Add(contract);
                }
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Add(contracts);
                        if (hs.Success)
                        {
                            trans.Commit();
                        }
                        else
                        {
                            trans.Rollback();
                        }
                        return new HandleState();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        ClearCache();
                        Get();
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public List<CatContractImportModel> CheckValidImport(List<CatContractImportModel> list)
        {
            var officeIds = DataContext.Get().Select(t => t.OfficeId).ToArray();
            var data = list.GroupBy(x => new { x.Office, x.CustomerId }).Where(t => t.Count() > 1).Select(y => y.Key).FirstOrDefault();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.CustomerId))
                {
                    item.CustomerIdError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_PARTNER_ID_EMPTY]);
                    item.IsValid = false;

                }

                else if (!catPartnerRepository.Any(x => x.AccountNo == item.CustomerId))
                {
                    item.CustomerIdError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_PARTNER_ID_NOT_FOUND], item.CustomerId);
                    item.IsValid = false;
                }

                if (string.IsNullOrEmpty(item.ContractType))
                {
                    item.AgreementTypeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_AGREEMENT_TYPE_EMPTY]);
                    item.IsValid = false;
                }

                else if (item.ContractType != "Trial" && item.ContractType != "Official" && item.ContractType != "Guaranteed" && item.ContractType != "Cash")
                {
                    item.AgreementTypeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_AGREEMENT_TYPE_NOT_FOUND], item.ContractType);
                    item.IsValid = false;
                }
                else
                {
                    if (string.IsNullOrEmpty(item.ContractNo) && item.ContractType == "Official")
                    {
                        item.ContractNoError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACTNO_EMPTY]);

                        item.IsValid = false;
                    }

                    if (string.IsNullOrEmpty(item.CreditLimited) && item.ContractType == "Official")
                    {
                        item.CreditLimitError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CREDIT_LIMIT_EMPTY]);
                        item.IsValid = false;
                    }
                    if (string.IsNullOrEmpty(item.PaymentTermTrialDay) && item.ContractType == "Official")
                    {
                        item.PaymentTermError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_PAYMENT_TERM_EMPTY]);
                        item.IsValid = false;
                    }
                }
                var customerId = catPartnerRepository.Get(x => x.AccountNo == item.CustomerId).Select(t => t.Id)?.FirstOrDefault();
                if (!string.IsNullOrEmpty(item.ContractNo))
                {
                    if (DataContext.Any(x => x.PartnerId == customerId && x.ContractNo == item.ContractNo))
                    {
                        item.ContractNoError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACT_NO_EXISTED], item.ContractNo);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.ContractNo == item.ContractNo) > 1)
                    {
                        item.ContractNoError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACT_NO_DUPLICATE], item.ContractNo);
                        item.IsValid = false;
                    }
                    // tạm thời comment, chưa rõ yêu cầu
                    //if (item.ContractNo.Length < 3 || item.ContractNo.Length > 50)
                    //{
                    //    item.ContractNoError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACTNO_LENGTH]);
                    //    item.IsValid = false;
                    //}
                    // 
                }

                if (string.IsNullOrEmpty(item.SaleService))
                {
                    item.SaleServiceError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_SALE_SERVICE_EMPTY]);
                    item.IsValid = false;
                }

                if (string.IsNullOrEmpty(item.Company))
                {
                    item.CompanyError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_COMPANY_EMPTY]);
                    item.IsValid = false;
                }
                else if (!sysCompanyRepository.Any(x => x.Code == item.Company))
                {
                    item.CompanyError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_COMPANY_NOT_FOUND]);
                    item.IsValid = false;
                }
                var officeArr = item.Office.Replace(" ", "").Split(";").ToArray();

                if (string.IsNullOrEmpty(item.Office))
                {
                    item.OfficeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_OFFICE_EMPTY]);
                    item.IsValid = false;
                }

                else if (!sysOfficeRepository.Any(x => officeArr.Contains(x.Code)))
                {
                    item.OfficeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_OFFICE_NOT_FOUND], item.Office);
                    item.IsValid = false;
                }

                if (string.IsNullOrEmpty(item.PaymentMethod))
                {
                    item.PaymentMethodError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_PAYMENT_METHOD_EMPTY]);
                    item.IsValid = false;
                }
                else if (item.PaymentMethod != "All" && item.PaymentMethod != "Collect" && item.PaymentMethod != "Prepaid")
                {
                    item.PaymentMethodError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_PAYMENT_METHOD_NOT_FOUND], item.PaymentMethod);
                    item.IsValid = false;
                }

                if (!string.IsNullOrEmpty(item.Salesman))
                {
                    if (!sysUserRepository.Any(x => x.Username == item.Salesman))
                    {
                        item.SalesmanError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_SALESMAN_NOT_FOUND], item.Salesman);
                        item.IsValid = false;
                    }
                }
                else
                {
                    item.SalesmanError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_SALESMAN_EMPTY]);
                    item.IsValid = false;
                }

                if (!string.IsNullOrEmpty(item.Status))
                {
                    if (item.Status.ToLower() != "active" && item.Status.ToLower() != "inactive")
                    {
                        item.ActiveError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_ACTIVE_NOT_FOUND], item.Status);
                        item.IsValid = false;
                    }
                }

                if (!item.EffectDate.HasValue)
                {
                    item.EffectDateError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_EFFECTIVE_DATE_EMPTY]);
                    item.IsValid = false;
                }


                if (item.EffectDate.HasValue && item.ExpireDate.HasValue)
                {
                    if (item.ExpireDate.Value < item.EffectDate.Value)
                    {
                        item.ExpiredtDateError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_EXPERIED_DATE_NOT_VALID]);
                        item.IsValid = false;
                    }
                }

                if (list.GroupBy(x => new { x.Office, x.CustomerId }).Where(t => t.Count() > 1).Select(y => y.Key).ToList().Count() > 0)
                {
                    if (data.CustomerId == item.CustomerId)
                    {
                        var dataFind = list.Where(x => x.CustomerId == data.CustomerId).ToList();
                        string saleService = string.Empty;
                        foreach (var it in dataFind)
                        {
                            saleService += it.SaleService.Trim() + ";";
                        }
                        var dataDuplicate = saleService.Replace(" ", "").Split(";").ToArray();
                        var dataCheck = dataDuplicate.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                        if (dataCheck.Any() && !string.IsNullOrEmpty(dataCheck.FirstOrDefault()))
                        {
                            item.SalesmanError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_DUPLICATE_SERVICE], item.SaleService);
                            item.IsValid = false;
                        }
                    }
                }
            });
            return list;
        }

        private ListEmailViewModel GetListAccountantAR(string OfficeId, string typeOfActive)
        {
            List<string> lstAccountant = new List<string>();
            List<string> lstCCAccountant = new List<string>();
            List<string> lstAR = new List<string>();
            List<string> lstCCAR = new List<string>();
            ListEmailViewModel EmailModel = new ListEmailViewModel();
            var arrayOffice = OfficeId.Split(";").ToArray();
            int lengthOffice = arrayOffice.Length;

            var DataHeadOffice = sysOfficeRepository.Get(x => x.OfficeType == "Head" && arrayOffice.Contains(x.Id.ToString().ToLower())).FirstOrDefault();
            var DataBranchOffice = sysOfficeRepository.Get(x => x.OfficeType == "Branch" && arrayOffice.Contains(x.Id.ToString().ToLower())).Select(t => t.Id).ToList();

            if (lengthOffice == 1)
            {
                // Get email of accountant, AR
                if (OfficeId == null)
                {
                    lstAccountant = null;
                    lstAR = null;
                }
                else
                {
                    var departmentAccountant = catDepartmentRepository.Get(x => x.DeptType == "ACCOUNTANT" && x.BranchId == new Guid(OfficeId.Replace(";", ""))).FirstOrDefault();
                    var emailSetting = departmentAccountant == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAccountant.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstAccountant = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailAccountant = departmentAccountant?.Email;
                        lstAccountant = listEmailAccountant?.Split(";").ToList();
                    }

                    var departmentAR = catDepartmentRepository.Get(x => x.DeptType == "AR" && x.BranchId == new Guid(OfficeId.Replace(";", ""))).FirstOrDefault();
                    emailSetting = departmentAR == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAR.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstAR = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailAR = departmentAR?.Email;
                        lstAR = listEmailAR?.Split(";").ToList();
                    }
                }

                var DataHeadOfficeAR = sysOfficeRepository.Get(x => x.OfficeType == "Head").FirstOrDefault();
                if (DataHeadOfficeAR == null)
                {
                    lstCCAR = null;
                }
                else
                {
                    var departmentHeadAR = catDepartmentRepository.Get(x => x.DeptType == "AR" && x.BranchId == DataHeadOfficeAR.Id).FirstOrDefault();
                    var emailSetting = departmentHeadAR == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentHeadAR.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstCCAR = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailCCAR = departmentHeadAR?.Email;
                        lstCCAR = listEmailCCAR?.Split(";").ToList();
                    }
                }
            }
            else if (lengthOffice > 1)
            {
                // list mail to Accountant, AR
                if (DataHeadOffice == null)
                {
                    lstAccountant = null;
                    lstAR = null;
                }
                else
                {
                    var departmentAccountant = catDepartmentRepository.Get(x => x.DeptType == "ACCOUNTANT" && x.BranchId == DataHeadOffice.Id).FirstOrDefault();
                    var emailSetting = departmentAccountant == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAccountant.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstAccountant = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailAccountant = departmentAccountant?.Email;
                        lstAccountant = listEmailAccountant?.Split(";").ToList();
                    }

                    var departmentAR = DataHeadOffice == null ? null : catDepartmentRepository.Get(x => x.DeptType == "AR" && x.BranchId == DataHeadOffice.Id).FirstOrDefault();
                    emailSetting = departmentAR == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && x.DeptId == departmentAR.Id).FirstOrDefault()?.EmailInfo;
                    if (!string.IsNullOrEmpty(emailSetting))
                    {
                        lstAR = emailSetting.Split(";").ToList();
                    }
                    else
                    {
                        var listEmailAR = departmentAR?.Email;
                        lstAR = listEmailAR?.Split(";").ToList();
                    }
                }
                // list mail cc Accountant, AR
                if (DataBranchOffice == null)
                {
                    lstCCAccountant = null;
                    lstCCAR = null;
                }
                else
                {
                    var departmentAccountant = catDepartmentRepository.Get(x => x.DeptType == "ACCOUNTANT" && DataBranchOffice.Contains((Guid)x.BranchId)).ToList();
                    if (departmentAccountant.Count() > 0)
                    {
                        var deptId = departmentAccountant.Select(x => x.Id).ToList();
                        var emailSetting = sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && deptId.Any(dept => dept == x.DeptId)).Select(x => x.EmailInfo).ToList();
                        if (emailSetting.Count() > 0)
                        {
                            foreach (var email in emailSetting)
                            {
                                lstCCAccountant.AddRange(email.Split(";").ToList());
                            }
                        }
                        else
                        {
                            var listEmailCCAcountant = string.Join(";", departmentAccountant.Select(x => x.Email).ToArray())?.Split(";").ToList();
                            lstCCAccountant = listEmailCCAcountant;
                        }
                    }
                    else
                    {
                        lstCCAccountant = null;
                    }

                    var departmentAR = catDepartmentRepository.Get(x => x.DeptType == "AR" && DataBranchOffice.Contains((Guid)x.BranchId));
                    if (departmentAR.Count() > 0)
                    {
                        var deptId = departmentAR.Select(x => x.Id).ToList();
                        var emailSetting = sysEmailSettingRepository.Get(x => x.EmailType == typeOfActive && deptId.Any(dept => dept == x.DeptId)).Select(x => x.EmailInfo).ToList();
                        if (emailSetting.Count() > 0)
                        {
                            foreach (var email in emailSetting)
                            {
                                lstCCAR.AddRange(email.Split(";").ToList());
                            }
                        }
                        else
                        {
                            var listEmailCCAR = string.Join(";", departmentAR.Select(x => x.Email).ToArray())?.Split(";").ToList();
                            lstCCAR = listEmailCCAR;
                        }
                    }
                    else
                    {
                        lstCCAR = null;
                    }
                }
            }
            EmailModel.ListAccountant = lstAccountant?.Where(t => !string.IsNullOrEmpty(t)).ToList();
            EmailModel.ListCCAccountant = lstCCAccountant?.Where(t => !string.IsNullOrEmpty(t)).ToList();

            EmailModel.ListAR = lstAR?.Where(t => !string.IsNullOrEmpty(t)).ToList();
            EmailModel.ListCCAR = lstCCAR?.Where(t => !string.IsNullOrEmpty(t)).ToList();

            return EmailModel;
        }

        private void SendMailActiveSuccess(CatPartnerModel partner, string type)
        {
            string employeeId = sysUserRepository.Get(x => x.Id == partner.UserCreatedContract).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoCreator = sysEmployeeRepository.Get(e => e.Id == employeeId)?.FirstOrDefault();
            string employeeIdPartner = sysUserRepository.Get(x => x.Id == partner.UserCreated).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoCreatorPartner = sysEmployeeRepository.Get(e => e.Id == employeeIdPartner)?.FirstOrDefault();
            string FullNameCreatetor = objInfoCreator?.EmployeeNameVn;
            string EnNameCreatetor = objInfoCreator?.EmployeeNameEn;
            string url = string.Empty;
            string employeeIdSalemans = sysUserRepository.Get(x => x.Id == partner.SalesmanId).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoSalesman = sysEmployeeRepository.Get(e => e.Id == employeeIdSalemans)?.FirstOrDefault();
            string employeeIdUserModified = sysUserRepository.Get(x => x.Id == partner.UserModified).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoModified = sysEmployeeRepository.Get(e => e.Id == employeeIdUserModified)?.FirstOrDefault();

            // var requester = sysUserRepository.Get(x => x.Id == currentUser.UserID).Select(t => t.EmployeeId).FirstOrDefault();
            // var mailRequester = sysEmployeeRepository.Get(e => e.Id == requester).FirstOrDefault()?.Email;

            List<string> lstBCc = ListMailCC();
            ListEmailViewModel listEmailViewModel = GetListAccountantAR(partner.OfficeIdContract, DataEnums.EMAIL_TYPE_ACTIVE_CONTRACT);
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
            StringBuilder subject = null;
            StringBuilder body = null;
            //string subject = string.Empty;
            //string body = string.Empty;
            string Title = string.Empty;
            string Name = string.Empty;
            string address = webUrl.Value.Url + "/en/#/" + url + partner.Id;
            string urlToSend = string.Empty;
            string UrlClone = string.Copy(ApiUrl.Value.Url);
            List<string> lstCc = new List<string> { };
            List<string> lstTo = new List<string> { };
            bool resultSendEmail = false;
            if (type == "active")
            {
                var partnerType = string.Empty;
                if (partner.PartnerType == "Customer")
                {
                    partnerType = "Customer";
                    Title = "<i> Your Customer - " + partner.PartnerNameVn + " is active with info below </i> </br>";
                    Name = "\t  Customer Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>";
                }
                else
                {
                    partnerType = "Agent";
                    Title = "<i> Your Agent - " + partner.PartnerNameVn + " is active with info below </i> </br>";
                    Name = "\t  Agent Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>";
                }
                #region change string to using template
                //linkEn = "View more detail, please you <a href='" + address + "'> click here </a>" + "to view detail.";
                //linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";

                //body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color:#004080;'> Dear " + EnNameCreatetor + ", </br> </br>" +
                //    Title +
                //    "<i> Khách hàng - " + partner.PartnerNameVn + " đã được duyệt với thông tin như sau: </i> </br> </br>" +
                //    Name +
                //    "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +
                //    "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + partner.ContractService + "</b>" + "</br>" +
                //    "\t  Contract type  / <i> Loại hợp đồng: </i> " + "<b>" + partner.ContractType + "</b>" + "</br> </br>"
                //    + linkEn + "</br>" + linkVn + "</br> </br>" +
                //    "<i> Thanks and Regards </i>" + "</br> </br>" +
                //    "<b> eFMS System, </b>" +
                //    "</br>"
                //    + "<p><img src = '[logoEFMS]' /></p> " + " </div>");


                //urlToSend = UrlClone.Replace("Catalogue", "");
                //body = body.Replace("[logoEFMS]", urlToSend + "/ReportPreview/Images/logo-eFMS.png");
                #endregion

                // Filling email with template
                var emailTemplate = sysEmailTemplateRepository.Get(x => x.Code == "CONTRACT-ACTIVE")?.FirstOrDefault();
                // Subject
                subject = new StringBuilder(emailTemplate.Subject);
                subject.Replace("{{partnerType}}", partnerType);
                subject.Replace("{{partnerName}}", partner.ShortName);

                // Body
                body = new StringBuilder(emailTemplate.Body);
                urlToSend = UrlClone.Replace("Catalogue", "");
                body.Replace("{{enNameCreatetor}}", EnNameCreatetor);
                body.Replace("{{title}}", Title);
                body.Replace("{{partnerNameVn}}", partner.PartnerNameVn);
                body.Replace("{{name}}", Name);
                body.Replace("{{taxCode}}", partner.TaxCode);
                body.Replace("{{contractService}}", partner.ContractService);
                body.Replace("{{contractType}}", partner.ContractType);
                body.Replace("{{address}}", address);
                body.Replace("{{logoEFMS}}", urlToSend + "/ReportPreview/Images/logo-eFMS.png");

                lstCc = listEmailViewModel.ListAccountant;

                lstTo.Add(objInfoSalesman?.Email);
                //lstTo.Add(objInfoCreatorPartner?.Email);
                lstTo.Add(objInfoCreator?.Email);
                lstTo = lstTo.Where(t => !string.IsNullOrEmpty(t)).ToList();

                resultSendEmail = SendMail.Send(subject.ToString(), body.ToString(), lstTo, null, lstCc, lstBCc);
            }
            else
            {
                #region change string to using template
                //linkEn = "You can <a href='" + address + "'> click here </a>" + "to view detail.";
                //linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
                //subject = "eFMS - Customer Approval Request From " + EnNameCreatetor;

                //body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color:#004080;'> Dear Accountant/AR Team, " + " </br> </br>" +

                //  "<i> You have a Customer Approval request from " + EnNameCreatetor + " as info below </i> </br>" +
                //  "<i> Bạn có một yêu cầu xác duyệt khách hàng từ " + EnNameCreatetor + " với thông tin như sau: </i> </br> </br>" +

                //  "\t  Customer ID  / <i> Mã Agent:</i> " + "<b>" + partner.AccountNo + "</b>" + "</br>" +
                //  "\t  Customer Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
                //  "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +

                //  "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + partner.ContractService + "</b>" + "</br>" +
                //  "\t  Contract type  / <i> Loại hợp đồng: </i> " + "<b>" + partner.ContractType + "</b>" + "</br>" +
                //  "\t  Contract No  / <i> Số hợp đồng: </i> " + "<b>" + partner.ContractNo + "</b>" + "</br>" +
                //  "\t  Requestor  / <i> Người yêu cầu: </i> " + "<b>" + EnNameCreatetor + "</b>" + "</br> </br>"
                // + linkEn + "</br>" + linkVn + "</br> </br>" +
                //  "<i> Thanks and Regards </i>" + "</br> </br>" +
                //  "<b> eFMS System, </b>" +
                //  "</br>"
                // + "<p><img src = '[logoEFMS]' /></p> " + " </div>");

                //urlToSend = UrlClone.Replace("Catalogue", "");
                //body = body.Replace("[logoEFMS]", urlToSend + "/ReportPreview/Images/logo-eFMS.png");
                #endregion
                // Filling email with template
                var emailTemplate = sysEmailTemplateRepository.Get(x => x.Code == "CONTRACT-APPROVEDREQUEST").FirstOrDefault();
                // Subject
                subject = new StringBuilder(emailTemplate.Subject);
                subject.Replace("{{enNameCreatetor}}", EnNameCreatetor);

                // Body
                body = new StringBuilder(emailTemplate.Body);
                urlToSend = UrlClone.Replace("Catalogue", "");
                body.Replace("{{dear}}", partner.ContractType == "Cash" ? "Accountant Team" : "AR Team");
                body.Replace("{{title}}", "Customer");
                body.Replace("{{enNameCreatetor}}", EnNameCreatetor);
                body.Replace("{{accountNo}}", partner.AccountNo);
                body.Replace("{{partnerNameVn}}", partner.PartnerNameVn);
                body.Replace("{{taxCode}}", partner.TaxCode);
                body.Replace("{{contractService}}", partner.ContractService);
                body.Replace("{{contractType}}", partner.ContractType);
                body.Replace("{{contractNo}}", partner.ContractNo);
                body.Replace("{{address}}", address);
                body.Replace("{{logoEFMS}}", urlToSend + "/ReportPreview/Images/logo-eFMS.png");

                if (partner.ContractType == "Cash")
                {
                    lstTo = listEmailViewModel.ListAccountant;
                    if (listEmailViewModel.ListCCAccountant != null)
                    {
                        lstCc.AddRange(listEmailViewModel.ListCCAccountant);
                    }
                }
                else
                {
                    lstTo = listEmailViewModel.ListAR;
                    if (listEmailViewModel.ListCCAR != null)
                    {
                        lstCc.AddRange(listEmailViewModel.ListCCAR);
                    }
                }
                lstCc.Add(objInfoSalesman?.Email);
                //lstCc.Add(objInfoCreatorPartner?.Email);
                //lstCc.Add(objInfoModified?.Email);
                lstCc.Add(objInfoCreator?.Email);
                lstCc = lstCc.Where(t => !string.IsNullOrEmpty(t)).ToList();

                resultSendEmail = SendMail.Send(subject.ToString(), body.ToString(), lstTo, null, lstCc, lstBCc);
            }

            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = lstTo != null ? string.Join("; ", lstTo) : string.Empty,
                Ccs = lstCc != null ? string.Join("; ", lstCc) : string.Empty,
                Bccs = lstBCc != null ? string.Join("; ", lstBCc) : string.Empty,
                Subject = subject.ToString(),
                Sent = resultSendEmail,
                SentDateTime = DateTime.Now,
                Body = body.ToString()
            };

            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();

        }

        public bool SendMailRejectComment(string partnerId, string contractId, string comment, string partnerType)
        {
            var partner = catPartnerRepository.Get(x => x.Id == partnerId).FirstOrDefault();
            var contract = DataContext.Get(x => x.Id == new Guid(contractId)).FirstOrDefault();
            string employeeId = sysUserRepository.Get(x => x.Id == contract.SaleManId).Select(t => t.EmployeeId).FirstOrDefault();
            var salesmanObj = sysEmployeeRepository.Get(e => e.Id == employeeId)?.FirstOrDefault();
            string employeeIdUserCreated = sysUserRepository.Get(x => x.Id == contract.UserCreated).Select(t => t.EmployeeId).FirstOrDefault();
            var userCreatedObj = sysEmployeeRepository.Get(e => e.Id == employeeIdUserCreated)?.FirstOrDefault();
            string urlToSend = string.Empty;
            contract.SaleService = GetContractServicesName(contract.SaleService);

            ListEmailViewModel listEmailViewModel = GetListAccountantAR(contract.OfficeId, string.Empty);

            string subject = string.Empty;
            string linkVn = string.Empty;
            string linkEn = string.Empty;
            string customerName = string.Empty;
            string url = string.Empty;
            string body = string.Empty;
            string UrlClone = string.Copy(ApiUrl.Value.Url);

            #region Change use template
            //if (partnerType == "Customer")
            //{
            //    url = "home/commercial/customer/";
            //    subject = "Reject Agreement Customer - " + partner.PartnerNameVn;
            //    customerName = "\t  Customer Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>";
            //}
            //else
            //{
            //    url = "home/commercial/agent/";
            //    subject = "Reject Agreement Agent - " + partner.PartnerNameVn;
            //    customerName = "\t  Agent Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>";
            //}
            //linkEn = "View more detail, please you <a href='" + address + "'> click here </a>" + "to view detail.";
            //linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
            //body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color:#004080;'> Dear " + salesmanObj.EmployeeNameVn + "," + " </br> </br>" +
            //            "Your Agreement of " + "<b>" + partner.PartnerNameVn + "</b>" + " is rejected by AR/Accountant as info bellow</br>" +
            //            "<i> Khách hàng or thỏa thuận " + partner.PartnerNameVn + " đã bị từ chối với lý do sau: </i> </br></br>" + customerName +
            //            "\t  Taxcode  / <i> Mã số thuế: </i> " + "<b>" + partner.TaxCode + "</b>" + "</br>" +
            //            "\t  Số hợp đồng  / <i> Contract No: </i> " + "<b>" + contract.ContractNo + "</b>" + "</br>" +
            //            "\t  Service  / <i> Dịch vụ: </i> " + "<b>" + contract.SaleService + "</b>" + "</br>" +
            //            "\t  Agreement type  / <i> Loại thỏa thuận: </i> " + "<b>" + contract.ContractType + "</b>" + "</br>" +
            //            "\t  Reason  / <i> Lý do: </i> " + "<b>" + comment + "</b>" + "</br></br>"
            //            + linkEn + "</br>" + linkVn + "</br> </br>" +
            //            "<i> Thanks and Regards </i>" + "</br> </br>" +
            //            "<b> eFMS System, </b>" +
            //            "</br>"
            //            + "<p><img src = '[logoEFMS]' /></p> " + " </div>");
            #endregion

            string address = webUrl.Value.Url + "/en/#/home/commercial/" + (partnerType == "Customer" ? "customer" : "agent") + "/" + partner.Id;
            urlToSend = UrlClone.Replace("Catalogue", "");
            //body = body.Replace("[logoEFMS]", urlToSend + "/ReportPreview/Images/logo-eFMS.png");
            var logoUrl = urlToSend + "/ReportPreview/Images/logo-eFMS.png";

            // Filling email with template
            var emailTemplate = sysEmailTemplateRepository.Get(x => x.Code == "CONTRACT-REJECT").FirstOrDefault();
            // Subject
            subject = emailTemplate.Subject;
            subject = subject.Replace("{{PartnerType}}", partnerType);
            subject = subject.Replace("{{PartnerName}}", partner.PartnerNameVn);

            body = emailTemplate.Body;
            body = body.Replace("{{EmployeeName}}", salesmanObj.EmployeeNameVn);
            body = body.Replace("{{PartnerName}}", partner.PartnerNameVn);
            body = body.Replace("{{TaxCode}}", partner.TaxCode);
            body = body.Replace("{{ContractNo}}", contract.ContractNo);
            body = body.Replace("{{SaleService}}", contract.SaleService);
            body = body.Replace("{{ContractType}}", contract.ContractType);
            body = body.Replace("{{Comment}}", comment);
            body = body.Replace("{{Address}}", address);
            body = body.Replace("{{LogoEFMS}}", logoUrl);

            List<string> lstBCc = ListMailCC();
            List<string> lstTo = new List<string>();
            List<string> lstCC = new List<string>();

            lstTo.Add(salesmanObj?.Email);
            //lstCC.Add(salesmanObj?.Email);
            lstTo.Add(userCreatedObj?.Email);
            lstCC = GetEmailsArAccDepartmentUser();
            lstCC = lstCC.Where(t => !string.IsNullOrEmpty(t)).ToList();
            lstTo = lstTo.Where(t => !string.IsNullOrEmpty(t)).ToList();

            bool result = SendMail.Send(subject, body, lstTo, null, lstCC, lstBCc);
            var logSendMail = new SysSentEmailHistory
            {
               SentUser = SendMail._emailFrom,
               Receivers = lstTo != null ? string.Join("; ", lstTo) : string.Empty,
               Ccs = lstCC != null ? string.Join("; ", lstCC) : string.Empty,
               Bccs = lstBCc != null ? string.Join("; ", lstBCc) : string.Empty,
               Subject = subject,
               Sent = result,
               SentDateTime = DateTime.Now,
               Body = body
            };
            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();
            return result;

        }

        /// <summary>
        /// Get email of dept type ar or acct current user
        /// </summary>
        /// <returns></returns>
        private List<string> GetEmailsArAccDepartmentUser()
        {
            var departmentsUser = userlevelRepository.Get(x => x.UserId == currentUser.UserID).Select(x => x.DepartmentId).ToList();
            var departments = catDepartmentRepository.Get(x => (x.DeptType == "AR" || x.DeptType == "ACCOUNTANT") && departmentsUser.Any(z => z == x.Id));
            if(departments.Count() > 1)
            {
                var department = departments.Where(x => x.Id == currentUser.DepartmentId).FirstOrDefault();
                return (department == null ? new List<string>() : department.Email?.Split(";").ToList());
            }
            else
            {
                return (departments.FirstOrDefault() == null ? new List<string>() : departments.FirstOrDefault().Email?.Split(";").ToList());
            }
        }

        public bool SendMailARConfirmed(string partnerId, string contractId, string partnerType)
        {
            string saleService = string.Empty;
            var partner = catPartnerRepository.Get(x => x.Id == partnerId).FirstOrDefault();
            var contract = DataContext.Get(x => x.Id == new Guid(contractId)).FirstOrDefault();
            string employeeId = sysUserRepository.Get(x => x.Id == contract.UserCreated).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoCreator = sysEmployeeRepository.Get(e => e.Id == employeeId)?.FirstOrDefault();

            string employeeIdSaleman = sysUserRepository.Get(x => x.Id == contract.SaleManId).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoSaleman = sysEmployeeRepository.Get(e => e.Id == employeeIdSaleman)?.FirstOrDefault();

            string FullNameCreatetor = objInfoCreator?.EmployeeNameVn;
            string EnNameCreatetor = objInfoCreator?.EmployeeNameEn;
            saleService = GetContractServicesName(contract.SaleService);
            contract.Arconfirmed = true;
            contract.DatetimeModified = DateTime.Now;
            var hs = DataContext.Update(contract, x => x.Id == new Guid(contractId), false);
            DataContext.SubmitChanges();
            string url = string.Empty;
            string urlToSend = string.Empty;

            List<string> lstBCc = ListMailCC();
            List<string> lstTo = new List<string>();
            List<string> lstCc = new List<string>();
            string UrlClone = string.Copy(ApiUrl.Value.Url);

            // info send to and cc
            ListEmailViewModel listEmailViewModel = GetListAccountantAR(contract.OfficeId, DataEnums.EMAIL_TYPE_ACTIVE_CONTRACT);
            lstTo = listEmailViewModel.ListAccountant;
            lstCc = listEmailViewModel.ListCCAccountant;

            string linkVn = string.Empty;
            string linkEn = string.Empty;
            //string subject = string.Empty;
            //string body = string.Empty;
            string Title = string.Empty;
            string Name = string.Empty;

            if (partnerType == "Customer")
            {
                url = "home/commercial/customer/";
            }
            else
            {
                url = "home/commercial/agent/";
            }

            string address = webUrl.Value.Url + "/en/#/" + url + partner.Id;


            linkEn = "You can <a href='" + address + "'> click here </a>" + "to view detail.";
            linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";

            #region change string to using template
            //subject = "eFMS - Partner Confirm Credit Term Request From " + FullNameCreatetor;

            //body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color:#004080;'> Dear Accountant/AR Team, " + " </br> </br>" +

            //  "<i> You have a Partner Confirm Credit Term request From " + FullNameCreatetor + " as info below </i> </br>" +
            //  "<i> Bạn có một yêu cầu xác duyệt đối tượng từ " + FullNameCreatetor + " với thông tin như sau: </i> </br> </br>" +

            //  "\t  Partner ID  / <i> Mã đối tượng:</i> " + "<b>" + partner.AccountNo + "</b>" + "</br>" +
            //  "\t  Partner Name  / <i> Tên đối tượng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
            //  "\t  Partner Type  / <i> Loại Partner:</i> " + "<b>" + partner.PartnerGroup + "</b>" + "</br>" +
            //  "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +
            //  "\t  Address / <i> Địa chỉ: </i>" + "<b>" + partner.AddressVn + "</b>" + "</br>" +
            //  "\t  Requestor / <i> Người yêu cầu: </i>" + "<b>" + EnNameCreatetor + "</b>" + "</br>" +

            //  "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + saleService + "</b>" + "</br>" +
            //  "\t  Agreement  type  / <i> Loại thỏa thuận: </i> " + "<b>" + contract.ContractType + "</b>" + "</br>" +
            //  "\t  Contract No  / <i> Số hợp đồng: </i> " + "<b>" + contract.ContractNo + "</b>" + "</br></br>"

            //  + linkEn + "</br>" + linkVn + "</br> </br>" +
            //  "<i> Thanks and Regards </i>" + "</br> </br>" +
            //  "<b> eFMS System, </b>" +
            //  "</br>"
            //  + "<p><img src = '[logoEFMS]' /></p> " + " </div>");

            //urlToSend = UrlClone.Replace("Catalogue", "");
            //body = body.Replace("[logoEFMS]", urlToSend + "/ReportPreview/Images/logo-eFMS.png");
            #endregion

            // Filling email with template
            var emailTemplate = sysEmailTemplateRepository.Get(x => x.Code == "PARTNER-ACTIVE").FirstOrDefault();
            // Subject
            var subject = new StringBuilder(emailTemplate.Subject);
            subject.Replace("{{fullNameCreatetor}}", FullNameCreatetor);

            var body = new StringBuilder(emailTemplate.Body);
            urlToSend = UrlClone.Replace("Catalogue", "");
            body.Replace("{{fullNameCreatetor}}", FullNameCreatetor);
            body.Replace("{{accountNo}}", partner.AccountNo);
            body.Replace("{{partnerNameVn}}", partner.PartnerNameVn);
            body.Replace("{{partnerGroup}}", partner.PartnerGroup);
            body.Replace("{{taxCode}}", partner.TaxCode);
            body.Replace("{{addressVn}}", partner.AddressVn);
            body.Replace("{{enNameCreatetor}}", EnNameCreatetor);
            body.Replace("{{saleService}}", saleService);
            body.Replace("{{contractType}}", contract.ContractType);
            body.Replace("{{contractNo}}", contract.ContractNo);
            body.Replace("{{address}}", address);
            body.Replace("{{logoEFMS}}", urlToSend + "/ReportPreview/Images/logo-eFMS.png");

            lstCc.Add(objInfoCreator?.Email);
            lstCc.Add(objInfoSaleman?.Email);
            lstCc = lstCc.Where(t => !string.IsNullOrEmpty(t)).ToList();

            bool result = SendMail.Send(subject.ToString(), body.ToString(), lstTo, null, lstCc, lstBCc);

            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = lstTo != null ? string.Join("; ", lstTo) : string.Empty,
                Ccs = lstCc != null ? string.Join("; ", lstCc) : string.Empty,
                Bccs = lstBCc != null ? string.Join("; ", lstBCc) : string.Empty,
                Subject = subject.ToString(),
                Sent = result,
                SentDateTime = DateTime.Now,
                Body = body.ToString()
            };
            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();
            return result;
        }

        private List<string> ListMailCC()
        {
            var emailBcc = ((eFMSDataContext)DataContext.DC).ExecuteFuncScalar("[dbo].[fn_GetEmailBcc]");
            List<string> emailBCCs = new List<string>();
            if (emailBcc != null)
            {
                emailBCCs = emailBcc.ToString().Split(";").ToList();
            }
            return emailBCCs;
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
            string path = this.ApiUrl.Value.Url;
            try
            {
                var list = new List<SysImage>();
                /* Kiểm tra các thư mục có tồn tại */
                var hs = new HandleState();
                ImageHelper.CreateDirectoryFile(string.Empty, model.PartnerId);
                List<SysImage> resultUrls = new List<SysImage>();
                fileName = model.Files.FileName.Replace("+", "_");
                string objectId = model.PartnerId;
                await ImageHelper.SaveFile(fileName, string.Empty, objectId, model.Files);
                string urlImage = path + "/files/" + objectId + "/" + fileName;
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
                var hs = await ImageHelper.DeleteFile(item.ObjectId + "\\" + item.Name, string.Empty);
            }
            return result;
        }

        public IQueryable<CatAgreementModel> QueryAgreement(CatContractCriteria criteria)
        {
            IQueryable<CatAgreementModel> results = null;
            IQueryable<SysUser> sysUser = sysUserRepository.Get();
            IQueryable<SysEmployee> employees = sysEmployeeRepository.Get();

            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {

                CatPartner partnerAcRef = catPartnerRepository.Get(x => x.Id == criteria.PartnerId).FirstOrDefault();
                if (partnerAcRef != null)
                {
                    var partnerId = criteria.IsGetChild == true ? partnerAcRef.Id : partnerAcRef.ParentId;
                    IQueryable <CatContract> catContracts = DataContext.Get().Where(x => x.PartnerId == partnerId && x.Active == (criteria.Status ?? true));

                    var queryContracts = from contract in catContracts
                                         join users in sysUser on contract.SaleManId equals users.Id
                                         join employee in employees on users.EmployeeId equals employee.Id
                                         select new { contract, users, employee };

                    if (queryContracts.Count() == 0) return null;
                    if (queryContracts.Count() > 0)
                    {
                        results = queryContracts.Select(x => new CatAgreementModel
                        {
                            ID = x.contract.Id,
                            SaleManId = x.contract.SaleManId,
                            SaleManName = x.employee.EmployeeNameEn,
                            ContractNo = x.contract.ContractNo,
                            ExpiredDate = x.contract.ExpiredDate,
                            ContractType = x.contract.ContractType,
                            CustomerAdvanceAmountVnd = x.contract.CustomerAdvanceAmountVnd ?? 0,
                            CreditCurrency = x.contract.CreditCurrency,
                            CurrencyId = x.contract.CurrencyId,
                            CustomerAdvanceAmountUsd = x.contract.CustomerAdvanceAmountUsd,
                        }).OrderBy(x => x.ExpiredDate);
                    }
                }

            }

            return results;
        }

        public CatContract GetContractById(Guid Id)
        {
            return DataContext.Get(x => x.Id == Id)?.FirstOrDefault();
        }
    }
}
