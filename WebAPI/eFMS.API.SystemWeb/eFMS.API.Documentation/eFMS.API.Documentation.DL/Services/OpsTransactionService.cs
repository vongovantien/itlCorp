using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.IService;
using eFMS.API.Common.Models;
using eFMS.API.Common;

namespace eFMS.API.Documentation.DL.Services
{
    public class OpsTransactionService : RepositoryBase<OpsTransaction, OpsTransactionModel>, IOpsTransactionService
    {
        //private ICatStageApiService catStageApi;
        //private ICatPlaceApiService catplaceApi;
        //private ICatPartnerApiService catPartnerApi;
        //private ISysUserApiService sysUserApi;
        private readonly ICurrentUser currentUser;
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsShipmentSurchargeService surchargeService;
        private readonly IContextBase<CatPartner> partnerRepository;
        private readonly IContextBase<SysUser> userRepository;
        private readonly IContextBase<CatUnit> unitRepository;
        private readonly IContextBase<CatPlace> placeRepository;
        private readonly IContextBase<OpsStageAssigned> opsStageAssignedRepository;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        private readonly IContextBase<CustomsDeclaration> customDeclarationRepository;
        private readonly IContextBase<AcctCdnote> acctCdNoteRepository;
        private readonly IContextBase<CsMawbcontainer> csMawbcontainerRepository;
        private readonly IContextBase<CatCommodity> commodityRepository;
        private readonly ICsMawbcontainerService mawbcontainerService;
        readonly IUserPermissionService permissionService;
        readonly IContextBase<CatCurrencyExchange> currencyExchangeRepository;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<SysOffice> sysOfficeRepo;
        readonly IContextBase<SysUserLevel> userlevelRepository;

        public OpsTransactionService(IContextBase<OpsTransaction> repository, 
            IMapper mapper, 
            ICurrentUser user, 
            IStringLocalizer<LanguageSub> localizer, 
            ICsShipmentSurchargeService surcharge, 
            IContextBase<CatPartner> partner, 
            IContextBase<SysUser> userRepo,
            IContextBase<CatUnit> unitRepo,
            IContextBase<CatPlace> placeRepo,
            IContextBase<OpsStageAssigned> opsStageAssignedRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<CustomsDeclaration> customDeclarationRepo,
            IContextBase<AcctCdnote> acctCdNoteRepo,
            IContextBase<CsMawbcontainer> csMawbcontainerRepo,
            ICsMawbcontainerService containerService, 
            IUserPermissionService perService,
            IContextBase<CatCurrencyExchange> currencyExchangeRepo,
            IContextBase<CatCommodity> commodityRepo,
            ICurrencyExchangeService currencyExchange,
            IContextBase<SysOffice> sysOffice,
            IContextBase<SysUserLevel> userlevelRepo) : base(repository, mapper)
        {
            //catStageApi = stageApi;
            //catplaceApi = placeApi;
            //catPartnerApi = partnerApi;
            //sysUserApi = userApi;
            currentUser = user;
            stringLocalizer = localizer;
            surchargeService = surcharge;
            partnerRepository = partner;
            userRepository = userRepo;
            unitRepository = unitRepo;
            placeRepository = placeRepo;
            opsStageAssignedRepository = opsStageAssignedRepo;
            surchargeRepository = surchargeRepo;
            customDeclarationRepository = customDeclarationRepo;
            acctCdNoteRepository = acctCdNoteRepo;
            csMawbcontainerRepository = csMawbcontainerRepo;
            mawbcontainerService = containerService;
            permissionService = perService;
            currencyExchangeRepository = currencyExchangeRepo;
            currencyExchangeService = currencyExchange;
            commodityRepository = commodityRepo;
            sysOfficeRepo = sysOffice;
            userlevelRepository = userlevelRepo;
        }
        public override HandleState Add(OpsTransactionModel model)
        {
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403);
            model.Id = Guid.NewGuid();
            model.DatetimeCreated = DateTime.Now;
            model.UserCreated = currentUser.UserID;
            model.DatetimeModified = model.DatetimeCreated;
            model.UserModified = model.UserCreated;
            model.CurrentStatus = "InSchedule";
            model.GroupId = currentUser.GroupId;
            model.DepartmentId = currentUser.DepartmentId;
            model.OfficeId = currentUser.OfficeID;
            model.CompanyId = currentUser.CompanyID;
            var customer = partnerRepository.Get(x => x.Id == model.CustomerId).FirstOrDefault();
            var dataUserLevels = userlevelRepository.Get(x => x.UserId == model.SalemanId).ToList();
            string SalesGroupId = string.Empty;
            string SalesDepartmentId = string.Empty;
            string SalesOfficeId = string.Empty;
            string SalesCompanyId = string.Empty;
            if (dataUserLevels.Select(t => t.GroupId).Count() >= 1)
            {
                var dataGroup = dataUserLevels.Where(x => x.OfficeId == currentUser.OfficeID).ToList();
                if (dataGroup.Any())
                {
                    SalesGroupId = String.Join(";", dataGroup.Select(t => t.GroupId).Distinct());
                    SalesDepartmentId = String.Join(";", dataGroup.Select(t => t.DepartmentId).Distinct());
                    SalesOfficeId = String.Join(";", dataGroup.Select(t => t.OfficeId).Distinct());
                    SalesCompanyId = String.Join(";", dataGroup.Select(t => t.CompanyId).Distinct());
                }
                else
                {
                    SalesGroupId = String.Join(";", dataUserLevels.Select(t => t.GroupId).Distinct());
                    SalesDepartmentId = String.Join(";", dataUserLevels.Select(t => t.DepartmentId).Distinct());
                    SalesOfficeId = String.Join(";", dataUserLevels.Select(t => t.OfficeId).Distinct());
                    SalesCompanyId = String.Join(";", dataUserLevels.Select(t => t.CompanyId).Distinct());
                }

            }

            model.SalesGroupId = !string.IsNullOrEmpty(SalesGroupId) ? SalesGroupId : null;
            model.SalesDepartmentId = !string.IsNullOrEmpty(SalesDepartmentId) ? SalesDepartmentId : null;
            model.SalesOfficeId = !string.IsNullOrEmpty(SalesOfficeId) ? SalesOfficeId : null;
            model.SalesCompanyId = !string.IsNullOrEmpty(SalesCompanyId) ? SalesCompanyId : null;
            //if(customer != null)
            //{
            //    model.SalemanId = customer.SalePersonId;
            //}
            var dayStatus = (int)(model.ServiceDate.Value.Date - DateTime.Now.Date).TotalDays;
            if(dayStatus > 0)
            {
                model.CurrentStatus = TermData.InSchedule;
            }
            else
            {
                model.CurrentStatus = TermData.Processing;
            }
            
            model.JobNo = CreateJobNoOps();
            var entity = mapper.Map<OpsTransaction>(model);
            return DataContext.Add(entity);
        }

