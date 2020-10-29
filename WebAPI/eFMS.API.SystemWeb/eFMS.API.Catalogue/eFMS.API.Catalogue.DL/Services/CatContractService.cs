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
        private readonly IContextBase<CatDepartment> catDepartmentRepository;
        private readonly IContextBase<CsTransaction> transactionRepository;
        private readonly IContextBase<OpsTransaction> opsRepository;
        private readonly IContextBase<SysSentEmailHistory> sendEmailHistoryRepository;
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
            IContextBase<CatDepartment> catDepartmentRepo,
            IContextBase<CsTransaction> transactionRepo,
            IContextBase<OpsTransaction> opsRepo,
            IContextBase<SysSentEmailHistory> sendEmailHistoryRepo,
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
                if (company != null)
                {
                    saleman.CompanyNameAbbr = company.BunameAbbr;
                    saleman.CompanyNameEn = company.BunameEn;
                    saleman.CompanyNameVn = company.BunameVn;
                }
                var officeIds = saleman.OfficeId.Split(";").ToList();
                if (officeIds.Count() > 0)
                {
                    foreach (var officeId in officeIds)
                    {
                        saleman.OfficeNameAbbr += sysOfficeRepository.Get(x => x.Id == new Guid(officeId)).Select(t => t.ShortName).FirstOrDefault() + "; ";
                    }
                }
                if (saleman.OfficeNameAbbr.Length > 0)
                {
                    saleman.OfficeNameAbbr = saleman.OfficeNameAbbr.Remove(saleman.OfficeNameAbbr.Length - 2);
                }
                saleman.SaleServiceName = GetContractServicesName(saleman.SaleService);
                saleman.Username = item.user.Username;
                results.Add(saleman);
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
                data = DataContract.Where(x => x.PartnerId == partnerId && x.OfficeId.Contains(currentUser.OfficeID.ToString()) && x.SaleService.Contains("CL")).Select(x => x.SaleManId).FirstOrDefault();
                if (string.IsNullOrEmpty(data))
                {
                    string IdAcRefPartner = catPartnerRepository.Get(x => x.Id == partnerId).Select(t => t.ParentId).FirstOrDefault();
                    data = DataContract.Where(x => x.PartnerId == IdAcRefPartner && IdAcRefPartner != null && x.OfficeId.Contains(currentUser.OfficeID.ToString()) && x.SaleService.Contains("CL")).Select(x => x.SaleManId).FirstOrDefault();
                    OfficeNameAbbr = sysOfficeRepository.Get(x => x.Id == currentUser.OfficeID).Select(t => t.ShortName).FirstOrDefault();

                }
            }
            else
            {
                data = DataContract.Where(x => x.PartnerId == partnerId && x.OfficeId.Contains(DataShipment.OfficeId.ToString()) && x.SaleService.Contains(DataShipment.TransactionType)).Select(x => x.SaleManId).FirstOrDefault();
                if (string.IsNullOrEmpty(data))
                {
                    string IdAcRefPartner = catPartnerRepository.Get(x => x.Id == partnerId).Select(t => t.ParentId).FirstOrDefault();
                    data = DataContract.Where(x => x.PartnerId == IdAcRefPartner && IdAcRefPartner != null && x.OfficeId.Contains(DataShipment.OfficeId.ToString()) && x.SaleService.Contains(DataShipment.TransactionType)).Select(x => x.SaleManId).FirstOrDefault();
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
            var hs = DataContext.Add(contract, false);
            DataContext.SubmitChanges();
            if (hs.Success)
            {
                //if (entity.IsRequestApproval == true)
                //{
                var ObjPartner = catPartnerRepository.Get(x => x.Id == entity.PartnerId).FirstOrDefault();
                CatPartnerModel model = mapper.Map<CatPartnerModel>(ObjPartner);
                model.ContractService = entity.SaleService;

                model.ContractService = GetContractServicesName(model.ContractService);

                model.ContractType = entity.ContractType;
                model.ContractNo = entity.ContractNo;
                model.SalesmanId = entity.SaleManId;
                model.UserCreatedContract = contract.UserCreated;
                SendMailActiveSuccess(model, string.Empty);
                //}
                ClearCache();
                Get();
            }
            return hs;
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
                            ContractServicesName = "Air Export; Air Import; Sea FCL Export; Sea LCL Export; Sea LCL Import; Custom Logistic; Trucking ";
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

        public HandleState Update(CatContractModel model)
        {
            var entity = mapper.Map<CatContract>(model);
            entity.UserModified = currentUser.UserID;
            entity.DatetimeModified = DateTime.Now;
            var currentContract = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
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
                    ClearCache();
                    Get();
                    SendMailActiveSuccess(modelPartner, string.Empty);
                }
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

        public HandleState ActiveInActiveContract(Guid id, string partnerId, SalesmanCreditModel credit)
        {
            var isUpdateDone = new HandleState();
            var objUpdate = DataContext.First(x => x.Id == id);
            if (objUpdate != null)
            {
                objUpdate.Active = objUpdate.Active == true ? false : true;
                if (credit.CreditLimit != null)
                {
                    objUpdate.CreditLimit = credit.CreditLimit;
                }
                if (credit.CreditRate != null)
                {
                    objUpdate.CreditLimitRate = credit.CreditRate;
                }
                isUpdateDone = DataContext.Update(objUpdate, x => x.Id == objUpdate.Id, false);
                objUpdate.DatetimeModified = DateTime.Now;
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
                    SendMailActiveSuccess(model, "active");
                }
            }
            return isUpdateDone;
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
                            contract.SaleService = "AE;SFE;SLE;SFI;SLI;CL;IT";
                        }
                    }
                    var vas = item.Vas.Split(";").ToArray();
                    contract.Vas = string.Empty;
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
                            contract.Vas = "AE;SFE;SLE;SFI;SLI;CL;IT";
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
                    contract.Active = item.Status == "Active" ? true : false;
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
                    if (string.IsNullOrEmpty(item.ContractNo) && (item.ContractType == "Trial" || item.ContractType == "Official"))
                    {
                        item.ContractNoError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACTNO_EMPTY]);

                        item.IsValid = false;
                    }
                    if (!item.ExpireDate.HasValue && (item.ContractType == "Trial" || item.ContractType == "Official"))
                    {
                        item.ExpiredtDateError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_EXPERIED_DATE_EMPTY]);
                        item.IsValid = false;

                    }
                    if (!item.EffectDate.HasValue && (item.ContractType == "Trial" || item.ContractType == "Official"))
                    {
                        item.EffectDateError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_EFFECTIVE_DATE_EMPTY]);
                        item.IsValid = false;
                    }
                    if (string.IsNullOrEmpty(item.CreditLimited) && (item.ContractType == "Trial" || item.ContractType == "Official"))
                    {
                        item.CreditLimitError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CREDIT_LIMIT_EMPTY]);
                        item.IsValid = false;
                    }
                    if (string.IsNullOrEmpty(item.PaymentTermTrialDay) && (item.ContractType == "Trial" || item.ContractType == "Official"))
                    {
                        item.PaymentTermError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_PAYMENT_TERM_EMPTY]);
                        item.IsValid = false;
                    }
                }
                if (!string.IsNullOrEmpty(item.ContractNo))
                {
                    var customerId = catPartnerRepository.Get(x => x.AccountNo == item.CustomerId).Select(t => t.Id)?.FirstOrDefault();
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

                    // start tạm thời comment, chưa rõ yêu cầu

                    //if (item.ContractNo.Length < 3 || item.ContractNo.Length > 50)
                    //{
                    //    item.ContractNoError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACTNO_LENGTH]);
                    //    item.IsValid = false;
                    //}

                    // end 
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
                var officeArr = item.Office.Split(";").ToArray();

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

                if (!string.IsNullOrEmpty(item.Status))
                {
                    if (item.Status.ToLower() != "active" && item.Status.ToLower() != "inactive")
                    {
                        item.ActiveError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_ACTIVE_NOT_FOUND], item.Status);
                        item.IsValid = false;
                    }
                }

                if (item.EffectDate.HasValue && item.ExpireDate.HasValue)
                {
                    if (item.ExpireDate.Value < item.EffectDate.Value)
                    {
                        item.ExpiredtDateError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_EXPERIED_DATE_NOT_VALID]);
                        item.IsValid = false;
                    }
                }
            });
            return list;
        }

        private void SendMailActiveSuccess(CatPartnerModel partner, string type)
        {
            string employeeId = sysUserRepository.Get(x => x.Id == partner.UserCreatedContract).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoCreator = sysEmployeeRepository.Get(e => e.Id == employeeId)?.FirstOrDefault();
            string FullNameCreatetor = objInfoCreator?.EmployeeNameVn;
            string EnNameCreatetor = objInfoCreator?.EmployeeNameEn;
            string url = string.Empty;
            string employeeIdSalemans = sysUserRepository.Get(x => x.Id == partner.SalesmanId).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoSalesman = sysEmployeeRepository.Get(e => e.Id == employeeIdSalemans)?.FirstOrDefault();
            List<string> lstBCc = ListMailCC();
            List<string> lstTo = new List<string>();

            // info send to and cc
            var listEmailAR = catDepartmentRepository.Get(x => x.DeptType == "AR" && x.BranchId == currentUser.OfficeID)?.Select(t => t.Email).FirstOrDefault();

            if (listEmailAR != null && listEmailAR.Any())
            {
                lstTo = listEmailAR.Split(";").ToList();
            }

            string emailCreator = objInfoCreator?.Email;

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
            string Title = string.Empty;
            string Name = string.Empty;
            string address = webUrl.Value.Url + "/en/#/" + url + partner.Id;
            List<string> lstCc = new List<string>
            {
            };
            bool resultSendEmail = false;
            if (type == "active")
            {
                if (partner.PartnerType == "Customer")
                {
                    subject = "Actived Customer - " + partner.ShortName;
                    Title = "<i> Your Customer - " + partner.PartnerNameVn + " is active with info below </i> </br>";
                    Name = "\t  Customer Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>";
                }
                else
                {
                    subject = "Actived Agent - " + partner.ShortName;
                    Title = "<i> Your Agent - " + partner.PartnerNameVn + " is active with info below </i> </br>";
                    Name = "\t  Agent Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>";
                }
                linkEn = "View more detail, please you <a href='" + address + "'> click here </a>" + "to view detail.";
                linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";

                body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'> Dear " + EnNameCreatetor + ", </br> </br>" +
                    Title +
                    "<i> Khách hàng - " + partner.PartnerNameVn + " đã được duyệt với thông tin như sau: </i> </br> </br>" +
                    Name +
                    "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +
                    "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + partner.ContractService + "</b>" + "</br>" +
                    "\t  Contract type  / <i> Loại hợp đồng: </i> " + "<b>" + partner.ContractType + "</b>" + "</br> </br>"
                    + linkEn + "</br>" + linkVn + "</br> </br>" +
                    "<i> Thanks and Regards </i>" + "</br> </br>" +
                    "eFMS System </div>");
           
                if (lstTo.Any())
                {
                    lstCc = lstTo;
                }
                lstCc.Add(objInfoSalesman?.Email);
                resultSendEmail = SendMail.Send(subject, body, lstTo, null, lstCc, lstBCc);
            }
            else
            {
                linkEn = "You can <a href='" + address + "'> click here </a>" + "to view detail.";
                linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
                subject = "eFMS - Customer Approval Request From " + EnNameCreatetor;

                body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'> Dear Accountant/AR Team, " + " </br> </br>" +

                  "<i> You have a Customer Approval request from " + EnNameCreatetor + " as info below </i> </br>" +
                  "<i> Bạn có một yêu cầu xác duyệt khách hàng từ " + EnNameCreatetor + " với thông tin như sau: </i> </br> </br>" +

                  "\t  Customer ID  / <i> Mã Agent:</i> " + "<b>" + partner.AccountNo + "</b>" + "</br>" +
                  "\t  Customer Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
                  "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +

                  "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + partner.ContractService + "</b>" + "</br>" +
                  "\t  Contract type  / <i> Loại hợp đồng: </i> " + "<b>" + partner.ContractType + "</b>" + "</br>" +
                  "\t  Contract No  / <i> Số hợp đồng: </i> " + "<b>" + partner.ContractNo + "</b>" + "</br>" +
                  "\t  Requestor  / <i> Người yêu cầu: </i> " + "<b>" + EnNameCreatetor + "</b>" + "</br> </br>"

                  + linkEn + "</br>" + linkVn + "</br> </br>" +
                  "<i> Thanks and Regards </i>" + "</br> </br>" +
                  "eFMS System </div>");

                lstCc.Add(objInfoSalesman?.Email);
                //SendMail.Send(subject, body, lstTo, null, lstCc, lstBCc);
                resultSendEmail = SendMail.Send(subject, body, lstTo, null, lstCc, lstBCc);
               
            }

            var logSendMail = new SysSentEmailHistory
            {
                Receivers = string.Join("; ", lstTo),
                Ccs = string.Join("; ", lstCc),
                Subject = subject,
                Sent = resultSendEmail,
                SentDateTime = DateTime.Now,
                Body = body
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
            contract.SaleService = GetContractServicesName(contract.SaleService);

            string subject = string.Empty;
            string linkVn = string.Empty;
            string linkEn = string.Empty;
            string customerName = string.Empty;
            string url = string.Empty;
            string body = string.Empty;
            if (partnerType == "Customer")
            {
                url = "home/commercial/customer/";
                subject = "Reject Agreement Customer - " + partner.PartnerNameVn;
                customerName = "\t  Customer Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>";
            }
            else
            {
                url = "home/commercial/agent/";
                subject = "Reject Agreement Agent - " + partner.PartnerNameVn;
                customerName = "\t  Agent Name  / <i> Tên khách hàng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>";
            }
            string address = webUrl.Value.Url + "/en/#/" + url + partner.Id;
            linkEn = "View more detail, please you <a href='" + address + "'> click here </a>" + "to view detail.";
            linkVn = "Bạn click <a href='" + address + "'> vào đây </a>" + "để xem chi tiết.";
            body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'> Dear " + salesmanObj.EmployeeNameVn + "," + " </br> </br>" +
                        "Your Agreement of " + "<b>" + partner.PartnerNameVn + "</b>" + " is rejected by AR/Accountant as info bellow</br>" +
                        "<i> Khách hàng or thỏa thuận " + partner.PartnerNameVn + " đã bị từ chối với lý do sau: </i> </br></br>" + customerName +
                        "\t  Taxcode  / <i> Mã số thuế: </i> " + "<b>" + partner.TaxCode + "</b>" + "</br>" +
                        "\t  Số hợp đồng  / <i> Contract No: </i> " + "<b>" + contract.ContractNo + "</b>" + "</br>" +
                        "\t  Service  / <i> Dịch vụ: </i> " + "<b>" + contract.SaleService + "</b>" + "</br>" +
                        "\t  Agreement type  / <i> Loại thỏa thuận: </i> " + "<b>" + contract.ContractType + "</b>" + "</br>" +
                        "\t  Reason  / <i> Lý do: </i> " + "<b>" + comment + "</b>" + "</br></br>"
                         + linkEn + "</br>" + linkVn + "</br> </br>" +
                        "<i> Thanks and Regards </i>" + "</br> </br>" +
                       "eFMS System </div>");
            List<string> lstCc = ListMailCC();
            List<string> lstTo = new List<string>();

            lstTo.Add(salesmanObj?.Email);

            //return SendMail.Send(subject, body, lstTo, null, null, lstCc);
            bool result = SendMail.Send(subject, body, lstTo, null, null, lstCc);
            var logSendMail = new SysSentEmailHistory
            {
                Receivers = string.Join("; ", lstTo),
                Ccs = string.Join("; ", lstCc),
                Subject = subject,
                Sent = result,
                SentDateTime = DateTime.Now,
                Body = body
            };
            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();
            return result;

        }

        public bool SendMailARConfirmed(string partnerId, string contractId, string partnerType)
        {
            string saleService = string.Empty;
            var partner = catPartnerRepository.Get(x => x.Id == partnerId).FirstOrDefault();
            var contract = DataContext.Get(x => x.Id == new Guid(contractId)).FirstOrDefault();
            string employeeId = sysUserRepository.Get(x => x.Id == contract.UserCreated).Select(t => t.EmployeeId).FirstOrDefault();
            var objInfoCreator = sysEmployeeRepository.Get(e => e.Id == employeeId)?.FirstOrDefault();
            string FullNameCreatetor = objInfoCreator?.EmployeeNameVn;
            string EnNameCreatetor = objInfoCreator?.EmployeeNameEn;
            saleService = GetContractServicesName(contract.SaleService);
            contract.Arconfirmed = true;
            contract.DatetimeModified = DateTime.Now;
            var hs  = DataContext.Update(contract, x => x.Id == new Guid( contractId),false);
            DataContext.SubmitChanges();
            string url = string.Empty;

            List<string> lstBCc = ListMailCC();
            List<string> lstTo = new List<string>();

            // info send to and cc
            var listEmailAR = catDepartmentRepository.Get(x => x.DeptType == "ACCOUNTANT" && x.BranchId == currentUser.OfficeID)?.Select(t => t.Email).FirstOrDefault();

            if (listEmailAR != null && listEmailAR.Any())
            {
                lstTo = listEmailAR.Split(";").ToList();
            }

            string linkVn = string.Empty;
            string linkEn = string.Empty;
            string subject = string.Empty;
            string body = string.Empty;
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
            subject = "eFMS - Partner Confirm Credit Term Request From " + FullNameCreatetor;

            body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'> Dear Accountant/AR Team, " + " </br> </br>" +

              "<i> You have a Partner Confirm Credit Term request From " + FullNameCreatetor + " as info below </i> </br>" +
              "<i> Bạn có một yêu cầu xác duyệt đối tượng từ " + FullNameCreatetor + " với thông tin như sau: </i> </br> </br>" +

              "\t  Partner ID  / <i> Mã đối tượng:</i> " + "<b>" + partner.AccountNo + "</b>" + "</br>" +
              "\t  Partner Name  / <i> Tên đối tượng:</i> " + "<b>" + partner.PartnerNameVn + "</b>" + "</br>" +
              "\t  Partner Type  / <i> Loại Partner:</i> " + "<b>" + partner.PartnerGroup + "</b>" + "</br>" +
              "\t  Taxcode / <i> Mã số thuế: </i>" + "<b>" + partner.TaxCode + "</b>" + "</br>" +
              "\t  Address / <i> Địa chỉ: </i>" + "<b>" + partner.AddressVn + "</b>" + "</br>" +
              "\t  Requestor / <i> Người yêu cầu: </i>" + "<b>" + partner.AddressVn + "</b>" + "</br>" +

              "\t  Service  / <i> Dịch vụ: </i>" + "<b>" + saleService + "</b>" + "</br>" +
              "\t  Agreement  type  / <i> Loại thỏa thuận: </i> " + "<b>" + contract.ContractType + "</b>" + "</br>" +
              "\t  Contract No  / <i> Số hợp đồng: </i> " + "<b>" + contract.ContractNo + "</b>" + "</br></br>" +

              linkEn + "</br>" + linkVn + "</br> </br>" +
              "<i> Thanks and Regards </i>" + "</br> </br>" +
              "eFMS System </div>");
            bool result =  SendMail.Send(subject, body, lstTo, null, null, lstBCc);

            var logSendMail = new SysSentEmailHistory
            {
                Receivers = string.Join("; ", lstTo),
                Ccs = string.Join("; ", lstBCc),
                Subject = subject,
                Sent = result,
                SentDateTime = DateTime.Now,
                Body = body
            };
            var hsLogSendMail = sendEmailHistoryRepository.Add(logSendMail);
            var hsSm = sendEmailHistoryRepository.SubmitChanges();
            return result;
        }

        private List<string> ListMailCC()
        {
            List<string> lstCc = new List<string>
            {
                "alex.phuong@itlvn.com",
                "luis.quang@itlvn.com",
                "andy.hoa@itlvn.com",
                "cara.oanh@itlvn.com",
                "lynne.loc@itlvn.com",
                "samuel.an@logtechub.com",
                "kenny.thuong@itlvn.com",
            };
            return lstCc;
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
                ImageHelper.CreateDirectoryFile(model.FolderName, model.PartnerId);
                List<SysImage> resultUrls = new List<SysImage>();
                fileName = model.Files.FileName;
                string objectId = model.PartnerId;
                await ImageHelper.SaveFile(fileName, model.FolderName, objectId, model.Files);
                string urlImage = path + "/" + model.FolderName + "/files/" + objectId + "/" + fileName;
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