        public string CreateJobNoOps()
        {
            SysOffice office = null;
            string prefixJob = string.Empty;
            var currentUserOffice = currentUser?.OfficeID ?? null;
            if (currentUserOffice != null)
            {
                office = sysOfficeRepo.Get(x => x.Id == currentUserOffice).FirstOrDefault();
                prefixJob = SetPrefixJobIdByOfficeCode(office?.Code);
            }
            prefixJob += DocumentConstants.OPS_SHIPMENT;
            var currentShipment = GetOpsTransactionToGenerateJobNo(office);
            int countNumberJob = 0;
            if (currentShipment != null)
            {
                countNumberJob = Convert.ToInt32(currentShipment.JobNo.Substring(prefixJob.Length + 5, 5));
            }
            return GenerateID.GenerateJobID(prefixJob, countNumberJob);
        }

        private OpsTransaction GetOpsTransactionToGenerateJobNo(SysOffice office)
        {
            OpsTransaction currentShipment = null;
            if (office != null)
            {
                if (office.Code == "ITLHAN")
                {
                    currentShipment = DataContext.Get(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                         && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                         && x.JobNo.StartsWith("HAN"))
                                                         .OrderByDescending(x => x.JobNo).FirstOrDefault();
                }
                else if (office.Code == "ITLDAD")
                {
                    currentShipment = DataContext.Get(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                         && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                         && x.JobNo.StartsWith("DAD"))
                                                         .OrderByDescending(x => x.JobNo).FirstOrDefault();
                }
                else
                {
                    currentShipment = DataContext.Get(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                         && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                         && !x.JobNo.StartsWith("DAD")
                                                         && !x.JobNo.StartsWith("HAN"))
                                                         .OrderByDescending(x => x.JobNo).FirstOrDefault();
                }
            }
            else
            {
                currentShipment = DataContext.Get(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                     && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                     && !x.JobNo.StartsWith("DAD")
                                                     && !x.JobNo.StartsWith("HAN"))
                                                     .OrderByDescending(x => x.JobNo).FirstOrDefault();
            }
            return currentShipment;
        }

        private string SetPrefixJobIdByOfficeCode(string officeCode)
        {
            string prefixCode = string.Empty;
            if (!string.IsNullOrEmpty(officeCode))
            {
                if (officeCode == "ITLHAN")
                {
                    prefixCode = "HAN-";
                }
                else if (officeCode == "ITLDAD")
                {
                    prefixCode = "DAD-";
                }
            }
            return prefixCode;
        }

        public int CheckDetailPermission(Guid id)
        {
            var detail = GetBy(id);
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Detail);
            int code = GetPermissionToUpdate(new ModelUpdate { BillingOpsId = detail.BillingOpsId, SaleManId = detail.SalemanId, UserCreated = detail.UserCreated, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId }, permissionRange);
            return code;
        }
        private OpsTransactionModel GetBy(Guid id)
        {
            var details = Get(x => x.Id == id && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED).FirstOrDefault();

            if (details != null)
            {
                CatPartner agent = partnerRepository.Get(x => x.Id == details.AgentId).FirstOrDefault();
                details.AgentName = agent?.PartnerNameEn;

                CatPartner supplier = partnerRepository.Get(x => x.Id == details.SupplierId).FirstOrDefault();
                details.SupplierName = supplier?.PartnerNameEn;

                CatPartner customer = partnerRepository.Get(x => x.Id == details.CustomerId).FirstOrDefault();
                details.CustomerName = customer?.ShortName;

                details.UserCreatedName = userRepository.Get(x => x.Id == details.UserCreated).FirstOrDefault()?.Username;
                details.UserModifiedName = userRepository.Get(x => x.Id == details.UserModified).FirstOrDefault()?.Username;
            }
            return details;
        }
        public OpsTransactionModel GetDetails(Guid id)
        {
            var detail = GetBy(id);
            if (detail == null) return null;
            List<string> authorizeUserIds = permissionService.GetAuthorizedIds("CL", currentUser);
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Delete);
            detail.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionDetail(permissionRangeWrite, authorizeUserIds, detail),
                AllowDelete = GetPermissionDetail(permissionRangeDelete, authorizeUserIds, detail)
            };
            var specialActions = currentUser.UserMenuPermission.SpecialActions;
            detail.Permission = PermissionEx.GetSpecialActions(detail.Permission, specialActions);
            return detail;
        }

        private bool GetPermissionDetail(PermissionRange permissionRangeWrite, List<string> authorizeUserIds, OpsTransactionModel detail)
        {
            bool result = false;
            switch (permissionRangeWrite)
            {
                case PermissionRange.All:
                    result = true;
                    break;
                case PermissionRange.Owner:
                    if (detail.BillingOpsId == currentUser.UserID || detail.UserCreated == currentUser.UserID || authorizeUserIds.Contains(detail.BillingOpsId))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Group:
                    if ((detail.GroupId == currentUser.GroupId && detail.DepartmentId == currentUser.DepartmentId && detail.OfficeId == currentUser.OfficeID && detail.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(detail.BillingOpsId) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Department:
                    if ((detail.DepartmentId == currentUser.DepartmentId && detail.OfficeId == currentUser.OfficeID && detail.CompanyId == currentUser.CompanyID) || authorizeUserIds.Contains(detail.BillingOpsId) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Office:
                    if ((detail.OfficeId == currentUser.OfficeID && detail.CompanyId == currentUser.CompanyID) || authorizeUserIds.Contains(detail.BillingOpsId) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Company:
                    if (detail.CompanyId == currentUser.CompanyID || authorizeUserIds.Contains(detail.BillingOpsId) || detail.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
            }
            return result;
        }

        public OpsTransactionResult Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            criteria.RangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
            var data = Query(criteria);
            int totalProcessing = 0;
            int totalfinish = 0;
            int totalOverdued = 0;
            if (data == null) rowsCount = 0;
            else
            {
                rowsCount = data.Select(x => x.Id).Count();
                totalProcessing = data.Count(x => x.CurrentStatus == TermData.Processing);
                totalfinish = data.Count(x => x.CurrentStatus == TermData.Finish);
                totalOverdued = data.Count(x => x.CurrentStatus == TermData.Overdue);
            }
            int totalCanceled = 0;            
            totalCanceled = DataContext.Count(x => x.CurrentStatus == TermData.Canceled && x.ServiceDate >= criteria.ServiceDateFrom && x.ServiceDate <= criteria.ServiceDateTo); //data.Count(x => x.CurrentStatus == DataTypeEx.GetJobStatus(JobStatus.Canceled));
            if (rowsCount == 0) return null;
            if (size > 1)
            {
                data = data.OrderByDescending(x => x.DatetimeModified);
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
                IQueryable<CatPartner> customers = partnerRepository.Get(x => x.PartnerGroup.Contains("CUSTOMER"));
                IQueryable<CatPlace> ports = placeRepository.Get(x => x.PlaceTypeId == "Port");

                data.ToList().ForEach(x => {
                    x.ClearanceNo = customDeclarationRepository.Get(cus => cus.JobNo == x.JobNo).OrderBy(cus => cus.ClearanceDate).ThenBy(cus => cus.ClearanceNo)
                    .Select(cus => cus.ClearanceNo).FirstOrDefault();
                    x.CustomerName = customers.FirstOrDefault(cus => cus.Id == x.CustomerId)?.ShortName;
                    x.POLName = ports.FirstOrDefault(pol => pol.Id == x.Pol)?.NameEn;
                    x.PODName = ports.FirstOrDefault(pod => pod.Id == x.Pod)?.NameEn;
                    
                    IQueryable<SysUser> sysUsers = userRepository.Get(u => u.Id == x.UserCreated);

                    x.UserCreatedName = sysUsers?.FirstOrDefault()?.Username;
                });
            }
            var results = new OpsTransactionResult
            {
                OpsTransactions = data,
                ToTalInProcessing = totalProcessing,
                ToTalFinish = totalfinish,
                TotalOverdued = totalOverdued,
                TotalCanceled = totalCanceled
            };
            return results;
        }
        public bool CheckAllowDelete(Guid jobId)
        {
            var detail = DataContext.Get(x => x.Id == jobId && x.CurrentStatus != TermData.Canceled)?.FirstOrDefault();
            if (detail == null) return false;
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Delete);
            //int code = GetPermissionToDelete(new ModelUpdate { BillingOpsId = detail.BillingOpsId, UserCreated = detail.UserCreated, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId }, permissionRange);
            var model = new ModelUpdate { BillingOpsId = detail.BillingOpsId, UserCreated = detail.UserCreated, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId };
            int code = PermissionEx.GetPermissionToDelete(model, permissionRange, currentUser);
            if (code == 403) return false;
            var query = surchargeRepository.Get(x => x.Hblid == detail.Id && (x.CreditNo != null || x.DebitNo != null || x.Soano != null || x.PaymentRefNo != null));
            if (query.Any())
            {
                return false;
            }
            return true;
        }
        public IQueryable<OpsTransaction> QueryByPermission(PermissionRange range)
        {
            IQueryable<OpsTransaction> data = null;
            List<string> authorizeUserIds = permissionService.GetAuthorizedIds("CL", currentUser);
            switch (range)
            {
                case PermissionRange.All:
                    data = DataContext.Get(x => x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null);
                    break;
                case PermissionRange.Owner:
                    data = DataContext.Get(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && (x.BillingOpsId == currentUser.UserID || x.SalemanId == currentUser.UserID
                                                 || authorizeUserIds.Contains(x.BillingOpsId) || authorizeUserIds.Contains(x.SalemanId)
                                                 || x.UserCreated == currentUser.UserID));
                    break;
                case PermissionRange.Group:
                    data = DataContext.Get(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && ((x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId)
                                                || x.UserCreated == currentUser.UserID));
                    break;
                case PermissionRange.Department:
                    data = DataContext.Get(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && ((x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId)
                                                || x.UserCreated == currentUser.UserID));
                    break;
                case PermissionRange.Office:
                    data = DataContext.Get(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && ((x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId) 
                                                || x.UserCreated == currentUser.UserID));
                    break;
                case PermissionRange.Company:
                    data = DataContext.Get(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && (x.CompanyId == currentUser.CompanyID || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId) 
                                                || x.UserCreated == currentUser.UserID));
                    break;
            }
            return data;
        }
        public IQueryable<OpsTransactionModel> Query(OpsTransactionCriteria criteria)
        {
            if (criteria.RangeSearch == PermissionRange.None) return null;
            var data = QueryByPermission(criteria.RangeSearch);
            if (data == null)
                return null;
            List<OpsTransactionModel> results = new List<OpsTransactionModel>();
            IQueryable<OpsTransaction> datajoin = data.Where(x => x.CurrentStatus != TermData.Canceled);
            if (criteria.ClearanceNo != null)
            {
                var listCustomsDeclaration = customDeclarationRepository.Get(x => x.ClearanceNo.ToLower().Contains(criteria.ClearanceNo.ToLower()));
                if(listCustomsDeclaration.Count() > 0)
                {
                    datajoin = from custom in listCustomsDeclaration
                               join datas in data on custom.JobNo equals datas.JobNo
                               select datas;
                    if(datajoin.Count() > 1)
                    {
                        datajoin = datajoin.GroupBy(x => x.JobNo).SelectMany(x => x).AsQueryable();
                    }
                }
                else
                {
                    return results.AsQueryable();
                }
            }
            if(criteria.CreditDebitInvoice != null)
            {
                var listDebit = acctCdNoteRepository.Get(x => x.Code.ToLower().Contains(criteria.CreditDebitInvoice.ToLower()));
                if(listDebit.Count() > 0)
                {
                    datajoin = from acctnote in listDebit
                               join datas in data on acctnote.JobId equals datas.Id
                               select datas;
                    if (datajoin.Count() > 1)
                    {
                        datajoin = datajoin.GroupBy(x => x.JobNo).SelectMany(x => x).AsQueryable();
                    }
                }
                else
                {
                    return results.AsQueryable(); 
                }

            }
            
            if (criteria.All == null)
            {
                datajoin = datajoin.Where(x => (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.Hwbno ?? "").IndexOf(criteria.Hwbno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.Mblno ?? "").IndexOf(criteria.Mblno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.ProductService ?? "").IndexOf(criteria.ProductService ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.ServiceMode ?? "").IndexOf(criteria.ServiceMode ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.CustomerId == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                                && (x.FieldOpsId == criteria.FieldOps || string.IsNullOrEmpty(criteria.FieldOps))
                                && (x.ShipmentMode == criteria.ShipmentMode || string.IsNullOrEmpty(criteria.ShipmentMode))
                                && ((x.ServiceDate ?? null) >= criteria.ServiceDateFrom || criteria.ServiceDateFrom == null)
                                && ((x.ServiceDate ?? null) <= criteria.ServiceDateTo || criteria.ServiceDateTo == null)
                            ).OrderByDescending(x => x.DatetimeModified);
            }
            else
            {
                datajoin = datajoin.Where(x => (x.JobNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Hwbno ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Mblno ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ProductService ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ServiceMode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.CustomerId == criteria.All || string.IsNullOrEmpty(criteria.All))
                                   || (x.FieldOpsId == criteria.All || string.IsNullOrEmpty(criteria.All))
                                   || (x.ShipmentMode == criteria.All || string.IsNullOrEmpty(criteria.All))
                               && ((x.ServiceDate ?? null) >= (criteria.ServiceDateFrom ?? null) && (x.ServiceDate ?? null) <= (criteria.ServiceDateTo ?? null))
                               ).OrderByDescending(x => x.DatetimeModified);
            }
            results = mapper.Map<List<OpsTransactionModel>>(datajoin);
            return results.AsQueryable();
        }
        
        private string SetProductServiceShipment(CustomsDeclarationModel model)
        {
            string productService = string.Empty;
            if (model.ServiceType == "Sea")
            {
                if (model.CargoType == "FCL")
                {
                    productService = "SeaFCL";
                }
                else
                {
                    productService = "SeaLCL";
                }
            }
            else
            {
                if(model.CargoType != null && model.ServiceType == null)
                {
                    model.ServiceType = "Sea";
                    if (model.CargoType == "FCL")
                    {
                        productService = "SeaFCL";
                    }
                    else
                    {
                        productService = "SeaLCL";
                    }
                }
                else
                {
                    productService = model.ServiceType;
                }
            }
            return productService;
        }

        public HandleState ConvertClearanceToJob(CustomsDeclarationModel model)
        {
            var result = new HandleState();
            try
            {
                var existedMessage = CheckExist(model.Mblid, model.Hblid);
                if (existedMessage != null)
                {
                    return new HandleState(existedMessage);
                }
                if (CheckExistClearance(model, model.Id))
                {
                    result = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CLEARANCENO_EXISTED, model.ClearanceNo].Value);
                    return result;
                }
                string productService = SetProductServiceShipment(model);
                if (model.CargoType == null && model.ServiceType == "Sea")
                {
                    result = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CLEARANCE_CARGOTYPE_NOT_ALLOW_EMPTY].Value);
                    return result;
                }
                if (productService == null)
                {
                    result = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CLEARANCE_CARGOTYPE_MUST_HAVE_SERVICE_TYPE].Value);
                    return result;
                }
                
                var opsTransaction = GetNewShipmentToConvert(productService, model);
                opsTransaction.JobNo = CreateJobNoOps();
                DataContext.Add(opsTransaction, false);

                if (model.Id > 0)
                {
                    var clearance = UpdateInfoConvertClearance(model.Id);
                    clearance.JobNo = opsTransaction.JobNo;
                    customDeclarationRepository.Update(clearance, x => x.Id == clearance.Id, false);
                }
                else
                {
                    var clearance = GetNewClearanceModel(model);
                    clearance.JobNo = opsTransaction.JobNo;
                    customDeclarationRepository.Add(clearance, false);
                }
                DataContext.SubmitChanges();
                customDeclarationRepository.SubmitChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        private OpsTransaction GetNewShipmentToConvert(string productService, CustomsDeclarationModel model)
        {
            var opsTransaction = new OpsTransaction
            {
                Id = Guid.NewGuid(),
                Hblid = Guid.NewGuid(),
                ProductService = productService,
                ServiceMode = model.Type,
                ShipmentMode = "External",
                Mblno = model.Mblid,
                Hwbno = model.Hblid,
                SumContainers = model.QtyCont,
                ServiceDate = model.ClearanceDate,
                SumGrossWeight = model.GrossWeight,
                SumNetWeight = model.NetWeight,
                SumCbm = model.Cbm,
                Shipper = model.Shipper,
                Consignee = model.Consignee,
                BillingOpsId = currentUser.UserID,
                GroupId = currentUser.GroupId,
                DepartmentId = currentUser.DepartmentId,
                OfficeId = currentUser.OfficeID,
                CompanyId = currentUser.CompanyID,
                DatetimeCreated = DateTime.Now,
                UserCreated = currentUser.UserID, //currentUser.UserID;
                DatetimeModified = DateTime.Now,
                UserModified = currentUser.UserID,
                ShipmentType = "Freehand",
            };
            var customer = partnerRepository.Get(x => x.TaxCode == model.PartnerTaxCode).FirstOrDefault();
            if (customer != null)
            {
                opsTransaction.CustomerId = customer.Id;
                opsTransaction.SalemanId = customer.SalePersonId;
            }
            var port = placeRepository.Get(x => x.Code == model.Gateway).FirstOrDefault();
            if (port != null)
            {
                if (model.Type == "Export")
                {
                    opsTransaction.Pol = port.Id;
                    opsTransaction.ClearanceLocation = port.Id;
                }
                if (model.Type == "Import")
                {
                    opsTransaction.Pod = port.Id;
                    opsTransaction.ClearanceLocation = port.Id;
                }
            }

            var unit = unitRepository.Get(x => x.Code == model.UnitCode).FirstOrDefault();
            if (unit != null)
            {
                opsTransaction.SumPackages = model.Pcs;
                opsTransaction.PackageTypeId = unit.Id;
            }
            var commodity = commodityRepository.Get(x => x.Code == model.CommodityCode).FirstOrDefault();
            if (commodity != null)
            {
                opsTransaction.CommodityGroupId = commodity.CommodityGroupId;
            }
            var dayStatus = (int)(opsTransaction.ServiceDate.Value.Date - DateTime.Now.Date).TotalDays;
            if (dayStatus > 0)
            {
                opsTransaction.CurrentStatus = TermData.InSchedule;
            }
            else
            {
                opsTransaction.CurrentStatus = TermData.Processing;
            }
            return opsTransaction;
        }

        private CustomsDeclaration GetNewClearanceModel(CustomsDeclarationModel model)
        {
            var clearance = mapper.Map<CustomsDeclaration>(model);
            clearance.ConvertTime = DateTime.Now;
            clearance.DatetimeCreated = DateTime.Now;
            clearance.DatetimeModified = DateTime.Now;
            clearance.UserCreated = model.UserModified = currentUser.UserID;
            clearance.Source = DocumentConstants.CLEARANCE_FROM_EFMS;
            clearance.GroupId = currentUser.GroupId;
            clearance.DepartmentId = currentUser.DepartmentId;
            clearance.OfficeId = currentUser.OfficeID;
            clearance.CompanyId = currentUser.CompanyID;
            return clearance;
        }

        private bool CheckExistClearance(CustomsDeclarationModel model, decimal id)
        {
            if (id == 0)
            {
                if (customDeclarationRepository.Any(x => x.ClearanceNo == model.ClearanceNo && x.ClearanceDate == model.ClearanceDate))
                {
                    return true;
                }
            }
            else
            {
                if (customDeclarationRepository.Any(x => (x.ClearanceNo == model.ClearanceNo && x.Id != id && x.ClearanceDate == model.ClearanceDate)))
                {
                    return true;
                }
            }
            return false;
        }

        public HandleState ConvertExistedClearancesToJobs(List<CustomsDeclarationModel> list)
        {
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403);
            var result = new HandleState();
            try
            {
                int i = 0;
                foreach (var item in list)
                {
                    var existedMessage = CheckExist(item.Mblid, item.Hblid);
                    if (existedMessage != null)
                    {
                        return new HandleState(existedMessage);
                    }

                    if (CheckExistClearance(item, item.Id))
                    {
                        result = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CLEARANCENO_EXISTED, item.ClearanceNo].Value);
                        return result;
                    }
                    if (item.JobNo == null)
                    {
                        var model = new BaseUpdateModel { UserCreated = item.UserCreated, GroupId = item.GroupId, DepartmentId = item.DepartmentId, OfficeId = item.OfficeId, CompanyId = item.CompanyId };
                        var code = PermissionExtention.GetPermissionCommonItem(model, permissionRange, currentUser);
                        if (code == 403) return new HandleState(403);
                        string productService = SetProductServiceShipment(item);
                        if (item.CargoType == null && item.ServiceType == "Sea")
                        {
                            result = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CLEARANCE_CARGOTYPE_NOT_ALLOW_EMPTY].Value);
                            return result;
                        }
                        if (productService == null)
                        {
                            result = new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_CLEARANCE_CARGOTYPE_MUST_HAVE_SERVICE_TYPE].Value);
                            return result;
                        }
                        var opsTransaction = GetNewShipmentToConvert(productService, item);
                        opsTransaction.JobNo = CreateJobNoOps(); //Generate JobNo [17/12/2020]
                        DataContext.Add(opsTransaction);
                        CustomsDeclaration clearance = UpdateInfoConvertClearance(item.Id);
                        clearance.JobNo = opsTransaction.JobNo;
                        customDeclarationRepository.Update(clearance, x => x.Id == clearance.Id);
                        i = i + 1;
                    }
                }
                DataContext.SubmitChanges();
                customDeclarationRepository.SubmitChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        private CustomsDeclaration UpdateInfoConvertClearance(int id)
        {
            var clearance = customDeclarationRepository.Get(x => x.Id == id).First();
            clearance.UserModified = currentUser.UserID;
            clearance.DatetimeModified = DateTime.Now;
            clearance.ConvertTime = DateTime.Now;
            clearance.GroupId = currentUser.GroupId;
            clearance.DepartmentId = currentUser.DepartmentId;
            clearance.OfficeId = currentUser.OfficeID;
            clearance.CompanyId = currentUser.CompanyID;
            return clearance;
        }

        public HandleState SoftDeleteJob(Guid id)
        {
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None) return new HandleState(403);
            var result = new HandleState();
            var job = DataContext.First(x => x.Id == id && x.CurrentStatus != TermData.Canceled);
            if (job == null)
            {
                result = new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);
            }
            else
            {
                var model = new ModelUpdate { BillingOpsId = job.BillingOpsId, UserCreated = job.UserCreated, CompanyId = job.CompanyId, OfficeId = job.OfficeId, DepartmentId = job.DepartmentId, GroupId = job.GroupId };
                int code = PermissionEx.GetPermissionToDelete(model, permissionRange, currentUser);
                if (code == 403) return new HandleState(403);
                job.CurrentStatus = TermData.Canceled;
                job.DatetimeModified = DateTime.Now;
                job.UserModified = currentUser.UserID;
                result = DataContext.Update(job, x => x.Id == id,false);
                if (result.Success)
                {
                    var clearances = customDeclarationRepository.Get(x => x.JobNo == job.JobNo);
                    if (clearances != null)
                    {
                        foreach(var item in clearances)
                        {
                            item.JobNo = null;
                            item.ConvertTime = null;
                            customDeclarationRepository.Update(item, x => x.Id == item.Id, false);
                        }
                    }
                }
            }
            DataContext.SubmitChanges();
            customDeclarationRepository.SubmitChanges();
            return result;
        }
        public string CheckExist(OpsTransactionModel model)
        {
            var existedHBL = DataContext.Any(x => x.Id != model.Id && x.Hwbno == model.Hwbno && x.CurrentStatus != TermData.Canceled);
            var existedMBL = DataContext.Any(x => x.Id != model.Id && x.Mblno == model.Mblno && x.CurrentStatus != TermData.Canceled);
            if (existedHBL)
            {
                return stringLocalizer[DocumentationLanguageSub.MSG_HBNO_EXISTED, model.Hwbno].Value;
            }
            if (existedMBL)
            {
                return stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED, model.Mblno].Value;
            }
            return null;
        }

        public string CheckExist(string mblNo, string hblNo)
        {
            var existedHBL = DataContext.Any(x => x.Hwbno == hblNo && x.CurrentStatus != TermData.Canceled);
            var existedMBL = DataContext.Any(x => x.Mblno == mblNo && x.CurrentStatus != TermData.Canceled);
            if (existedHBL)
            {
                return stringLocalizer[DocumentationLanguageSub.MSG_HBNO_EXISTED, hblNo].Value;
            }
            if (existedMBL)
            {
                return stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED, mblNo].Value;
            }
            return null;
        }

        public Crystal PreviewFormPLsheet(Guid id, string currency)
        {
            double _doubleNumber = 0.000000001;
            var shipment = DataContext.First(x => x.Id == id);
            Crystal result = null;
            var parameter = new FormPLsheetReportParameter
            {
                Contact = currentUser.UserName,
                CompanyName = "CompanyName",
                CompanyDescription = "CompanyDescription",
                CompanyAddress1 = "CompanyAddress1",
                CompanyAddress2 = "CompanyAddress2",
                Website = "Website",
                CurrDecimalNo = 3,
                DecimalNo = 0,
                HBLList = shipment.Hwbno
            };

            result = new Crystal
            {
                ReportName = "FormPLsheet.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            var dataSources = new List<FormPLsheetReport>{};
            var agent = partnerRepository.Get(x => x.Id == shipment.AgentId).FirstOrDefault();
            var supplier = partnerRepository.Get(x => x.Id == shipment.SupplierId).FirstOrDefault();
            var surcharges = surchargeService.GetByHB(shipment.Hblid);
            var user = userRepository.Get(x => x.Id == shipment.SalemanId).FirstOrDefault();
            var units = unitRepository.Get();
            var polName = placeRepository.Get(x => x.Id == shipment.Pol).FirstOrDefault()?.NameEn;
            var podName = placeRepository.Get(x => x.Id == shipment.Pod).FirstOrDefault()?.NameEn;
            if(surcharges != null)
            {
                foreach(var item in surcharges)
                {
                    var unitCode = units.FirstOrDefault(x => x.Id == item.UnitId)?.Code;
                    bool isOBH = false;
                    decimal cost = 0;
                    decimal revenue = 0;
                    decimal costNonVat = 0;
                    decimal revenueNonVat = 0;
                    decimal saleProfitIncludeVAT = 0;
                    decimal saleProfitNonVAT = 0;
                    string partnerName = string.Empty;
                    if (item.Type == DocumentConstants.CHARGE_OBH_TYPE)
                    {
                        isOBH = true;
                        partnerName = item.PayerName;
                        cost = item.Total;
                    }
                    if(item.Type == DocumentConstants.CHARGE_BUY_TYPE)
                    {
                        cost = item.Total;
                        costNonVat = (item.Quantity * item.UnitPrice) ?? 0;
                    }
                    if(item.Type == DocumentConstants.CHARGE_SELL_TYPE)
                    {
                        revenue = item.Total;
                        revenueNonVat = (item.Quantity * item.UnitPrice) ?? 0;
                    }
                    saleProfitIncludeVAT = cost + revenue;
                    saleProfitNonVAT = costNonVat + revenueNonVat;

                    decimal _exchangeRateUSD = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, DocumentConstants.CURRENCY_USD);
                    decimal _exchangeRateLocal = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL);

                    var surchargeRpt = new FormPLsheetReport();

                    surchargeRpt.COSTING = "COSTING Test";
                    surchargeRpt.TransID = shipment.JobNo?.ToUpper();
                    surchargeRpt.TransDate = (DateTime)shipment.DatetimeCreated;
                    surchargeRpt.HWBNO = shipment.Hwbno?.ToUpper();
                    surchargeRpt.MAWB = shipment.Mblno?.ToUpper();
                    surchargeRpt.PartnerName = "PartnerName";
                    surchargeRpt.ContactName = user?.Username;
                    surchargeRpt.ShipmentType = "Logistics";
                    surchargeRpt.NominationParty = string.Empty;
                    surchargeRpt.Nominated = true;
                    surchargeRpt.POL = polName?.ToUpper();
                    surchargeRpt.POD = podName?.ToUpper();
                    surchargeRpt.Commodity = string.Empty;
                    surchargeRpt.Volumne = string.Empty;
                    surchargeRpt.Carrier = supplier?.PartnerNameEn?.ToUpper();
                    surchargeRpt.Agent = agent?.PartnerNameEn?.ToUpper();
                    surchargeRpt.ContainerNo = item.ContNo;
                    surchargeRpt.OceanVessel = string.Empty;
                    surchargeRpt.LocalVessel = string.Empty;
                    surchargeRpt.FlightNo = shipment.FlightVessel?.ToUpper();
                    surchargeRpt.SeaImpVoy = string.Empty;
                    surchargeRpt.LoadingDate = ((DateTime)shipment.ServiceDate).ToString("dd' 'MMM' 'yyyy");
                    surchargeRpt.ArrivalDate = shipment.FinishDate != null ? ((DateTime)shipment.FinishDate).ToString("dd' 'MM' 'yyyy") : null;
                    surchargeRpt.FreightCustomer = "FreightCustomer";
                    surchargeRpt.FreightColoader = 128;
                    surchargeRpt.PayableAccount = item.PartnerName?.ToUpper();
                    surchargeRpt.Description = item.ChargeNameEn;
                    surchargeRpt.Curr = item.CurrencyId;
                    surchargeRpt.VAT = item.Vatrate ?? 0;
                    surchargeRpt.VATAmount = 12;
                    surchargeRpt.Cost = cost + (decimal)_doubleNumber; //Phí chi của charge
                    surchargeRpt.Revenue = revenue + (decimal)_doubleNumber;
                    surchargeRpt.Exchange = currency == DocumentConstants.CURRENCY_USD ? _exchangeRateUSD * saleProfitIncludeVAT : _exchangeRateUSD * saleProfitIncludeVAT; //Exchange phí của charge về USD
                    surchargeRpt.Exchange = surchargeRpt.Exchange + (decimal)_doubleNumber; //Cộng thêm phần thập phân
                    surchargeRpt.VNDExchange = currency == DocumentConstants.CURRENCY_LOCAL ? _exchangeRateLocal : _exchangeRateUSD;
                    surchargeRpt.VNDExchange = surchargeRpt.VNDExchange + (decimal)_doubleNumber; //Cộng thêm phần thập phân
                    surchargeRpt.Paid = (revenue > 0 || cost < 0) && isOBH == false ? false : true;
                    surchargeRpt.DatePaid = DateTime.Now;
                    surchargeRpt.Docs = !string.IsNullOrEmpty(item.InvoiceNo) ? item.InvoiceNo : (item.CreditNo ?? item.DebitNo); //Ưu tiên: InvoiceNo >> CD Note Code  of charge
                    surchargeRpt.Notes = item.Notes;
                    surchargeRpt.InputData = "InputData";
                    surchargeRpt.SalesProfit = currency == DocumentConstants.CURRENCY_USD ? _exchangeRateUSD * saleProfitNonVAT : _exchangeRateLocal * saleProfitNonVAT; //Non VAT
                    surchargeRpt.SalesProfit = surchargeRpt.SalesProfit + (decimal)_doubleNumber; //Cộng thêm phần thập phân
                    surchargeRpt.Quantity = item.Quantity;
                    surchargeRpt.UnitPrice = item.UnitPrice ?? 0;
                    surchargeRpt.UnitPrice = surchargeRpt.UnitPrice + (decimal)_doubleNumber; //Cộng thêm phần thập phân
                    surchargeRpt.Unit = unitCode;
                    surchargeRpt.LastRevised = string.Empty;
                    surchargeRpt.OBH = isOBH;
                    surchargeRpt.ExtRateVND = 34;
                    surchargeRpt.KBck = true;
                    surchargeRpt.NoInv = true;
                    surchargeRpt.Approvedby = string.Empty;
                    surchargeRpt.ApproveDate = DateTime.Now;
                    surchargeRpt.SalesCurr = currency;
                    surchargeRpt.GW = shipment.SumGrossWeight ?? 0;
                    surchargeRpt.MCW = 13;
                    surchargeRpt.HCW = shipment.SumChargeWeight ?? 0;
                    surchargeRpt.PaymentTerm = string.Empty;
                    surchargeRpt.DetailNotes = string.Empty;
                    surchargeRpt.ExpressNotes = string.Empty;
                    surchargeRpt.InvoiceNo = "InvoiceNo";
                    surchargeRpt.CodeVender = "CodeVender";
                    surchargeRpt.CodeCus = "CodeCus";
                    surchargeRpt.Freight = true;
                    surchargeRpt.Collect = true;
                    surchargeRpt.FreightPayableAt = "FreightPayableAt";
                    surchargeRpt.PaymentTime = 1;
                    surchargeRpt.PaymentTimeCus = 1;
                    surchargeRpt.Noofpieces = 12;
                    surchargeRpt.UnitPieaces = "UnitPieaces";
                    surchargeRpt.TpyeofService = "TpyeofService";
                    surchargeRpt.ShipmentSource = "FREE-HAND";
                    surchargeRpt.RealCost = true;
                    
                    dataSources.Add(surchargeRpt);
                }
            }
            result.AddDataSource(dataSources);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);

            return result;
        }
        public HandleState Update(OpsTransactionModel model)
        {
            var detail = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            int code = GetPermissionToUpdate(new ModelUpdate { BillingOpsId = model.BillingOpsId, SaleManId = model.SalemanId, UserCreated = model.UserCreated, CompanyId = model.CompanyId,  OfficeId = model.OfficeId, DepartmentId = model.DepartmentId, GroupId = model.GroupId }, permissionRange);
            if (code == 403) return new HandleState(403);
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            if (model.SalemanId != detail.SalemanId)
            {
                var dataUserLevels = userlevelRepository.Get(x => x.UserId == model.SalemanId).ToList();
                if (dataUserLevels.Select(t => t.GroupId).Count() >= 1)
                {
                    var dataGroup = dataUserLevels.Where(x => x.OfficeId == currentUser.OfficeID).ToList();
                    if (dataGroup.Any())
                    {
                        model.SalesGroupId = String.Join(";", dataGroup.Select(t => t.GroupId).Distinct());
                        model.SalesDepartmentId = String.Join(";", dataGroup.Select(t => t.DepartmentId).Distinct());
                        model.SalesOfficeId = String.Join(";", dataGroup.Select(t => t.OfficeId).Distinct());
                        model.SalesCompanyId = String.Join(";", dataGroup.Select(t => t.CompanyId).Distinct());
                    }
                    else
                    {
                        model.SalesGroupId = String.Join(";", dataUserLevels.Select(t => t.GroupId).Distinct());
                        model.SalesDepartmentId = String.Join(";", dataUserLevels.Select(t => t.DepartmentId).Distinct());
                        model.SalesOfficeId = String.Join(";", dataUserLevels.Select(t => t.OfficeId).Distinct());
                        model.SalesCompanyId = String.Join(";", dataUserLevels.Select(t => t.CompanyId).Distinct());
                    }
                }
            }
            else
            {
                model.SalesGroupId = detail.SalesGroupId;
                model.SalesDepartmentId = detail.SalesDepartmentId;
                model.SalesOfficeId = detail.SalesOfficeId;
                model.SalesCompanyId = detail.SalesCompanyId;
            }
            var hs = Update(model, x => x.Id == model.Id);
            if (hs.Success)
            {
                if (model.CsMawbcontainers != null)
                {
                    var hsContainer = mawbcontainerService.UpdateMasterBill(model.CsMawbcontainers, model.Id);
                }
                //Cập nhật JobNo, Mbl, Hbl cho các charge của housebill
                var hsSurcharge = UpdateSurchargeOfHousebill(model);
            }
            return hs;
        }
        private int GetPermissionToUpdate(ModelUpdate model, PermissionRange permissionRange)
        {
            if (permissionRange == PermissionRange.None) return 403;
            List<string> authorizeUserIds = permissionService.GetAuthorizedIds("CL", currentUser);
            int code = PermissionEx.GetPermissionItemOpsToUpdate(model, permissionRange, currentUser, authorizeUserIds);
            return code;
        }

        public ResultHandle CheckAllowConvertJob(List<CustomsDeclarationModel> list)
        {
            ResultHandle result = null;
            string notFoundPartnerTaxCodeMessages = string.Empty;
            string duplicateMessages = string.Empty;
            foreach(var item in list)
            {
                var customer = partnerRepository.Get(x => x.TaxCode == item.PartnerTaxCode)?.FirstOrDefault();
                if (customer == null) notFoundPartnerTaxCodeMessages += item.PartnerTaxCode + ", ";
                string dupMessage = CheckExist(item, item.Id);
                if(dupMessage != null)
                {
                    duplicateMessages += dupMessage + ", ";
                }
            }
            if(notFoundPartnerTaxCodeMessages.Length > 0)
            {
                notFoundPartnerTaxCodeMessages = "Partner TaxCode '" + notFoundPartnerTaxCodeMessages.Substring(0, notFoundPartnerTaxCodeMessages.Length - 2) + "' Not found";
                result = new ResultHandle { Status = false, Message = notFoundPartnerTaxCodeMessages, Data = 403 };
                return result;
            }
            if(duplicateMessages.Length > 0)
            {
                duplicateMessages = duplicateMessages.Substring(0, duplicateMessages.Length - 2);
                result = new ResultHandle { Status = false, Message = duplicateMessages, Data = 403 };
                return result;
            }
            var permissionDetailRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Detail);
            var hs = GetPermissionDetailConvertClearances(list, permissionDetailRange);
            if(hs.Success == true)
            {
                var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
                hs = GetPermissionDetailConvertClearances(list, permissionRange);
            }
            result = new ResultHandle { Status = hs.Success, Message = hs.Message?.ToString(), Data = 401 };
            return result;
        }
        private string CheckExist(CustomsDeclarationModel model, decimal id)
        {
            string message = null;
            if (id == 0)
            {
                if (customDeclarationRepository.Any(x => x.ClearanceNo == model.ClearanceNo && x.ClearanceDate == model.ClearanceDate))
                {
                    message = string.Format("This clearance no '{0}' and clearance date '{1}' has been existed", model.ClearanceNo, model.ClearanceDate);
                }
            }
            else
            {
                if (customDeclarationRepository.Any(x => (x.ClearanceNo == model.ClearanceNo && x.Id != id && x.ClearanceDate == model.ClearanceDate)))
                {
                    message = string.Format("This clearance no '{0}' and clearance date '{1}' has been existed", model.ClearanceNo, model.ClearanceDate);
                }
            }
            return message;
        }

        private HandleState GetPermissionDetailConvertClearances(List<CustomsDeclarationModel> list, PermissionRange permissionDetailRange)
        {
            var result = new HandleState();
            switch (permissionDetailRange)
            {
                case PermissionRange.None:
                    result = new HandleState(403, "You don't have permission to convert");
                    break;
                case PermissionRange.Owner:
                    var items = list.Where(x => x.UserCreated != currentUser.UserID).Select(x => x.ClearanceNo);
                    if (items.Count() > 0)
                    {
                        result = new HandleState(false, "Items: " + String.Join(", ", items.Distinct()) + " . You don't have permission to convert");
                    }
                    break;
                case PermissionRange.Group:
                    items = list.Where(x => x.GroupId != currentUser.GroupId
                                         && x.OfficeId != currentUser.OfficeID
                                         && x.DepartmentId != currentUser.DepartmentId).Select(x => x.ClearanceNo);
                    if (items.Count() > 0)
                    {
                        result = new HandleState(false, "Items: " + String.Join(", ", items.Distinct()) + " . You don't have permission to convert");
                    }
                    break;
                case PermissionRange.Department:
                    items = list.Where(x => x.DepartmentId != currentUser.DepartmentId && x.OfficeId != currentUser.OfficeID).Select(x => x.ClearanceNo);
                    if (items.Count() > 0)
                    {
                        result = new HandleState(false, "Items: " + String.Join(", ", items.Distinct()) + " . You don't have permission to convert");
                    }
                    break;
                case PermissionRange.Office:
                    items = list.Where(x => x.OfficeId != currentUser.OfficeID).Select(x => x.ClearanceNo);
                    if (items.Count() > 0)
                    {
                        result = new HandleState(false, "Items: " + String.Join(", ", items.Distinct()) + " . You don't have permission to convert");
                    }
                    break;
                case PermissionRange.Company:
                    items = list.Where(x => x.CompanyId != currentUser.CompanyID).Select(x => x.ClearanceNo);
                    if (items.Count() > 0)
                    {
                        result = new HandleState(false, "Items: " + String.Join(", ", items.Distinct()) + " . You don't have permission to convert");
                    }
                    break;
            }
            return result;
        }

        public HandleState LockOpsTransaction(Guid jobId)
        {
            OpsTransaction job = DataContext.First(x => x.Id == jobId && x.CurrentStatus != TermData.Canceled);
            if (job == null)
            {
                return new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);

            }
            if (job.IsLocked == true)
            {
                return new HandleState("Shipment has been locked !");
            }
            HandleState hs = new HandleState();

            job.UserModified = currentUser.UserID;
            job.DatetimeModified = DateTime.Now;
            job.IsLocked = true;
            job.LockedDate = DateTime.Now;
            job.LockedUser = currentUser.UserName;

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    hs = DataContext.Update(job, x => x.Id == jobId);

                    trans.Commit();
                    return hs;

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

        private HandleState UpdateSurchargeOfHousebill(OpsTransactionModel model)
        {
            try
            {
                var surcharges = surchargeRepository.Where(x => x.Hblid == model.Hblid);                
                foreach (var surcharge in surcharges)
                {
                    surcharge.JobNo = model.JobNo;
                    surcharge.Mblno = model.Mblno;
                    surcharge.Hblno = model.Hwbno;
                    var hsUpdateSurcharge = surchargeRepository.Update(surcharge, x => x.Id == surcharge.Id, false);                    
                }
                var sm = surchargeRepository.SubmitChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        /// <summary>
        /// Add a duplicate job to OpsTransaction
        /// </summary>
        /// <param name="model">OpsTransactionModel</param>
        /// <returns></returns>
        public ResultHandle ImportDuplicateJob(OpsTransactionModel model)
        {
            var detail = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            int code = GetPermissionToUpdate(new ModelUpdate { BillingOpsId = model.BillingOpsId, SaleManId = model.SalemanId, UserCreated = model.UserCreated, CompanyId = model.CompanyId, OfficeId = model.OfficeId, DepartmentId = model.DepartmentId, GroupId = model.GroupId }, permissionRange);
            if (code == 403) return new ResultHandle { Status = false, Message = "You can't duplicate this job." };
            var newContainers = new List<CsMawbcontainer>();
            var newSurcharges = new List<CsShipmentSurcharge>();
            // Create model import
            var _id = model.Id;
            var _hblId = model.Hblid;
            model.Id = Guid.NewGuid();
            model.Hblid = Guid.NewGuid();
            model.JobNo = CreateJobNoOps();
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            model.UserCreated = currentUser.UserID;
            model.GroupId = currentUser.GroupId;
            model.DepartmentId = currentUser.DepartmentId;
            model.OfficeId = currentUser.OfficeID;
            model.CompanyId = currentUser.CompanyID;
            var dataUserLevels = userlevelRepository.Get(x => x.UserId == model.SalemanId).ToList();
            if (dataUserLevels.Select(t => t.GroupId).Count() >= 1)
            {
                var dataGroup = dataUserLevels.Where(x => x.OfficeId == currentUser.OfficeID).ToList();
                if (dataGroup.Any())
                {
                    model.SalesGroupId = string.Join(";", dataGroup.Select(t => t.GroupId).Distinct());
                    model.SalesDepartmentId = string.Join(";", dataGroup.Select(t => t.DepartmentId).Distinct());
                    model.SalesOfficeId = string.Join(";", dataGroup.Select(t => t.OfficeId).Distinct());
                    model.SalesCompanyId = string.Join(";", dataGroup.Select(t => t.CompanyId).Distinct());
                }
                else
                {
                    model.SalesGroupId = string.Join(";", dataUserLevels.Select(t => t.GroupId).Distinct());
                    model.SalesDepartmentId = string.Join(";", dataUserLevels.Select(t => t.DepartmentId).Distinct());
                    model.SalesOfficeId = string.Join(";", dataUserLevels.Select(t => t.OfficeId).Distinct());
                    model.SalesCompanyId = string.Join(";", dataUserLevels.Select(t => t.CompanyId).Distinct());
                }
            }
            var dayStatus = (int)(model.ServiceDate.Value.Date - DateTime.Now.Date).TotalDays;
            if (dayStatus > 0)
            {
                model.CurrentStatus = TermData.InSchedule;
            }
            else
            {
                model.CurrentStatus = TermData.Processing;
            }

            // Update list Container
            var listContainerOld = model.CsMawbcontainers;
            if (listContainerOld != null)
            {
                var masterContainers = GetNewMasterBillContainer(model.Id, model.Hblid, listContainerOld);
                newContainers.AddRange(masterContainers);
            }
            // Update list SurCharge
            var listSurCharge = CopySurChargeToNewJob(_hblId, model.Hblid);
            if (listSurCharge?.Count() > 0)
            {
                newSurcharges.AddRange(listSurCharge);
            }

            try
            {
                var entity = mapper.Map<OpsTransaction>(model);
                var hs = DataContext.Add(entity);
                if (hs.Success)
                {
                    if (newContainers.Count > 0)
                    {
                        var hsContainer = csMawbcontainerRepository.Add(newContainers, false);
                        csMawbcontainerRepository.SubmitChanges();
                    }

                    if (newSurcharges.Count() > 0)
                    {
                        HandleState hsSurcharges = surchargeRepository.Add(newSurcharges, false);
                        surchargeRepository.SubmitChanges();
                    }
                    return new ResultHandle { Status = true, Message = "The job have been saved!", Data = entity };
                }
                return new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
            }
            catch (Exception ex)
            {
                return new ResultHandle { Status = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// Get list surcharges from old job to new job
        /// </summary>
        /// <param name="_oldHblId"></param>
        /// <param name="_newHblId"></param>
        /// <returns></returns>
        private List<CsShipmentSurcharge> CopySurChargeToNewJob(Guid _oldHblId, Guid _newHblId)
        {
            List<CsShipmentSurcharge> surCharges = null;
            var charges = surchargeRepository.Get(x => x.Hblid == _oldHblId);
            if (charges.Select(x => x.Id).Count() != 0)
            {
                surCharges = new List<CsShipmentSurcharge>();
                foreach (var item in charges)
                {
                    item.Id = Guid.NewGuid();
                    item.UserCreated = currentUser.UserID;
                    item.DatetimeCreated = DateTime.Now;
                    item.Hblid = _newHblId;
                    item.Soano = null;
                    item.PaySoano = null;
                    item.CreditNo = null;
                    item.DebitNo = null;
                    item.Soaclosed = null;
                    item.SettlementCode = null;

                    item.AcctManagementId = null;
                    item.InvoiceNo = null;
                    item.InvoiceDate = null;
                    item.VoucherId = null;
                    item.VoucherIddate = null;

                    surCharges.Add(item);
                }
            }
            return surCharges;
        }

        /// <summary>
        /// Get list containers from old job to new job
        /// </summary>
        /// <param name="newJobId"></param>
        /// <param name="newHblId"></param>
        /// <param name="csMawbcontainers"></param>
        /// <returns></returns>
        private List<CsMawbcontainer> GetNewMasterBillContainer(Guid newJobId, Guid newHblId, List<CsMawbcontainerModel> csMawbcontainers)
        {
            var containers = new List<CsMawbcontainer>();
            foreach (var item in csMawbcontainers)
            {
                item.Id = Guid.NewGuid();
                item.Mblid = newJobId;
                item.Hblid = newHblId;
                item.UserModified = currentUser.UserID;
                item.DatetimeModified = DateTime.Now;
                containers.Add(item);
            }
            return containers;
        }
    }
}
