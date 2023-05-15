﻿using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using eFMS.API.ForPartner.DL.Models.Receivable;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
        private readonly IContextBase<AcctAdvanceRequest> accAdvanceRequestRepository;
        private readonly IContextBase<AcctAdvancePayment> accAdvancePaymentRepository;
        private readonly IContextBase<OpsTransaction> opsTransactionRepository;
        readonly IContextBase<SysUserLevel> userlevelRepository;
        private readonly IContextBase<AcctAdvancePayment> acctAdvancePayment;
        private readonly IContextBase<AcctSettlementPayment> acctSettlementPayment;
        private readonly IContextBase<CatContract> catContractRepository;
        private readonly IContextBase<CsTransaction> transactionRepository;
        private readonly IContextBase<SysSettingFlow> settingFlowRepository;
        private readonly IContextBase<CatCharge> catChargeRepository;
        private readonly IContextBase<CsLinkCharge> csLinkChargeRepository;
        private readonly IContextBase<CatDepartment> departmentRepository;
        private readonly IContextBase<SysGroup> groupRepository;
        private readonly IContextBase<CsShipmentSurcharge> surChargeRepository;
        private readonly IContextBase<CsTransactionDetail> transactionDetailRepository;
        private readonly ICsShipmentSurchargeService csShipmentSurchargeServe;
        private readonly ICsTransactionService csTransactionServe;
        private decimal _decimalNumber = Constants.DecimalNumber;
        private decimal _decimalMinNumber = Constants.DecimalMinNumber;
        private IDatabaseUpdateService databaseUpdateService;
        private readonly IAccAccountReceivableService accAccountReceivableService;
        private readonly IContextBase<AccAccountingManagement> accMngtRepo;
        private readonly IOptions<ApiUrl> apiUrl;
        private readonly ICsStageAssignedService csStageAssignedService;
        private readonly ICheckPointService checkPointService;

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
            IContextBase<AcctAdvanceRequest> accAdvanceRequestRepo,
            IContextBase<SysUserLevel> userlevelRepo,
            IContextBase<AcctAdvancePayment> _acctAdvancePayment,
            IContextBase<AcctSettlementPayment> _acctSettlementPayment,
            IContextBase<OpsTransaction> _opsTransactionRepository,
            IContextBase<AcctAdvancePayment> _accAdvancePaymentRepository,
            IContextBase<CatContract> catContractRepo,
            IContextBase<CsTransaction> transactionRepo,
            IContextBase<SysSettingFlow> settingFlowRepo,
            IContextBase<CatCharge> catChargeRepo,
            IContextBase<CsLinkCharge> csLinkChargeRepo,
            IContextBase<CatDepartment> departmentRepo,
            IContextBase<CsShipmentSurcharge> surChargeRepo,
            IContextBase<SysGroup> groupRepo,
            IDatabaseUpdateService _databaseUpdateService,
            IAccAccountReceivableService accAccountReceivable,
            IContextBase<CsTransactionDetail> transactionDetail,
            IContextBase<AccAccountingManagement> accMngt,
            IOptions<ApiUrl> aUrl,
            ICsStageAssignedService csStageAssigned,
            ICheckPointService checkPoint
            ) : base(repository, mapper)
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
            accAdvanceRequestRepository = accAdvanceRequestRepo;
            acctAdvancePayment = _acctAdvancePayment;
            acctSettlementPayment = _acctSettlementPayment;
            opsTransactionRepository = _opsTransactionRepository;
            accAdvancePaymentRepository = _accAdvancePaymentRepository;
            catContractRepository = catContractRepo;
            transactionRepository = transactionRepo;
            settingFlowRepository = settingFlowRepo;
            catChargeRepository = catChargeRepo;
            csLinkChargeRepository = csLinkChargeRepo;
            departmentRepository = departmentRepo;
            groupRepository = groupRepo;
            databaseUpdateService = _databaseUpdateService;
            accAccountReceivableService = accAccountReceivable;
            surChargeRepository = surChargeRepo;
            transactionDetailRepository = transactionDetail;
            accMngtRepo = accMngt;
            apiUrl = aUrl;
            csStageAssignedService = csStageAssigned;
            checkPointService = checkPoint;
        }
        public override HandleState Add(OpsTransactionModel model)
        {
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403);

            HandleState result = new HandleState();

            model.Id = Guid.NewGuid();
            model.DatetimeCreated = DateTime.Now;
            model.UserCreated = currentUser.UserID;
            model.DatetimeModified = model.DatetimeCreated;
            model.UserModified = model.UserCreated;
            model.CurrentStatus = TermData.InSchedule;
            model.GroupId = currentUser.GroupId;
            model.DepartmentId = currentUser.DepartmentId;
            model.OfficeId = currentUser.OfficeID;
            model.CompanyId = currentUser.CompanyID;

            model.Hwbno = model.Hwbno?.Trim();
            model.Mblno = model.Mblno?.Trim();

            SaleManPermissionModel salemanPermissionInfo = GetAndUpdateSaleManInfo(model.SalemanId);
            model.SalesGroupId = salemanPermissionInfo.SalesGroupId;
            model.SalesDepartmentId = salemanPermissionInfo.SalesDepartmentId;
            model.SalesOfficeId = salemanPermissionInfo.SalesOfficeId;
            model.SalesCompanyId = salemanPermissionInfo.SalesCompanyId;

            int dayStatus = (int)(model.ServiceDate.Value.Date - DateTime.Now.Date).TotalDays;
            if (dayStatus > 0)
            {
                model.CurrentStatus = TermData.InSchedule;
            }
            else
            {
                model.CurrentStatus = TermData.Processing;
            }
            model.JobNo = string.Empty;
            //using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var opsInsert = mapper.Map<OpsTransaction>(model);
                    if (model.IsReplicate == true)
                    {
                        SysSettingFlow settingFlowOffice = settingFlowRepository.Get(x => x.OfficeId == currentUser.OfficeID && x.Flow == "Replicate")?.FirstOrDefault();
                        if (settingFlowOffice != null && settingFlowOffice.ReplicateOfficeId != null)
                        {
                            SysUserLevel dataUserLevel = userlevelRepository.Get(x => x.UserId == currentUser.UserID
                                                            && x.OfficeId == settingFlowOffice.ReplicateOfficeId).FirstOrDefault();
                            SysOffice officeReplicate = sysOfficeRepo.Get(x => x.Id == settingFlowOffice.ReplicateOfficeId)?.FirstOrDefault();

                            if (dataUserLevel == null)
                            {
                                return new HandleState((object)
                                    string.Format("You don't have permission at {0}, Please you check with system admin!", officeReplicate.ShortName)
                                    );
                            };
                            // Add new ops
                            OpsTransaction entityReplicate = MappingReplicateJob(opsInsert, dataUserLevel);
                            opsInsert.ReplicatedId = entityReplicate.Id;
                            var addResult = databaseUpdateService.InsertDataToDB(opsInsert);
                            if (!addResult.Status)
                            {
                                return new HandleState((object)"Fail to create ops job. Please try again.");
                            }
                            var opsInfo = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                            // Insert Replicate Data
                            entityReplicate.JobNo += opsInfo.JobNo;
                            addResult = databaseUpdateService.InsertDataToDB(entityReplicate);
                            result = new HandleState(addResult.Status, (object)addResult.Message);
                            if (model.CsMawbcontainers?.Count > 0 && result.Success)
                            {
                                var hsContainer = mawbcontainerService.UpdateMasterBill(model.CsMawbcontainers, entityReplicate.Id);
                                model.CsMawbcontainers.ForEach(x => x.Id = Guid.Empty);
                            }
                        }
                    }
                    else
                    {
                        // Add new ops
                        var addResult = databaseUpdateService.InsertDataToDB(opsInsert);
                        if (!addResult.Status)
                        {
                            return new HandleState((object)"Fail to create ops job. Please try again.");
                        }
                        var opsInfo = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                        OpsTransaction entity = mapper.Map<OpsTransaction>(opsInfo);
                        result = new HandleState(addResult.Status, (object)addResult.Message);
                    }
                    if (model.CsMawbcontainers?.Count > 0 && result.Success)
                    {
                        var hsContainer = mawbcontainerService.UpdateMasterBill(model.CsMawbcontainers, model.Id);
                    }
                }
                catch (Exception ex)
                {
                    new LogHelper("eFMS_Add_OpsTransaction_Log", ex.ToString());
                    result = new HandleState(ex.Message);
                }
            }
            return result;
        }

        private OpsTransaction MappingReplicateJob(OpsTransaction originJob, SysUserLevel dataUserLevel)
        {
            SysUser salemanDefault = userRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault();
            SaleManPermissionModel salemanPermissionInfoReplicate = GetAndUpdateSaleManInfo(salemanDefault.Id);

            var propInfo = originJob.GetType().GetProperties();
            OpsTransaction entityReplicate = new OpsTransaction();
            foreach (var item in propInfo)
            {
                entityReplicate.GetType().GetProperty(item.Name).SetValue(entityReplicate, item.GetValue(originJob, null), null);
            }

            entityReplicate.DatetimeCreated = DateTime.Now;
            entityReplicate.UserCreated = currentUser.UserID;
            entityReplicate.UserModified = currentUser.UserID;
            entityReplicate.DatetimeModified = DateTime.Now;
            entityReplicate.JobNo = GeneratePreFixReplicate() + originJob.JobNo;
            entityReplicate.Id = Guid.NewGuid();
            entityReplicate.Hblid = Guid.NewGuid();
            entityReplicate.ServiceNo = null;
            entityReplicate.ServiceHblId = null;
            entityReplicate.OfficeId = dataUserLevel.OfficeId;
            entityReplicate.DepartmentId = dataUserLevel.DepartmentId;
            entityReplicate.GroupId = dataUserLevel.GroupId;
            entityReplicate.CompanyId = dataUserLevel.CompanyId;
            entityReplicate.SalesGroupId = salemanPermissionInfoReplicate.SalesGroupId;
            entityReplicate.SalesDepartmentId = salemanPermissionInfoReplicate.SalesDepartmentId;
            entityReplicate.SalesOfficeId = salemanPermissionInfoReplicate.SalesOfficeId;
            entityReplicate.SalesCompanyId = salemanPermissionInfoReplicate.SalesCompanyId;
            entityReplicate.IsLocked = false;
            entityReplicate.SalemanId = salemanDefault.Id;
            entityReplicate.LinkSource = DocumentConstants.CLEARANCE_FROM_REPLICATE;

            return entityReplicate;
        }
        private SaleManPermissionModel GetAndUpdateSaleManInfo(string SalemanId)
        {
            SaleManPermissionModel model = new SaleManPermissionModel();

            List<SysUserLevel> dataUserLevels = userlevelRepository.Get(x => x.UserId == SalemanId).ToList();
            string SalesGroupId = string.Empty;
            string SalesDepartmentId = string.Empty;
            string SalesOfficeId = string.Empty;
            string SalesCompanyId = string.Empty;

            if (dataUserLevels.Select(t => t.GroupId).Count() >= 1)
            {
                List<SysUserLevel> dataGroup = dataUserLevels.Where(x => x.OfficeId == currentUser.OfficeID).ToList();
                if (dataGroup.Any())
                {
                    SalesGroupId = String.Join(";", dataGroup.Select(t => t.GroupId).Distinct());
                    SalesDepartmentId = String.Join(";", dataGroup.Where(x => x.DepartmentId != null).Select(t => t.DepartmentId).Distinct());
                    SalesOfficeId = String.Join(";", dataGroup.Where(x => x.OfficeId != null).Select(t => t.OfficeId).Distinct());
                    SalesCompanyId = String.Join(";", dataGroup.Where(x => x.CompanyId != null).Select(t => t.CompanyId).Distinct());
                }
                else
                {
                    SalesGroupId = String.Join(";", dataUserLevels.Select(t => t.GroupId).Distinct());
                    SalesDepartmentId = String.Join(";", dataUserLevels.Where(x => x.DepartmentId != null).Select(t => t.DepartmentId).Distinct());
                    SalesOfficeId = String.Join(";", dataUserLevels.Where(x => x.OfficeId != null).Select(t => t.OfficeId).Distinct());
                    SalesCompanyId = String.Join(";", dataUserLevels.Where(x => x.CompanyId != null).Select(t => t.CompanyId).Distinct());
                }

            }

            model.SalesGroupId = !string.IsNullOrEmpty(SalesGroupId) ? SalesGroupId : null;
            model.SalesDepartmentId = !string.IsNullOrEmpty(SalesDepartmentId) ? SalesDepartmentId : null;
            model.SalesOfficeId = !string.IsNullOrEmpty(SalesOfficeId) ? SalesOfficeId : null;
            model.SalesCompanyId = !string.IsNullOrEmpty(SalesCompanyId) ? SalesCompanyId : null;

            return model;
        }

        public string CreateJobNoOps(string transactionType)
        {
            SysOffice office = null;
            string prefixJob = string.Empty;
            var currentUserOffice = currentUser?.OfficeID ?? null;
            if (currentUserOffice != null)
            {
                office = sysOfficeRepo.Get(x => x.Id == currentUserOffice).FirstOrDefault();
                prefixJob = SetPrefixJobIdByOfficeCode(office?.Code);
            }
            if (transactionType == "TK")
            {
                prefixJob += DocumentConstants.TKI_SHIPMENT;
            }
            else
            {
                prefixJob += DocumentConstants.OPS_SHIPMENT;
            }
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
            switch (office.Code)
            {
                case "ITLHAN":
                    currentShipment = DataContext.Get(x => x.LinkSource != DocumentConstants.CLEARANCE_FROM_REPLICATE
                                                         && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                         && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                         && x.JobNo.StartsWith("H") && !x.JobNo.StartsWith("HAN-"))
                                                         .OrderByDescending(x => x.JobNo).FirstOrDefault(); //CR: HAN -> H [15202]
                    break;
                case "ITLDAD":
                    currentShipment = DataContext.Get(x => x.LinkSource != DocumentConstants.CLEARANCE_FROM_REPLICATE
                                                        && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                        && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                        && x.JobNo.StartsWith("D") && !x.JobNo.StartsWith("DAD-"))
                                                        .OrderByDescending(x => x.JobNo).FirstOrDefault(); //CR: DAD -> D [15202]
                    break;
                case "ITLCAM":
                    currentShipment = DataContext.Get(x => x.LinkSource != DocumentConstants.CLEARANCE_FROM_REPLICATE
                                                        && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                        && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                        && x.JobNo.StartsWith("C") && !x.JobNo.StartsWith("CAM-"))
                                                        .OrderByDescending(x => x.JobNo).FirstOrDefault();
                    break;
                default:
                    currentShipment = DataContext.Get(x => x.LinkSource != DocumentConstants.CLEARANCE_FROM_REPLICATE
                                                     && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                     && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                     && !x.JobNo.StartsWith("D") && !x.JobNo.StartsWith("DAD-")
                                                     && !x.JobNo.StartsWith("C") && !x.JobNo.StartsWith("CAM-")
                                                     && !x.JobNo.StartsWith("H") && !x.JobNo.StartsWith("HAN-"))
                                                     .OrderByDescending(x => x.JobNo).FirstOrDefault();
                    break;
            }
            return currentShipment;
        }

        private string SetPrefixJobIdByOfficeCode(string officeCode)
        {
            string prefixCode = string.Empty;
            switch (officeCode)
            {
                case "ITLHAN":
                    prefixCode = "H";
                    break;
                case "ITLDAD":
                    prefixCode = "D";
                    break;
                case "ITLCAM":
                    prefixCode = "C";
                    break;
                default:
                    break;
            }
            return prefixCode;
        }

        public int CheckDetailPermission(Guid id)
        {
            var detail = GetBy(id);
            var lstGroups = userlevelRepository.Get(x => x.GroupId == currentUser.GroupId).Select(t => t.UserId).ToList();
            var lstDepartments = userlevelRepository.Get(x => x.DepartmentId == currentUser.DepartmentId).Select(t => t.UserId).ToList();

            var SalemansIds = DataContext.Get(x => x.Id == id).Select(t => t.SalemanId).ToArray();
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Detail);
            int code = GetPermissionToUpdate(new ModelUpdate { BillingOpsId = detail.BillingOpsId, SaleManId = detail.SalemanId, UserCreated = detail.UserCreated, SalemanIds = SalemansIds, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId, Groups = lstGroups, Departments = lstDepartments }, permissionRange);
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
                details.CustomerAccountNo = customer?.AccountNo;
                CatPlace place = placeRepository.Get(x => x.Id == details.ClearanceLocation).FirstOrDefault();
                details.PlaceNameCode = place?.Code;

                details.UserCreatedName = userRepository.Get(x => x.Id == details.UserCreated).FirstOrDefault()?.Username;
                details.UserModifiedName = userRepository.Get(x => x.Id == details.UserModified).FirstOrDefault()?.Username;

                details.SalesmanName = userRepository.Get(x => x.Id.ToString() == details.SalemanId)?.FirstOrDefault()?.Username;

                details.IsAllowChangeSaleman = !(details.LinkSource == DocumentConstants.CLEARANCE_FROM_REPLICATE);
            }
            return details;
        }
        public OpsTransactionModel GetDetails(Guid id)
        {
            //PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            var detail = GetBy(id);
            if (detail == null) return null;
            var tranType = detail.TransactionType == "TK" ? "TK" : "CL";
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, detail.TransactionType == "TK" ?Menu.opsTruckingInland:Menu.opsJobManagement);
            List<string> authorizeUserIds = permissionService.GetAuthorizedIds(tranType, currentUser);
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Delete);
            detail.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionDetail(permissionRangeWrite, authorizeUserIds, detail),
                AllowDelete = GetPermissionDetail(permissionRangeDelete, authorizeUserIds, detail)
            };
            //var specialActions = currentUser.UserMenuPermission.SpecialActions;
            var specialActions = _user.UserMenuPermission.SpecialActions;
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
            ICurrentUser _user = PermissionEx.GetUserMenuPermissionTransaction(criteria.TransactionType, currentUser);

            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);

            criteria.RangeSearch = rangeSearch;

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

                data.ToList().ForEach(x =>
                {
                    x.ClearanceNo = customDeclarationRepository.Get(cus => cus.JobNo == x.JobNo).OrderBy(cus => cus.ClearanceDate).ThenBy(cus => cus.ClearanceNo)
                    .Select(cus => cus.ClearanceNo).FirstOrDefault();
                    x.CustomerName = customers.FirstOrDefault(cus => cus.Id == x.CustomerId)?.ShortName;
                    x.POLName = ports.FirstOrDefault(pol => pol.Id == x.Pol)?.NameEn;
                    x.PODName = ports.FirstOrDefault(pod => pod.Id == x.Pod)?.NameEn;
                    x.GroupName = groupRepository.Get(y => y.Id == x.GroupId)?.FirstOrDefault().ShortName;
                    x.DepartmentName = departmentRepository.Get(z => z.Id == x.DepartmentId)?.FirstOrDefault().DeptNameAbbr;
                    IQueryable<SysUser> sysUsers = userRepository.Get(u => u.Id == x.UserCreated);

                    x.UserCreatedName = sysUsers?.FirstOrDefault()?.Username;
                    x.UserCreatedNameLinkJob = string.IsNullOrEmpty(x.UserCreatedLinkJob) ? "" : userRepository.Get(u => u.Id == x.UserCreatedLinkJob)?.FirstOrDefault()?.Username;

                    if (x.ReplicatedId != null)
                    {
                        var replicateJob = DataContext.Get(d => d.Id == x.ReplicatedId)?.FirstOrDefault();
                        x.ReplicateJobNo = replicateJob?.JobNo;
                    }
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

        private string GetClearanceNoOfShipment(string jobNo, IQueryable<CsShipmentSurcharge> surcharge, IQueryable<CustomsDeclaration> clearances)
        {
            var surchargeShipment = surcharge.Where(x => x.JobNo == jobNo);
            var clearanceNos = surchargeShipment.Select(x => x.ClearanceNo).ToList();
            var clearanceShipments = clearances.Where(x => x.JobNo == jobNo && clearanceNos.Contains(x.ClearanceNo))?.OrderBy(x => x.ClearanceDate).ThenBy(x => x.DatetimeModified).ToList();
            var clearanceShipment = clearanceShipments.FirstOrDefault();
            if (clearanceShipments.Any(x => x.ConvertTime != null))
            {
                clearanceShipment = clearanceShipments.FirstOrDefault(x => x.ConvertTime != null);
            }
            if (surchargeShipment.Count() > 0 && clearanceShipment != null)
            {
                return clearanceShipment.ClearanceNo;
            }
            else
            {
                return clearances.Where(x => x.JobNo == jobNo)?.OrderBy(x => x.ClearanceDate).ThenBy(x => x.DatetimeModified).FirstOrDefault().ClearanceNo;
            }
        }

        private IQueryable<OpsTransactionModel> FormatDataPaging(IQueryable<OpsTransaction> dataQuery)
        {
            IQueryable<CatPartner> customers = partnerRepository.Get(x => x.PartnerGroup.Contains("CUSTOMER"));
            IQueryable<CatPlace> ports = placeRepository.Get(x => x.PlaceTypeId == "Port");
            List<OpsTransactionModel> list = new List<OpsTransactionModel>();

            foreach (var x in dataQuery)
            {
                OpsTransactionModel item = mapper.Map<OpsTransactionModel>(x);
                item.ClearanceNo = customDeclarationRepository.Get(cus => cus.JobNo == item.JobNo)
                    .OrderBy(cus => cus.ClearanceDate)
                    .ThenBy(cus => cus.ClearanceNo)
                    .Select(cus => cus.ClearanceNo)
                    .FirstOrDefault();

                item.CustomerName = customers.FirstOrDefault(cus => cus.Id == x.CustomerId)?.ShortName;
                item.POLName = ports.FirstOrDefault(pol => pol.Id == x.Pol)?.NameEn;
                item.PODName = ports.FirstOrDefault(pod => pod.Id == x.Pod)?.NameEn;
                item.GroupName = groupRepository.Get(y => y.Id == x.GroupId)?.FirstOrDefault().ShortName;
                item.DepartmentName = departmentRepository.Get(z => z.Id == x.DepartmentId)?.FirstOrDefault().DeptNameAbbr;

                IQueryable<SysUser> sysUsers = userRepository.Get(u => u.Id == x.UserCreated);

                item.UserCreatedName = sysUsers?.FirstOrDefault()?.Username;
                item.UserCreatedNameLinkJob = string.IsNullOrEmpty(x.UserCreatedLinkJob) ? "" : userRepository.Get(u => u.Id == x.UserCreatedLinkJob)?.FirstOrDefault()?.Username;

                if (x.ReplicatedId != null)
                {
                    var replicateJob = DataContext.Get(d => d.Id == x.ReplicatedId)?.FirstOrDefault();
                    item.ReplicateJobNo = replicateJob?.JobNo;
                }

                list.Add(item);
            }
            return list.AsQueryable();
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
            return true;
        }
        public bool CheckAllowDeleteJobUsed(Guid jobId)
        {
            var detail = DataContext.Get(x => x.Id == jobId && x.CurrentStatus != TermData.Canceled)?.FirstOrDefault();
            if (detail.ReplicatedId != null || detail.IsLocked == true)
            {
                return false;
            }
            var query = surchargeRepository.Get(x => x.Hblid == detail.Hblid && (!string.IsNullOrEmpty(x.CreditNo)
                        || !string.IsNullOrEmpty(x.DebitNo)
                        || !string.IsNullOrEmpty(x.Soano)
                        || !string.IsNullOrEmpty(x.PaymentRefNo)
                        || !string.IsNullOrEmpty(x.AdvanceNo)
                        || !string.IsNullOrEmpty(x.VoucherId)
                        || !string.IsNullOrEmpty(x.PaySoano)
                        || !string.IsNullOrEmpty(x.SettlementCode)
                        || !string.IsNullOrEmpty(x.SyncedFrom)
                        || !string.IsNullOrEmpty(x.PaySyncedFrom)
                        || !string.IsNullOrEmpty(x.LinkChargeId)
                        || x.LinkFee == true)
                        );
            if (query.Any() || accAdvanceRequestRepository.Any(x => x.JobId == detail.JobNo))
            {
                return false;
            }
            return true;
        }
        public Expression<Func<OpsTransaction, bool>> QueryByPermission(PermissionRange range)
        {
            //IQueryable<OpsTransaction> data = null;
            Expression<Func<OpsTransaction, bool>> query = q => (q.CurrentStatus != TermData.Canceled || q.CurrentStatus == null);
            List<string> authorizeUserIds = permissionService.GetAuthorizedIds("CL", currentUser);
            switch (range)
            {
                case PermissionRange.All:
                    // query = query.And(x => x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null);
                    break;
                case PermissionRange.Owner:
                    query = query.And(x => ((x.BillingOpsId == currentUser.UserID && x.OfficeId == currentUser.OfficeID)
                                                    || x.SalemanId == currentUser.UserID
                                                    || authorizeUserIds.Contains(x.BillingOpsId)
                                                    || authorizeUserIds.Contains(x.SalemanId)
                                                    || (x.UserCreated == currentUser.UserID && x.OfficeId == currentUser.OfficeID)
                                            ));
                    break;
                case PermissionRange.Group:
                    var dataUserLevel = userlevelRepository.Get(x => x.GroupId == currentUser.GroupId).Select(t => t.UserId).ToList();
                    query = query.And(x => ((x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId)
                                                || (dataUserLevel.Contains(x.SalemanId))));
                    break;
                case PermissionRange.Department:
                    var dataUserLevelDepartment = userlevelRepository.Get(x => x.DepartmentId == currentUser.DepartmentId).Select(t => t.UserId).ToList();
                    query = query.And(x => ((x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId)
                                                || dataUserLevelDepartment.Contains(x.SalemanId)));
                    break;
                case PermissionRange.Office:
                    query = query.And(x => ((x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId)));
                    break;
                case PermissionRange.Company:
                    query = query.And(x => (x.CompanyId == currentUser.CompanyID
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId)
                                                || x.UserCreated == currentUser.UserID));
                    break;
            }

            return query;
        }


        /// <summary>
        /// Nếu không có điều kiện search thì load list Job 3 tháng kể từ ngày modified mới nhất trở về trước
        /// </summary>
        /// <returns></returns>
        private Expression<Func<OpsTransaction, bool>> ExpressionQueryDefault(OpsTransactionCriteria criteria)
        {
            Expression<Func<OpsTransaction, bool>> query = q => true;
            if (string.IsNullOrEmpty(criteria.All) && string.IsNullOrEmpty(criteria.JobNo)
                && string.IsNullOrEmpty(criteria.Mblno) && string.IsNullOrEmpty(criteria.Hwbno)
                && string.IsNullOrEmpty(criteria.CustomerId) && string.IsNullOrEmpty(criteria.ClearanceNo)
                && string.IsNullOrEmpty(criteria.ProductService) && string.IsNullOrEmpty(criteria.ServiceMode)
                && criteria.CreatedDateFrom == null && criteria.CreatedDateTo == null
                && string.IsNullOrEmpty(criteria.ShipmentMode) && string.IsNullOrEmpty(criteria.FieldOps)
                && string.IsNullOrEmpty(criteria.CreditDebitInvoice)
                && criteria.ServiceDateFrom == null && criteria.ServiceDateTo == null)
            {
                var maxDate = (DataContext.Get().Max(x => x.DatetimeModified) ?? DateTime.Now).AddDays(1).Date;
                var minDate = maxDate.AddMonths(-3).AddDays(-1).Date; //Bắt đầu từ ngày MaxDate trở về trước 3 tháng
                query = query.And(x => x.DatetimeModified.Value > minDate && x.DatetimeModified.Value < maxDate);
            }

            return query.And(x => x.TransactionType == criteria.TransactionType);
        }

        public IQueryable<OpsTransactionModel> Query(OpsTransactionCriteria criteria)
        {
            if (criteria.RangeSearch == PermissionRange.None) return null;
            //IQueryable<OpsTransaction> data = QueryByPermission(criteria.RangeSearch);

            //Nếu không có điều kiện search thì load 3 tháng kể từ ngày modified mới nhất
            var queryDefault = ExpressionQueryDefault(criteria);
            var data = DataContext.Get(queryDefault);
            var queryPermission = QueryByPermission(criteria.RangeSearch);
            if (criteria.TransactionType == null)
            {
                queryPermission = QuerySearchLinkJob(queryPermission, criteria);
            }
            data = data.Where(queryPermission);

            if (data == null) return null;

            List<OpsTransactionModel> results = new List<OpsTransactionModel>();
            IQueryable<OpsTransaction> datajoin = data.Where(x => x.CurrentStatus != TermData.Canceled);

            if (!string.IsNullOrEmpty(criteria.ClearanceNo))
            {
                IQueryable<CustomsDeclaration> listCustomsDeclaration = customDeclarationRepository.Get(x => x.ClearanceNo.ToLower().Contains(criteria.ClearanceNo.ToLower()));
                if (listCustomsDeclaration.Count() > 0)
                {
                    datajoin = from custom in listCustomsDeclaration
                               join datas in data on custom.JobNo equals datas.JobNo
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
            if (!string.IsNullOrEmpty(criteria.CreditDebitInvoice))
            {
                IQueryable<AcctCdnote> listDebit = acctCdNoteRepository.Get(x => x.Code.ToLower().Contains(criteria.CreditDebitInvoice.ToLower()));
                if (listDebit.Count() > 0)
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
                                //&& (x.TransactionType ?? "").IndexOf(criteria.TransactionType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.CustomerId == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                                && (x.FieldOpsId == criteria.FieldOps || string.IsNullOrEmpty(criteria.FieldOps))
                                && (x.ShipmentMode == criteria.ShipmentMode || string.IsNullOrEmpty(criteria.ShipmentMode))
                                && ((x.ServiceDate ?? null) >= criteria.ServiceDateFrom || criteria.ServiceDateFrom == null)
                                && ((x.ServiceDate ?? null) <= criteria.ServiceDateTo || criteria.ServiceDateTo == null)
                                && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= criteria.CreatedDateFrom || criteria.CreatedDateFrom == null)
                                && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= criteria.CreatedDateTo || criteria.CreatedDateTo == null)
                            ).OrderByDescending(x => x.DatetimeModified);
            }
            else
            {
                datajoin = datajoin.Where(x => (x.JobNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Hwbno ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Mblno ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ProductService ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ServiceMode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   //|| (x.TransactionType ?? "").IndexOf(criteria.TransactionType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.CustomerId == criteria.All || string.IsNullOrEmpty(criteria.All))
                                   || (x.FieldOpsId == criteria.All || string.IsNullOrEmpty(criteria.All))
                                   || (x.ShipmentMode == criteria.All || string.IsNullOrEmpty(criteria.All))
                                   || ((x.ServiceDate ?? null) >= (criteria.ServiceDateFrom ?? null) && (x.ServiceDate ?? null) <= (criteria.ServiceDateTo ?? null))
                                   || ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.CreatedDateFrom ?? null) && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.CreatedDateTo ?? null))
                               ).OrderByDescending(x => x.DatetimeModified);
            }
            //datajoin=datajoin.Where(x=>x.TransactionType==criteria.TransactionType);
            results = mapper.Map<List<OpsTransactionModel>>(datajoin);
            return results.AsQueryable();
        }

        private Expression<Func<OpsTransaction, bool>> QuerySearchLinkJob(Expression<Func<OpsTransaction, bool>> query, OpsTransactionCriteria criteria)
        {
            if (!string.IsNullOrEmpty(criteria.LinkFeeSearch) && criteria.LinkFeeSearch == "Have Linked")
                query = query.And(x => x.IsLinkFee == true);
            if (!string.IsNullOrEmpty(criteria.LinkFeeSearch) && criteria.LinkFeeSearch == "Not Link")
                query = query.And(x => x.IsLinkFee == null || x.IsLinkFee == false);
            if (!string.IsNullOrEmpty(criteria.LinkJobSearch) && criteria.LinkJobSearch == "Have Linked")
                query = query.And(x => !string.IsNullOrEmpty(x.ServiceNo));
            if (!string.IsNullOrEmpty(criteria.LinkJobSearch) && criteria.LinkJobSearch == "Not Link")
                query = query.And(x => string.IsNullOrEmpty(x.ServiceNo));
            return query;
        }

        private string SetProductServiceShipment(CustomsDeclarationModel model)
        {
            string productService = string.Empty;
            if (model.ServiceType == "Sea")
            {
                if (model.CargoType == "FCL")
                {
                    productService = "Sea FCL";
                }
                else
                {
                    productService = "Sea LCL";
                }
            }
            else
            {
                if (model.CargoType != null && model.ServiceType == null)
                {
                    model.ServiceType = "Sea";
                    if (model.CargoType == "FCL")
                    {
                        productService = "Sea FCL";
                    }
                    else
                    {
                        productService = "Sea LCL";
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
                var existedMessage = CheckExist(null, model.Mblid, model.Hblid);
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

                // Check if customer existed
                // CatPartner customer = new CatPartner();
                var hsCheckPoint = checkPointService.ValidateCheckPointPartnerConvertClearance(model.AccountNo, model.PartnerTaxCode, out CatContract contract);
                if(hsCheckPoint.Success == false)
                {
                    return hsCheckPoint;
                }
                CatContract customerContract = new CatContract();
                customerContract = contract;
               
                OpsTransaction opsTransaction = GetNewShipmentToConvert(productService, model, customerContract);
                opsTransaction.JobNo = CreateJobNoOps(null); //Generate JobNo [17/12/2020]

                bool existedJobNo = CheckExistJobNo(opsTransaction.Id, opsTransaction.JobNo);
                if (existedJobNo == true)
                {
                    return new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_JOBNO_EXISTED, opsTransaction.JobNo].Value);
                }

                //if (model.Id > 0)
                //{
                //    CustomsDeclaration clearance = UpdateInfoConvertClearance(model);
                //    clearance.JobNo = opsTransaction.JobNo;

                //    customDeclarationRepository.Update(clearance, x => x.Id == clearance.Id);
                //}
                if (model.IsReplicate)
                {
                    HandleState rs = CreateJobAndClearanceReplicate(opsTransaction, productService, model, customerContract, out OpsTransaction opsReplicate, out CustomsDeclaration cdReplicate);
                    if (rs.Message == null)
                    {
                        if (opsReplicate != null)
                        {
                            DataContext.Add(opsReplicate, false);
                            opsTransaction.ReplicatedId = opsReplicate.Id;  // ID của job Replicate.
                        }
                        DataContext.Add(opsTransaction, false);

                        result = DataContext.SubmitChanges();
                        if (result.Success && model.Id == 0)
                        {
                            CustomsDeclaration clearance = GetNewClearanceModel(model);
                            clearance.JobNo = opsTransaction.JobNo;
                            HandleState hsAddCd = customDeclarationRepository.Add(clearance, false);

                            if (cdReplicate != null)
                            {
                                cdReplicate.JobNo = opsReplicate.JobNo;
                                HandleState hsAddCdReplicate = customDeclarationRepository.Add(cdReplicate, false);
                            }

                            result = customDeclarationRepository.SubmitChanges();
                        }
                    }
                    else
                    {
                        return new HandleState((object)rs.Message);
                    }

                }
                else
                {
                    HandleState hs = DataContext.Add(opsTransaction);
                    if (hs.Success)
                    {
                        CustomsDeclaration clearance = GetNewClearanceModel(model);
                        clearance.JobNo = opsTransaction.JobNo;

                        result = customDeclarationRepository.Add(clearance);
                    }
                    else
                    {
                        return new HandleState((object)hs.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_CONVERT_CLEARANCE_LOG", ex.ToString());
                result = new HandleState(ex.Message);
            }
            return result;
        }

        private OpsTransaction GetNewShipmentToConvert(string productService, CustomsDeclarationModel model, CatContract customerContract)
        {
            OpsTransaction opsTransaction = new OpsTransaction
            {
                Id = Guid.NewGuid(),
                Hblid = Guid.NewGuid(),
                ProductService = productService,
                ServiceMode = model.Type,
                ShipmentMode = "External",
                Mblno = model.Mblid,
                Hwbno = model.Hblid,
                SumContainers = model.QtyCont,
                SumPackages = model.Pcs,
                ServiceDate = model.ClearanceDate,
                SumGrossWeight = model.GrossWeight,
                SumNetWeight = model.NetWeight,
                SumCbm = model.Cbm,
                Shipper = model.Shipper,
                Consignee = model.Consignee,
                Eta = model.Eta,
                ClearanceDate = model.ClearanceDate,
                BillingOpsId = currentUser.UserID,
                GroupId = currentUser.GroupId,
                DepartmentId = currentUser.DepartmentId,
                OfficeId = currentUser.OfficeID,
                CompanyId = currentUser.CompanyID,
                DatetimeCreated = DateTime.Now,
                UserCreated = currentUser.UserID, //currentUser.UserID;
                DatetimeModified = DateTime.Now,
                UserModified = currentUser.UserID,
                ShipmentType = customerContract.ShipmentType == "Nominated" ? "Nominated" : "Freehand",
            };

            CatPartner customer = new CatPartner();
            if (model.AccountNo == null)
            {
                customer = partnerRepository.Get(x => x.TaxCode == model.PartnerTaxCode).FirstOrDefault();
            }
            else
            {
                customer = partnerRepository.Get(x => x.AccountNo == model.AccountNo).FirstOrDefault();
            }
            if (customer != null)
            {
                opsTransaction.CustomerId = customer.Id;

            }
            CatPlace port = placeRepository.Get(x => x.Code == model.Gateway).FirstOrDefault();
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

            CatUnit unit = unitRepository.Get(x => x.Code == model.UnitCode).FirstOrDefault();
            if (unit != null)
            {

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

            opsTransaction.SalemanId = customerContract.SaleManId;
            SaleManPermissionModel salemanPermissionInfo = GetAndUpdateSaleManInfo(customerContract.SaleManId);
            opsTransaction.SalesGroupId = salemanPermissionInfo.SalesGroupId;
            opsTransaction.SalesDepartmentId = salemanPermissionInfo.SalesDepartmentId;
            opsTransaction.SalesOfficeId = salemanPermissionInfo.SalesOfficeId;
            opsTransaction.SalesCompanyId = salemanPermissionInfo.SalesCompanyId;

            var supllier = partnerRepository.Get(x => x.TaxCode == DocumentConstants.NON_CARRIER_PARTNER_CODE).FirstOrDefault();
            opsTransaction.SupplierId = supllier?.Id;

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
                if (customDeclarationRepository.Any(x => x.ClearanceNo == model.ClearanceNo
                && x.ClearanceDate == model.ClearanceDate
                && x.Source != DocumentConstants.CLEARANCE_FROM_REPLICATE))
                {
                    return true;
                }
            }
            else
            {
                if (customDeclarationRepository.Any(x => (x.ClearanceNo == model.ClearanceNo
                && x.Id != id
                && x.ClearanceDate == model.ClearanceDate
                && x.Source != DocumentConstants.CLEARANCE_FROM_REPLICATE))) // không check trùng vs tk replicate (có thể phát sinh tk rác)
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
                    var hsCheckPoint = checkPointService.ValidateCheckPointPartnerConvertClearance(item.AccountNo, item.PartnerTaxCode, out CatContract contract);
                    if (hsCheckPoint.Success == false)
                    {
                        return hsCheckPoint;
                    }
                    CatContract customerContract = new CatContract();
                    customerContract = contract;

                    string existedMessage = CheckExist(null, item.Mblid, item.Hblid);
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

                        using (var trans = DataContext.DC.Database.BeginTransaction())
                        {
                            try
                            {
                                OpsTransaction opsTransaction = GetNewShipmentToConvert(productService, item, customerContract);
                                opsTransaction.JobNo = CreateJobNoOps(null); //Generate JobNo [17/12/2020]

                                bool existedJobNo = CheckExistJobNo(opsTransaction.Id, opsTransaction.JobNo);
                                if (existedJobNo == true)
                                {
                                    return new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_JOBNO_EXISTED, opsTransaction.JobNo].Value);
                                }

                                CustomsDeclaration clearance = UpdateInfoConvertClearance(item);

                                if (item.IsReplicate)
                                {
                                    HandleState rs = CreateJobAndClearanceReplicate(opsTransaction, productService, item, customerContract, out OpsTransaction opsReplicate, out CustomsDeclaration cdReplicate);
                                    if (rs.Message == null)
                                    {
                                        DataContext.Add(opsTransaction, false);
                                        if (opsReplicate != null)
                                        {
                                            opsTransaction.ReplicatedId = opsReplicate.Id;
                                            DataContext.Add(opsReplicate, false);
                                        }

                                        if (cdReplicate != null)
                                        {
                                            cdReplicate.JobNo = opsReplicate.JobNo;
                                        }
                                        customDeclarationRepository.Add(cdReplicate, false);

                                        result = DataContext.SubmitChanges();

                                        if (result.Success)
                                        {
                                            clearance.JobNo = opsTransaction.JobNo;
                                            customDeclarationRepository.Update(clearance, x => x.Id == clearance.Id, false);

                                            customDeclarationRepository.SubmitChanges();
                                        }

                                        i = i + 1;

                                        trans.Commit();
                                    }
                                    else
                                    {
                                        trans.Rollback();
                                        return new HandleState(new Exception(Convert.ToString(rs.Message)));
                                    }
                                }
                                else
                                {
                                    HandleState hsJob = DataContext.Add(opsTransaction);
                                    if (hsJob.Success)
                                    {
                                        clearance.JobNo = opsTransaction.JobNo;
                                        result = customDeclarationRepository.Update(clearance, x => x.Id == clearance.Id);

                                        i = i + 1;

                                        trans.Commit();
                                    }

                                }

                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                return new HandleState(ex.Message);
                                throw;
                            }
                            finally
                            {
                                trans.Dispose();
                            }
                        }


                    }
                }

                DataContext.SubmitChanges();
                customDeclarationRepository.SubmitChanges();
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_CONVERT_CLEARANCE_LOG", ex.ToString());
                result = new HandleState(ex.Message);
            }
            return result;
        }

        private HandleState CreateJobAndClearanceReplicate(OpsTransaction opsTransaction, string productService, CustomsDeclarationModel cd,
            CatContract customerContract, out OpsTransaction opsTransactionReplicate, out CustomsDeclaration clearanceReplicate)
        {
            clearanceReplicate = null;
            opsTransactionReplicate = null;
            SysSettingFlow settingFlowOffice = settingFlowRepository.Get(x => x.OfficeId == currentUser.OfficeID && x.Flow == "Replicate")?.FirstOrDefault();

            HandleState result = new HandleState();
            if (settingFlowOffice != null && settingFlowOffice.ReplicateOfficeId != null)
            {
                SysUserLevel dataUserLevel = userlevelRepository.Get(x => x.UserId == currentUser.UserID
                       && x.OfficeId == settingFlowOffice.ReplicateOfficeId).FirstOrDefault();
                var officeReplicate = sysOfficeRepo.Get(x => x.Id == settingFlowOffice.ReplicateOfficeId)?.FirstOrDefault();

                if (dataUserLevel == null)
                {
                    return new HandleState((object)
                        string.Format("You don't have permission at {0}, Please you check with system admin!", officeReplicate.ShortName)
                        );
                };

                string preFix = "R";
                if (!string.IsNullOrEmpty(settingFlowOffice.ReplicatePrefix))
                {
                    preFix = settingFlowOffice.ReplicatePrefix;
                }

                opsTransactionReplicate = GetNewShipmentToConvert(productService, cd, customerContract);

                opsTransactionReplicate.JobNo = preFix + opsTransaction.JobNo;
                opsTransactionReplicate.ServiceNo = null;
                opsTransactionReplicate.ServiceHblId = null;
                opsTransactionReplicate.LinkSource = DocumentConstants.CLEARANCE_FROM_REPLICATE;
                opsTransactionReplicate.OfficeId = settingFlowOffice.ReplicateOfficeId; // office của setting replicate

                // mapping saleman
                var salemanDefault = userRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault();
                SaleManPermissionModel salemanPermissionInfoReplicate = GetAndUpdateSaleManInfo(salemanDefault.Id);
                opsTransactionReplicate.SalesGroupId = salemanPermissionInfoReplicate.SalesGroupId;
                opsTransactionReplicate.SalesDepartmentId = salemanPermissionInfoReplicate.SalesDepartmentId;
                opsTransactionReplicate.SalesOfficeId = salemanPermissionInfoReplicate.SalesOfficeId;
                opsTransactionReplicate.SalesCompanyId = salemanPermissionInfoReplicate.SalesCompanyId;
                opsTransactionReplicate.SalemanId = salemanDefault.Id;

                // mapping permission
                opsTransactionReplicate.OfficeId = dataUserLevel.OfficeId;
                opsTransactionReplicate.DepartmentId = dataUserLevel.DepartmentId;
                opsTransactionReplicate.GroupId = dataUserLevel.GroupId;
                opsTransactionReplicate.CompanyId = dataUserLevel.CompanyId;

                //HandleState hsAddOpsReplicate = DataContext.Add(opsTransactionReplicate);
                clearanceReplicate = new CustomsDeclaration
                {
                    ConvertTime = DateTime.Now,
                    DatetimeCreated = DateTime.Now,
                    DatetimeModified = DateTime.Now,
                    UserCreated = currentUser.UserID,
                    UserModified = currentUser.UserID,
                    Source = DocumentConstants.CLEARANCE_FROM_REPLICATE,
                    GroupId = currentUser.GroupId,
                    DepartmentId = currentUser.DepartmentId,
                    OfficeId = settingFlowOffice.ReplicateOfficeId, // theo Office của Replicatate setting
                    CompanyId = currentUser.CompanyID,
                    JobNo = opsTransactionReplicate.JobNo,
                    Hblid = cd.Hblid,
                    Mblid = cd.Mblid,
                    NetWeight = cd.NetWeight,
                    GrossWeight = cd.GrossWeight,
                    Pcs = cd.Pcs,
                    Cbm = cd.Cbm,
                    UnitCode = cd.UnitCode,
                    Note = cd.Note,
                    AccountNo = cd.AccountNo,
                    PartnerTaxCode = cd.PartnerTaxCode,
                    ServiceType = cd.ServiceType,
                    Gateway = cd.Gateway,
                    Type = cd.Type,
                    CargoType = cd.CargoType,
                    Shipper = cd.Shipper,
                    Consignee = cd.Consignee,
                    ClearanceDate = cd.ClearanceDate,
                    ClearanceNo = cd.ClearanceNo,
                    QtyCont = cd.QtyCont,
                };

                // result = customDeclarationRepository.Add(clearanceReplicate);
            }
            return result;
        }

        private CustomsDeclaration UpdateInfoConvertClearance(CustomsDeclarationModel custom)
        {
            var clearance = mapper.Map<CustomsDeclaration>(custom);
            clearance.UserModified = currentUser.UserID;
            clearance.DatetimeModified = DateTime.Now;
            clearance.ConvertTime = DateTime.Now;
            clearance.GroupId = currentUser.GroupId;
            clearance.DepartmentId = currentUser.DepartmentId;
            clearance.OfficeId = currentUser.OfficeID;
            clearance.CompanyId = currentUser.CompanyID;
            return clearance;
        }

        public HandleState SoftDeleteJob(Guid id, out List<ObjectReceivableModel> modelReceivables)
        {
            modelReceivables = new List<ObjectReceivableModel>();
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
                result = DataContext.Update(job, x => x.Id == id, false);
                if (result.Success)
                {
                    //Xóa Job OPS xóa luôn surcharge [Andy - 05/02/2021]
                    var charges = surchargeRepository.Get(x => x.Hblid == job.Hblid);
                    modelReceivables = accAccountReceivableService.GetListObjectReceivableBySurcharges(charges);
                    foreach (var item in charges)
                    {
                        surchargeRepository.Delete(x => x.Id == item.Id, false);
                    }
                    //Xóa job OPS rep xóa luôn tờ khai rep
                    if(job.LinkSource == DocumentConstants.CLEARANCE_FROM_REPLICATE)
                    {
                        var clearancesRep = customDeclarationRepository.Get(x => x.JobNo == job.JobNo && x.Source == "Replicate");
                        if (clearancesRep != null)
                        {
                            foreach (var item in clearancesRep)
                            {
                                customDeclarationRepository.Delete(x => x.Id == item.Id, false);
                            }
                        }
                    }
                    else
                    {
                        var clearances = customDeclarationRepository.Get(x => x.JobNo == job.JobNo);
                        if (clearances != null)
                        {
                            foreach (var item in clearances)
                            {
                                item.JobNo = null;
                                item.ConvertTime = null;
                                customDeclarationRepository.Update(item, x => x.Id == item.Id, false);
                            }
                        }
                    }
                }
                // Xóa job rep thì xóa liên kết với job origin.
                if (job.LinkSource == DocumentConstants.CLEARANCE_FROM_REPLICATE)
                {
                    var jobOrigin = DataContext.First(x => x.ReplicatedId == job.Id);
                    if (jobOrigin != null)
                    {
                        jobOrigin.ReplicatedId = null;
                        DataContext.Update(jobOrigin, x => x.Id == jobOrigin.Id, false);
                    }
                }
            }
            result = DataContext.SubmitChanges();
            if (result.Success)
            {
                surchargeRepository.SubmitChanges();
                customDeclarationRepository.SubmitChanges();
            }
            return result;
        }

        /// <summary>
        /// Check if hbl+mbl no has been existed
        /// </summary>
        /// <param name="model">OpsTransactionModel</param>
        /// <param name="mblNo">MBL No of OpsTransaction</param>
        /// <param name="hblNo">HBL No of OpsTransaction</param>
        /// <returns></returns>
        public string CheckExist(OpsTransactionModel model, string mblNo, string hblNo)
        {
            var existedMblHbl = false;
            if (model == null)
            {
                existedMblHbl = DataContext.Any(x => x.Hwbno == hblNo
                && x.Mblno == mblNo
                && x.CurrentStatus != TermData.Canceled
                && x.OfficeId == currentUser.OfficeID
                && x.TransactionType==model.TransactionType
                );
            }
            else
            {
                var duplicateHBLMBL = DataContext.Get(x => x.Id != model.Id
                && x.CurrentStatus != TermData.Canceled
                && x.Hwbno == model.Hwbno
                && x.Mblno == model.Mblno
                && x.OfficeId == currentUser.OfficeID
                && x.TransactionType == model.TransactionType
                ).ToList();
                if (duplicateHBLMBL.Count > 0)
                {
                    if (model.ReplicatedId == null || model.ReplicatedId == Guid.Empty)
                    {
                        existedMblHbl = duplicateHBLMBL.Any(x => x.ReplicatedId != model.Id);
                        //if (!string.IsNullOrEmpty(model.ServiceNo))
                        //{
                        //    existedMblHbl = duplicateHBLMBL.Any(x => x.ReplicatedId != model.Id);
                        //}
                        //else
                        //{
                        //    existedMblHbl = true;
                        //}
                    }
                    else
                    {
                        existedMblHbl = duplicateHBLMBL.Any(x => x.Id != model.ReplicatedId);
                    }
                }

                //var existedMblHblData = DataContext.Get(x => x.Id != model.Id
                //&& x.CurrentStatus != TermData.Canceled
                //&& x.Hwbno == model.Hwbno
                //&& x.Mblno == model.Mblno
                //&& (x.ReplicatedId != Guid.Empty ? x.ReplicatedId == model.ReplicatedId : x.ReplicatedId != model.ReplicatedId)
                //).ToList();
            }
            if (existedMblHbl)
            {
                return stringLocalizer[DocumentationLanguageSub.MSG_MBLNO_HBNO_EXISTED].Value;
            }
            return null;
        }

        private bool CheckExistJobNo(Guid Id, string jobNo)
        {
            bool isExisted = false;
            if (Id == Guid.Empty)
            {
                return isExisted;
            }

            isExisted = DataContext.Any(x => x.Id != Id && x.JobNo == jobNo && x.CurrentStatus != TermData.Canceled);

            return isExisted;
        }

        public Crystal PreviewFormPLsheet(Guid id, string currency)
        {
            var shipment = DataContext.First(x => x.Id == id);
            Crystal result = null;
            var parameter = new FormPLsheetReportParameter
            {
                Contact = currentUser.UserName,
                CompanyName = sysOfficeRepo.Get(x => x.Id == shipment.OfficeId).FirstOrDefault().BranchNameEn,
                CompanyDescription = string.Empty,
                CompanyAddress1 = sysOfficeRepo.Get(x => x.Id == shipment.OfficeId).FirstOrDefault().AddressEn,
                CompanyAddress2 = DocumentConstants.COMPANY_CONTACT,
                Website = DocumentConstants.COMPANY_WEBSITE,
                CurrDecimalNo = 2,
                DecimalNo = 0,
                HBLList = shipment.Hwbno
            };

            result = new Crystal
            {
                ReportName = "FormPLsheet.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            var dataSources = new List<FormPLsheetReport> { };
            var agent = partnerRepository.Get(x => x.Id == shipment.AgentId).FirstOrDefault();
            var supplier = partnerRepository.Get(x => x.Id == shipment.SupplierId).FirstOrDefault();
            var surcharges = surchargeService.GetByHB(shipment.Hblid);
            var user = userRepository.Get(x => x.Id == shipment.SalemanId).FirstOrDefault();
            var units = unitRepository.Get();
            var polName = placeRepository.Get(x => x.Id == shipment.Pol).FirstOrDefault()?.NameEn;
            var podName = placeRepository.Get(x => x.Id == shipment.Pod).FirstOrDefault()?.NameEn;
            if (surcharges != null)
            {
                foreach (var surcharge in surcharges)
                {
                    var unitCode = units.FirstOrDefault(x => x.Id == surcharge.UnitId)?.Code;
                    bool isOBH = false;
                    decimal cost = 0;
                    decimal revenue = 0;
                    string partnerName = string.Empty;
                    if (surcharge.Type == DocumentConstants.CHARGE_OBH_TYPE)
                    {
                        isOBH = true;
                        partnerName = surcharge.PayerName;
                        cost = surcharge.Total;
                    }
                    if (surcharge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                    {
                        cost = surcharge.Total;
                    }
                    if (surcharge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                    {
                        revenue = surcharge.Total;
                    }

                    string _paymentStatus = string.Empty;
                    if (surcharge.Type == DocumentConstants.CHARGE_SELL_TYPE || surcharge.Type == DocumentConstants.CHARGE_OBH_TYPE)
                    {
                        if (surcharge.AcctManagementId != null && surcharge.AcctManagementId != Guid.Empty)
                        {
                            var acct = accMngtRepo.Get(x => x.Id == surcharge.AcctManagementId)?.FirstOrDefault();
                            _paymentStatus = acct?.PaymentStatus;
                        }
                    }

                    var surchargeRpt = new FormPLsheetReport();

                    surchargeRpt.COSTING = "COSTING";
                    surchargeRpt.TransID = shipment.JobNo?.ToUpper();
                    surchargeRpt.TransDate = (DateTime)shipment.DatetimeCreated;
                    surchargeRpt.HWBNO = shipment.Hwbno?.ToUpper();
                    surchargeRpt.MAWB = shipment.Mblno?.ToUpper();
                    surchargeRpt.PartnerName = string.Empty; //Not Use
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
                    surchargeRpt.ContainerNo = surcharge.ContNo;
                    surchargeRpt.OceanVessel = string.Empty;
                    surchargeRpt.LocalVessel = string.Empty;
                    surchargeRpt.FlightNo = shipment.FlightVessel?.ToUpper();
                    surchargeRpt.SeaImpVoy = string.Empty;
                    surchargeRpt.LoadingDate = ((DateTime)shipment.ServiceDate).ToString("dd MMM yyyy");
                    surchargeRpt.ArrivalDate = shipment.FinishDate != null ? ((DateTime)shipment.FinishDate).ToString("dd MMM yyyy") : null;
                    surchargeRpt.PayableAccount = surcharge.PartnerName?.ToUpper();
                    surchargeRpt.Description = surcharge.ChargeNameEn;
                    surchargeRpt.Curr = !string.IsNullOrEmpty(surcharge.CurrencyId) ? surcharge.CurrencyId.Trim() : string.Empty; //Currency of charge
                    surchargeRpt.Cost = cost + _decimalNumber; //Phí chi của charge
                    surchargeRpt.Revenue = revenue + _decimalNumber;
                    surchargeRpt.Exchange = 0;
                    surchargeRpt.Paid = (revenue > 0 || cost < 0) && isOBH == false ? false : true;
                    surchargeRpt.Docs = !string.IsNullOrEmpty(surcharge.InvoiceNo) ? surcharge.InvoiceNo : (surcharge.CreditNo ?? surcharge.DebitNo); //Ưu tiên: InvoiceNo >> CD Note Code  of charge
                    surchargeRpt.Notes = surcharge.Notes;
                    surchargeRpt.InputData = string.Empty; //Gán rỗng
                    surchargeRpt.Quantity = surcharge.Quantity + _decimalNumber; //Cộng thêm phần thập phân
                    surchargeRpt.UnitPrice = (surcharge.UnitPrice ?? 0) + _decimalMinNumber; //Cộng thêm phần thập phân
                    surchargeRpt.UnitPriceStr = surcharge.CurrencyId == DocumentConstants.CURRENCY_LOCAL ? string.Format("{0:n0}", (surcharge.UnitPrice ?? 0)) : string.Format("{0:n3}", (surcharge.UnitPrice ?? 0));
                    surchargeRpt.Unit = unitCode;
                    surchargeRpt.LastRevised = string.Empty;
                    surchargeRpt.OBH = isOBH;
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
                    surchargeRpt.TypeOfService = "CL";
                    surchargeRpt.ShipmentSource = "FREE-HAND";
                    surchargeRpt.RealCost = true;
                    //Đối với phí OBH thì NetAmountCurr gán bằng 0
                    surchargeRpt.NetAmountCurr = (surcharge.Type != DocumentConstants.CHARGE_OBH_TYPE ? currencyExchangeService.ConvertNetAmountChargeToNetAmountObj(surcharge, currency) : 0) + _decimalMinNumber; //NetAmount quy đổi về currency preview
                    surchargeRpt.GrossAmountCurr = currencyExchangeService.ConvertAmountChargeToAmountObj(surcharge, currency) + _decimalMinNumber;  //GrossAmount quy đổi về currency preview
                    surchargeRpt.PaymentStatus = _paymentStatus;
                    dataSources.Add(surchargeRpt);
                }
            }

            // Get path link to report
            CrystalEx._apiUrl = apiUrl.Value.Url;
            string folderDownloadReport = CrystalEx.GetLinkDownloadReports();
            var reportName = "FormPLsheet_" + shipment.JobNo + ".pdf";
            var _pathReportGenerate = folderDownloadReport + "/" + reportName.Replace("/", "_");
            result.PathReportGenerate = _pathReportGenerate;

            result.AddDataSource(dataSources);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);

            return result;
        }
        public HandleState Update(OpsTransactionModel model)
        {
            try
            {
                var detail = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();

                var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
                int code = GetPermissionToUpdate(new ModelUpdate { BillingOpsId = model.BillingOpsId, SaleManId = detail.SalemanId, UserCreated = detail.UserCreated, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId }, permissionRange);
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

                if (model.IsLinkJob && string.IsNullOrEmpty(model.UserCreatedLinkJob))
                {
                    model.UserCreatedLinkJob = currentUser.UserName;
                    model.DateCreatedLinkJob = DateTime.Now;
                }

                // Cập nhật ServiceHblId nếu null thì giữ ServiceHblId trước đó
                if (!string.IsNullOrEmpty(model.ServiceNo) && model.ServiceHblId == null && detail.ServiceHblId != null)
                {
                    var transaction = transactionRepository.Get(x => x.JobNo == model.ServiceNo).FirstOrDefault();
                    model.ServiceHblId = transactionDetailRepository.Any(x => x.JobId == transaction.Id && x.Id == detail.ServiceHblId) ?
                        transactionDetailRepository.Get(x => x.JobId == transaction.Id && x.Id == detail.ServiceHblId).FirstOrDefault()?.Id
                        : transactionDetailRepository.Get(x => x.JobId == transaction.Id)?.OrderBy(x => x.DatetimeModified).FirstOrDefault()?.Id;

                }

                OpsTransaction entity = mapper.Map<OpsTransaction>(model);
                var hs = DataContext.Update(entity, x => x.Id == model.Id);
                if (hs.Success)
                {
                    if (model.CsMawbcontainers != null)
                    {
                        var hsContainer = mawbcontainerService.UpdateMasterBill(model.CsMawbcontainers, model.Id);
                    }
                    //Cập nhật JobNo, Mbl, Hbl cho các charge của housebill
                    var hsSurcharge = UpdateSurchargeOfHousebill(model);

                    // Cập nhật MBL, HBL cho các phiếu tạm ứng
                    HandleState hsAdvanceRq = UpdateMblHblAdvanceRequest(model);

                    // update saleman cdnote type debit/invoice
                    var cdnote = acctCdNoteRepository.Get(x => x.JobId == model.Id && x.Type != "CREDIT").FirstOrDefault();
                    if (cdnote != null && cdnote?.SalemanId != model.SalemanId)
                    {
                        cdnote.SalemanId = model.SalemanId;
                        acctCdNoteRepository.Update(cdnote, x => x.Id == cdnote.Id);
                    }
                }
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_Update_OpsTransaction_Log", ex.ToString());
                return new HandleState(ex.Message);
            }

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
            foreach (var item in list)
            {
                var customer = new CatPartner();
                if (item.AccountNo == null)
                {
                    customer = partnerRepository.Get(x => x.TaxCode == item.PartnerTaxCode)?.FirstOrDefault();
                }
                else
                {
                    customer = partnerRepository.Get(x => x.AccountNo == item.AccountNo)?.FirstOrDefault();
                }

                if (customer == null) notFoundPartnerTaxCodeMessages += (item.AccountNo ?? item.PartnerTaxCode) + ", ";
                string dupMessage = CheckExist(item, item.Id);
                if (dupMessage != null)
                {
                    duplicateMessages += dupMessage + ", ";
                }
            }
            if (notFoundPartnerTaxCodeMessages.Length > 0)
            {
                notFoundPartnerTaxCodeMessages = "Customer '" + notFoundPartnerTaxCodeMessages.Substring(0, notFoundPartnerTaxCodeMessages.Length - 2) + "' Not found";
                result = new ResultHandle { Status = false, Message = notFoundPartnerTaxCodeMessages, Data = 403 };
                return result;
            }
            if (duplicateMessages.Length > 0)
            {
                duplicateMessages = duplicateMessages.Substring(0, duplicateMessages.Length - 2);
                result = new ResultHandle { Status = false, Message = duplicateMessages, Data = 403 };
                return result;
            }
            var permissionDetailRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Detail);
            var hs = GetPermissionDetailConvertClearances(list, permissionDetailRange);
            if (hs.Success == true)
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
                if (customDeclarationRepository.Any(x => x.ClearanceNo == model.ClearanceNo
                && x.ClearanceDate == model.ClearanceDate
                && x.Source != DocumentConstants.CLEARANCE_FROM_REPLICATE))
                {
                    message = string.Format("This clearance no '{0}' and clearance date '{1}' has been existed", model.ClearanceNo, model.ClearanceDate);
                }
            }
            else
            {
                if (customDeclarationRepository.Any(x => (x.ClearanceNo == model.ClearanceNo
                && x.Id != id
                && x.ClearanceDate == model.ClearanceDate
                && x.Source != DocumentConstants.CLEARANCE_FROM_REPLICATE)))
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

        public HandleState UpdateSurchargeOfHousebill(OpsTransactionModel model)
        {
            try
            {
                var surcharges = surchargeRepository.Where(x => x.Hblid == model.Hblid);
                foreach (var surcharge in surcharges)
                {
                    surcharge.JobNo = model.JobNo;
                    surcharge.Mblno = model.Mblno;
                    surcharge.Hblno = model.Hwbno;
                    surcharge.DatetimeModified = DateTime.Now;
                    surcharge.UserModified = currentUser.UserID;
                    var hsUpdateSurcharge = surchargeRepository.Update(surcharge, x => x.Id == surcharge.Id, false);
                }
                var sm = surchargeRepository.SubmitChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                string logErr = String.Format("Có lỗi khi cập nhật JobNo {0}, MBLNo {1}, HBLNo {2} trong CsShipmentSurcharge by {3} at {4} \n {5}", model.JobNo, model.Mblno, model.Hwbno, currentUser.UserName, DateTime.Now, ex.ToString());
                new LogHelper("eFMS_Update_CsShipmentSurcharge_Log", logErr);
                return new HandleState(ex.Message);
            }
        }

        public HandleState UpdateMblHblAdvanceRequest(OpsTransactionModel model)
        {
            HandleState hs = new HandleState();
            try
            {
                IQueryable<AcctAdvanceRequest> advR = accAdvanceRequestRepository.Get(x => x.Hblid == model.Hblid);
                if (advR != null)
                {
                    foreach (var item in advR)
                    {
                        item.Mbl = model.Mblno;
                        item.Hbl = model.Hwbno;
                        item.DatetimeModified = DateTime.Now;
                        item.UserModified = currentUser.UserID;

                        accAdvanceRequestRepository.Update(item, x => x.Id == item.Id, false);
                    }

                    hs = accAdvanceRequestRepository.SubmitChanges();
                }
                return hs;

            }
            catch (Exception ex)
            {
                string logErr = String.Format("Có lỗi khi cập nhật MBLNo {0}, HBLNo {1} trong acctAdvanceRequest by {2} at {3} \n {4}", model.Mblno, model.Hwbno, currentUser.UserName, DateTime.Now, ex.ToString());
                new LogHelper("eFMS_Update_Advance_Log", logErr);
                return new HandleState(ex.Message);
            }
        }

        /// <summary>
        /// Add a duplicate job to OpsTransaction
        /// </summary>
        /// <param name="model">OpsTransactionModel</param>
        /// <returns></returns>
        public ResultHandle ImportDuplicateJob(OpsTransactionModel model, out List<Guid> surchargeIds)
        {
            surchargeIds = new List<Guid>(); // ds các charge phí update công nợ.
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            int code = GetPermissionToUpdate(new ModelUpdate { BillingOpsId = model.BillingOpsId, SaleManId = model.SalemanId, UserCreated = model.UserCreated, CompanyId = model.CompanyId, OfficeId = model.OfficeId, DepartmentId = model.DepartmentId, GroupId = model.GroupId }, permissionRange);
            if (code == 403 || model.LinkSource == DocumentConstants.CLEARANCE_FROM_REPLICATE) return new ResultHandle { Status = false, Message = "You can't duplicate this job." };

            List<CsMawbcontainer> newContainers = new List<CsMawbcontainer>();
            List<CsShipmentSurcharge> newSurcharges = new List<CsShipmentSurcharge>();
            List<CsShipmentSurcharge> newSurchargesReplicates = new List<CsShipmentSurcharge>();


            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    // Create model import
                    Guid _hblId = model.Hblid;
                    Guid? _replicateId = model.ReplicatedId;

                    model.Hblid = Guid.NewGuid();
                    model.JobNo = CreateJobNoOps(model.TransactionType);
                    model.UserModified = currentUser.UserID;
                    model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                    model.UserCreated = currentUser.UserID;
                    model.GroupId = currentUser.GroupId;
                    model.DepartmentId = currentUser.DepartmentId;
                    model.OfficeId = currentUser.OfficeID;
                    model.CompanyId = currentUser.CompanyID;

                    model.IsLocked = false; // Luôn luôn mở job khi duplicate.
                    model.ReplicatedId = null;
                    model.LinkSource = null;
                    model.Hwbno = model.Hwbno?.Trim();
                    model.Mblno = model.Mblno?.Trim();
                    List<SysUserLevel> dataUserLevels = userlevelRepository.Get(x => x.UserId == model.SalemanId).ToList();
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
                    int dayStatus = (int)(model.ServiceDate.Value.Date - DateTime.Now.Date).TotalDays;
                    if (dayStatus > 0)
                    {
                        model.CurrentStatus = TermData.InSchedule;
                    }
                    else
                    {
                        model.CurrentStatus = TermData.Processing;
                    }

                    // Update list Container
                    List<CsMawbcontainerModel> listContainerOld = model.CsMawbcontainers;
                    if (listContainerOld != null)
                    {
                        List<CsMawbcontainer> masterContainers = GetNewMasterBillContainer(model.Id, model.Hblid, listContainerOld);
                        newContainers.AddRange(masterContainers);
                    }

                    List<CsShipmentSurcharge> listSurCharge = CopySurChargeToNewJob(_hblId, model);
                    if (listSurCharge?.Count() > 0)
                    {
                        newSurcharges.AddRange(listSurCharge);
                    }

                    model.IsLinkFee = null;
                    model.DateCreatedLinkJob = null;
                    model.UserCreatedLinkJob = null;

                    OpsTransaction entity = mapper.Map<OpsTransaction>(model);

                    if (model.IsReplicate == true && _replicateId != null)
                    {
                        SysSettingFlow settingFlowOffice = settingFlowRepository.Get(x => x.OfficeId == currentUser.OfficeID && x.Flow == "Replicate")?.FirstOrDefault();
                        if (settingFlowOffice != null && settingFlowOffice.ReplicateOfficeId != null)
                        {
                            SysUserLevel dataUserLevel = userlevelRepository.Get(x => x.UserId == currentUser.UserID
                                                            && x.OfficeId == settingFlowOffice.ReplicateOfficeId).FirstOrDefault();
                            SysOffice officeReplicate = sysOfficeRepo.Get(x => x.Id == settingFlowOffice.ReplicateOfficeId)?.FirstOrDefault();

                            if (dataUserLevel != null)
                            {
                                OpsTransaction replicateJob = DataContext.Get(x => x.Id == _replicateId)?.FirstOrDefault();
                                if (replicateJob != null)
                                {
                                    OpsTransaction entityReplicate = MappingReplicateJob(entity, dataUserLevel);
                                    DataContext.Add(entityReplicate, false);

                                    entity.ReplicatedId = entityReplicate.Id;
                                    entityReplicate.JobNo = GeneratePreFixReplicate() + entity.JobNo;

                                    List<CsShipmentSurcharge> listSurChargeReplicate = CopySurChargeToNewJob(replicateJob.Hblid, entityReplicate, false);
                                    if (listSurChargeReplicate?.Count() > 0)
                                    {
                                        newSurcharges.AddRange(listSurChargeReplicate);
                                    }
                                }

                            };
                        }
                    };
                    DataContext.Add(entity, false);
                    HandleState hs = DataContext.SubmitChanges();

                    if (hs.Success)
                    {
                        if (newContainers.Count > 0)
                        {
                            HandleState hsContainer = csMawbcontainerRepository.Add(newContainers);
                        }

                        if (newSurcharges.Count() > 0)
                        {
                            HandleState hsSurcharges = surchargeRepository.Add(newSurcharges);
                        }

                        trans.Commit();
                        surchargeIds = newSurcharges.Select(x => x.Id).ToList();
                        return new ResultHandle { Status = true, Message = "The job have been saved!", Data = entity };
                    }
                    trans.Rollback();
                    return new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("eFMS_DUPLICATE_JOB_LOG", ex.ToString());
                    return new ResultHandle { Status = false, Message = "Job can't be saved!" };
                }
                finally
                {
                    trans.Dispose();
                }
            }

        }

        /// <summary>
        /// Get list surcharges from old job to new job
        /// </summary>
        /// <param name="_oldHblId"></param>
        /// <param name="_newHblId"></param>
        /// <returns></returns>
        private List<CsShipmentSurcharge> CopySurChargeToNewJob(Guid _oldHblId, OpsTransaction shipment, bool isOrigin = true)
        {
            List<CsShipmentSurcharge> surCharges = new List<CsShipmentSurcharge>();
            IQueryable<CsShipmentSurcharge> charges = surchargeRepository.Get(x => x.Hblid == _oldHblId && x.IsFromShipment == true && string.IsNullOrEmpty(x.LinkChargeId));
            OpsTransaction jobRepOld = DataContext.Get(x => x.Hblid == _oldHblId)?.FirstOrDefault();

            if (isOrigin == false)
            {
                // Không lấy phí đã AutoRate | LINK_FEE ( phí link từ Buy(ChargeOrg) làm thanh toán qua sell (ChargeLinkId)
                OpsTransaction JobRepOld = DataContext.Get(x => x.Hblid == _oldHblId)?.FirstOrDefault();
                List<CsLinkCharge> csLinkFee = csLinkChargeRepository.Get(x => x.JobNoLink == JobRepOld.JobNo && x.LinkChargeType == DocumentConstants.LINK_CHARGE_TYPE_AUTO_RATE).ToList();
                if (csLinkFee.Count() > 0)
                {
                    List<string> listChargeExisted = csLinkFee.Select(x => x.ChargeLinkId).ToList();
                    charges = charges.Where(x => !listChargeExisted.Contains(x.Id.ToString()));
                }
            }

            decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;
            if (charges.Select(x => x.Id).Count() != 0)
            {
                foreach (var charge in charges)
                {
                    var chargeItem = charge.GetType().GetProperties();
                    CsShipmentSurcharge item = new CsShipmentSurcharge();
                    foreach (var i in chargeItem)
                    {
                        item.GetType().GetProperty(i.Name).SetValue(item, i.GetValue(charge, null), null);
                    }

                    item.Id = Guid.NewGuid();
                    item.UserCreated = currentUser.UserID;
                    item.DatetimeCreated = DateTime.Now;

                    item.Soano = null;
                    item.PaySoano = null;
                    item.CreditNo = null;
                    item.DebitNo = null;
                    item.Soaclosed = null;
                    item.SettlementCode = null;
                    item.AcctManagementId = null;
                    item.InvoiceNo = null;
                    item.SeriesNo = null;
                    item.InvoiceDate = null;
                    item.VoucherId = null;
                    item.VoucherIddate = null;
                    item.SyncedFrom = null;
                    item.PaySyncedFrom = null;
                    item.ReferenceNo = null;
                    item.ExchangeDate = DateTime.Now;
                    item.AdvanceNoFor = null;

                    #region -- Tính lại giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                    //** FinalExchangeRate = null do cần tính lại dựa vào ExchangeDate mới
                    item.FinalExchangeRate = null;

                    var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item, kickBackExcRate);
                    item.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                    item.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                    item.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                    item.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                    item.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                    item.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                    item.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                    #endregion -- Tính lại giá trị các field: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                    item.ClearanceNo = null;
                    item.AdvanceNo = null;
                    item.PayerAcctManagementId = null;
                    item.VoucherIdre = null;
                    item.VoucherIdredate = null;
                    item.LinkChargeId = null;
                    item.LinkFee = null;
                    item.ModifiedDateLinkFee = null;
                    item.UserIdLinkFee = null;

                    item.JobNo = shipment.JobNo;
                    item.Hblno = shipment.Hwbno;
                    item.Mblno = shipment.Mblno;
                    item.OfficeId = shipment.OfficeId;
                    item.Hblid = shipment.Hblid;
                    item.CombineBillingNo = null;
                    item.ObhcombineBillingNo = null;

                    if (jobRepOld.SupplierId != shipment.SupplierId && item.IsRefundFee == true)
                    {
                        item.PaymentObjectId = shipment.SupplierId;
                    }

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

        public int CheckUpdateMBL(OpsTransactionModel model, out string mblNo, out List<string> advs)
        {
            mblNo = string.Empty;
            advs = new List<string>();
            int errorCode = 0;  // 1|2
            bool hasChargeSynced = false;
            bool hasAdvanceRequest = false;

            if (DataContext.Any(x => x.Id == model.Id
            && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
            && ((x.Mblno ?? "").ToLower() != (model.Mblno ?? "") || (x.Hwbno ?? "").ToLower() != (model.Hwbno ?? ""))))
            {
                OpsTransaction shipment = DataContext.Get(x => x.Id == model.Id)?.FirstOrDefault();
                if (shipment != null)
                {
                    hasChargeSynced = surchargeRepository.Any(x => x.Hblid == shipment.Hblid && (!string.IsNullOrEmpty(x.SyncedFrom) || !string.IsNullOrEmpty(x.PaySyncedFrom)));
                }

                if (hasChargeSynced)
                {
                    errorCode = 1;
                    mblNo = shipment.Mblno;
                }
                else
                {
                    var query = from advR in accAdvanceRequestRepository.Get(x => x.JobId == shipment.JobNo)
                                join adv in accAdvancePaymentRepository.Get(x => x.SyncStatus == "Synced") on advR.AdvanceNo equals adv.AdvanceNo
                                select adv.AdvanceNo;

                    if (query != null && query.Count() > 0)
                    {
                        hasAdvanceRequest = true;
                        advs = query.Distinct().ToList();
                    }
                    if (hasAdvanceRequest)
                    {
                        errorCode = 2;
                        mblNo = shipment.Mblno;
                    }
                }
            }

            return errorCode;
        }

        private string GeneratePreFixReplicate()
        {
            string preFix = "R";
            SysSettingFlow settingFlowOffice = settingFlowRepository.Get(x => x.OfficeId == currentUser.OfficeID && x.Flow == "Replicate")?.FirstOrDefault();
            if (settingFlowOffice != null && settingFlowOffice.ReplicateOfficeId != null)
            {
                if (!string.IsNullOrEmpty(settingFlowOffice.ReplicatePrefix))
                {
                    preFix = settingFlowOffice.ReplicatePrefix;
                }
            }
            return preFix;
        }

        public ResultHandle ChargeFromReplicate(string arrJob, out List<Guid> Ids)
        {
            new LogHelper("[EFMS_OPSTRANSACTIONSERVICE_CHARGEFROMREPLICATE]", "\n-------------------------------------------------------------------------\n");
            string logMessage = string.Format(" *  \n [START][USER]: {0}{1} * ", DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"), currentUser.UserName);
            new LogHelper("[EFMS_OPSTRANSACTIONSERVICE_CHARGEFROMREPLICATE]", logMessage);
            Ids = new List<Guid>();

            ResultHandle hs = new ResultHandle();
            List<CsShipmentSurcharge> surchargeAdds = new List<CsShipmentSurcharge>();
            CatPartner partnerInternal = new CatPartner();
            var charges = GetChargesToLinkCharge(new Guid(currentUser.UserID), arrJob);

            using (var trans = surchargeRepository.DC.Database.BeginTransaction())
            {
                try
                {
                    if (charges != null && charges.Count() > 0)
                    {
                        foreach (var charge in charges)
                        {
                            CsShipmentSurcharge surcharge = new CsShipmentSurcharge();

                            if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                            {
                                //var catCharge = catChargeRepository.Get(x => x.DebitCharge == charge.ChargeId && x.DebitCharge != null).FirstOrDefault();
                                //if (catCharge != null) { surcharge.ChargeId = catCharge.Id; } else continue;
                                if (charge.CreditCharge == null)
                                {
                                    continue;
                                }
                                charge.Type = DocumentConstants.CHARGE_BUY_TYPE;
                                charge.ChargeId = charge.CreditCharge ?? Guid.Empty;

                                if (!string.IsNullOrEmpty(charge.PartnerInternal_Id))
                                    charge.PaymentObjectId = charge.PartnerInternal_Id;
                            }
                            else if (charge.Type == DocumentConstants.CHARGE_OBH_TYPE)
                            {
                                //[17/01/2022][Nếu phí hiện trường thì set thêm sm done]
                                if (!string.IsNullOrEmpty(charge.SettlementCode) && charge.IsFromShipment == false && acctSettlementPayment.Get(x => x.SettlementNo == charge.SettlementCode
                               && (x.StatusApproval == "Department Manager Approved" || x.StatusApproval == "Done")).FirstOrDefault() == null)
                                    continue;

                                //[01/03/2022][17133][Nếu phí OBH có Buying Mapping]
                                //var catCharge = catChargeRepository.Get(x => x.Id == charge.ChargeId && x.CreditCharge != null).FirstOrDefault();
                                if (charge.CreditCharge != null)
                                {
                                    charge.ChargeId = charge.CreditCharge ?? Guid.Empty;
                                    charge.Type = DocumentConstants.CHARGE_BUY_TYPE;
                                    charge.PayerId = null;
                                    if (!string.IsNullOrEmpty(charge.PartnerInternal_Id))
                                        charge.PaymentObjectId = charge.PartnerInternal_Id;
                                }
                                else
                                {
                                    charge.Type = DocumentConstants.CHARGE_OBH_TYPE;
                                    if (!string.IsNullOrEmpty(charge.PartnerInternal_Id))
                                        charge.PayerId = charge.PartnerInternal_Id;
                                }
                            }
                            else { continue; }

                            var propInfo = charge.GetType().GetProperties();
                            foreach (var item in propInfo)
                            {
                                var p = surcharge.GetType().GetProperty(item.Name);
                                if (p != null) { p.SetValue(surcharge, item.GetValue(charge, null), null); }
                            }

                            surcharge.LinkChargeId = charge.Id.ToString();
                            surcharge.Id = Guid.NewGuid();
                            surcharge.JobNo = charge.JobNo_Org ?? "";
                            surcharge.Hblid = charge.HBLId_Org;
                            surcharge.Hblno = charge.HBLNo_Org;
                            surcharge.Mblno = charge.MBLNo_Org;
                            surcharge.OfficeId = charge.OfficeId_Org;

                            surcharge.Soano = null;
                            surcharge.PaySoano = null;
                            surcharge.CreditNo = null;
                            surcharge.DebitNo = null;
                            surcharge.SettlementCode = null;
                            surcharge.VoucherId = null;
                            surcharge.VoucherIddate = null;
                            surcharge.VoucherIdre = null;
                            surcharge.VoucherIdredate = null;
                            surcharge.AcctManagementId = null;
                            surcharge.PayerAcctManagementId = null;
                            surcharge.IsFromShipment = true;
                            surcharge.SyncedFrom = null;
                            surcharge.PaySyncedFrom = null;
                            surcharge.AdvanceNo = null;
                            surcharge.CombineBillingNo = null;
                            surcharge.AdvanceNoFor = null;
                            surcharge.ObhcombineBillingNo = null;
                            surcharge.TypeOfFee = null;
                            surcharge.ReferenceNo = null;

                            surcharge.UserCreated = currentUser.UserID;
                            surcharge.DatetimeCreated = DateTime.Now;

                            surchargeAdds.Add(surcharge);
                        }
                    }
                    if (surchargeAdds.Count > 0)
                    {
                        logMessage = string.Format(" *  \n [TIME]:{0}[SURCHARGE_ADD]: {1} * ", DateTime.Now.ToString("DD/MM/YYYY hh:mm:ss"), JsonConvert.SerializeObject(surchargeAdds));
                        new LogHelper("[EFMS_OPSTRANSACTIONSERVICE_CHARGEFROMREPLICATE]", logMessage);
                        surchargeRepository.Add(surchargeAdds, false);
                        var result = surchargeRepository.SubmitChanges();
                        if (result.Success)
                            trans.Commit();

                        foreach (var sur in surchargeAdds)
                        {
                            var charge = surchargeRepository.Get(x => x.Id == Guid.Parse(sur.LinkChargeId)).FirstOrDefault();
                            if (charge != null)
                            {
                                charge.LinkChargeId = sur.Id.ToString();
                                var resultUpdate = surchargeRepository.Update(charge, x => x.Id == charge.Id, false);
                            }
                        }
                        HandleState hsSurchargeAdd = surchargeRepository.SubmitChanges();
                        if (hsSurchargeAdd.Success)
                        {
                            Ids.AddRange(surchargeAdds.Select(x => x.Id));
                        }
                    }
                    else
                    {
                        trans.Rollback();
                        return new ResultHandle { Status = true, Message = "Empty Charge From Replicate", Data = null };
                    }
                    var jobNos = surchargeAdds.GroupBy(item => item.JobNo).Select(group => new { JobNo = group.Key, Charges = group.ToList() }).ToList();
                    return new ResultHandle { Status = true, Message = "The " + jobNos.Count() + " job is linked Charge successfully", Data = null };
                }
                catch (Exception ex)
                {

                    trans.Rollback();
                    new LogHelper("[EFMS_OPSTRANSACTIONSERVICE_CHARGEFROMREPLICATE]\n[ERROR]", ex.ToString());
                    return new ResultHandle { Status = false, Message = "Job can't be charge from replicate !" };
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        private SysOffice GetInfoOfficeOfUser(Guid? officeId)
        {
            SysOffice result = sysOfficeRepo.Get(x => x.Id == officeId).FirstOrDefault();
            return result;
        }

        public async Task<HandleState> ReplicateJobs(ReplicateIds model)
        {
            HandleState hs = new HandleState();
            Guid? IdsRep = Guid.Empty;
            List<OpsTransaction> opsReps = new List<OpsTransaction>();
            List<Guid> IdsJobUpdate = new List<Guid>();

            if (model.Ids.Count == 0)
            {
                return hs;
            }

            foreach (var Id in model.Ids)
            {
                OpsTransaction job = DataContext.Get(x => x.Id == Id)?.FirstOrDefault();

                if (job.ReplicatedId != null)
                {
                    return new HandleState((object)string.Format("Job replicate {0} is existed", job.JobNo));
                }

                if (job.LinkSource == DocumentConstants.CLEARANCE_FROM_REPLICATE)
                {
                    return new HandleState((object)string.Format("{0} is not origin job", job.JobNo));
                }

                SysSettingFlow settingFlowOffice = settingFlowRepository.Get(x => x.OfficeId == currentUser.OfficeID && x.Flow == "Replicate")?.FirstOrDefault();
                if (settingFlowOffice != null && settingFlowOffice.ReplicateOfficeId != null)
                {
                    SysUserLevel dataUserLevel = userlevelRepository.Get(x => x.UserId == currentUser.UserID
                     && x.OfficeId == settingFlowOffice.ReplicateOfficeId).FirstOrDefault();
                    SysOffice officeReplicate = sysOfficeRepo.Get(x => x.Id == settingFlowOffice.ReplicateOfficeId)?.FirstOrDefault();

                    if (dataUserLevel == null)
                    {
                        return new HandleState((object)
                            string.Format("You don't have permission at {0}, Please you check with system admin!", officeReplicate.ShortName)
                            );
                    };
                    // mapping saleman
                    var salemanDefault = userRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault();
                    SaleManPermissionModel salemanPermissionInfoReplicate = GetAndUpdateSaleManInfo(salemanDefault.Id);

                    var propInfo = job.GetType().GetProperties();
                    OpsTransaction entityReplicate = new OpsTransaction();
                    foreach (var item in propInfo)
                    {
                        entityReplicate.GetType().GetProperty(item.Name).SetValue(entityReplicate, item.GetValue(job, null), null);
                    }
                    entityReplicate.DatetimeCreated = DateTime.Now;
                    entityReplicate.UserCreated = currentUser.UserID;
                    entityReplicate.UserModified = currentUser.UserID;
                    entityReplicate.DatetimeModified = DateTime.Now;
                    entityReplicate.JobNo = GeneratePreFixReplicate() + job.JobNo;
                    entityReplicate.Id = Guid.NewGuid();
                    entityReplicate.Hblid = Guid.NewGuid();
                    entityReplicate.ServiceNo = null;
                    entityReplicate.ServiceHblId = null;
                    entityReplicate.OfficeId = dataUserLevel.OfficeId;
                    entityReplicate.DepartmentId = dataUserLevel.DepartmentId;
                    entityReplicate.GroupId = dataUserLevel.GroupId;
                    entityReplicate.CompanyId = dataUserLevel.CompanyId;
                    entityReplicate.SalesGroupId = salemanPermissionInfoReplicate.SalesGroupId;
                    entityReplicate.SalesDepartmentId = salemanPermissionInfoReplicate.SalesDepartmentId;
                    entityReplicate.SalesOfficeId = salemanPermissionInfoReplicate.SalesOfficeId;
                    entityReplicate.SalesCompanyId = salemanPermissionInfoReplicate.SalesCompanyId;
                    entityReplicate.SalemanId = salemanDefault.Id;
                    entityReplicate.IsLocked = false;
                    entityReplicate.IsLinkFee = false;
                    entityReplicate.LinkSource = DocumentConstants.CLEARANCE_FROM_REPLICATE;

                    hs = DataContext.Add(entityReplicate);
                    if (hs.Success)
                    {
                        job.ReplicatedId = entityReplicate.Id;
                        hs = DataContext.Update(job, x => x.Id == Id);

                        var customD = customDeclarationRepository.First(x => x.JobNo == job.JobNo);
                        // Add tk ảo.
                        if (customD != null)
                        {
                            var cdInfo = customD.GetType().GetProperties();
                            CustomsDeclaration cdReplicate = new CustomsDeclaration();

                            foreach (var item in cdInfo)
                            {
                                if (item.Name != "Id")
                                {
                                    cdReplicate.GetType().GetProperty(item.Name).SetValue(cdReplicate, item.GetValue(customD, null), null);
                                }
                            }

                            cdReplicate.ConvertTime = DateTime.Now;
                            cdReplicate.DatetimeCreated = DateTime.Now;
                            cdReplicate.DatetimeModified = DateTime.Now;
                            cdReplicate.UserCreated = currentUser.UserID;
                            cdReplicate.UserModified = currentUser.UserID;
                            cdReplicate.Source = DocumentConstants.CLEARANCE_FROM_REPLICATE;
                            cdReplicate.GroupId = currentUser.GroupId;
                            cdReplicate.DepartmentId = currentUser.DepartmentId;
                            cdReplicate.OfficeId = settingFlowOffice.ReplicateOfficeId; // theo Office của Replicatate setting
                            cdReplicate.CompanyId = currentUser.CompanyID;
                            cdReplicate.JobNo = entityReplicate.JobNo;

                            HandleState hsCd = customDeclarationRepository.Add(cdReplicate);
                        }

                        // copy assignment
                        var assign = opsStageAssignedRepository.Get(x => x.JobId == job.Id);
                        if (assign?.Count() > 0)
                        {
                            var listStage = new List<CsStageAssignedModel>();
                            foreach (var item in assign)
                            {
                                var opsAssignProp = item.GetType().GetProperties();
                                CsStageAssignedModel newOpsAssigned = new CsStageAssignedModel();
                                foreach (var prop in opsAssignProp)
                                {
                                    newOpsAssigned.GetType().GetProperty(prop.Name).SetValue(newOpsAssigned, prop.GetValue(item, null), null);
                                }

                                newOpsAssigned.DatetimeCreated = DateTime.Now;
                                newOpsAssigned.DatetimeModified = DateTime.Now;
                                newOpsAssigned.UserCreated = currentUser.UserID;
                                newOpsAssigned.UserModified = currentUser.UserID;
                                newOpsAssigned.Id = Guid.NewGuid();
                                newOpsAssigned.JobId = entityReplicate.Id;

                                listStage.Add(newOpsAssigned);
                            }
                            HandleState hsAssign = await csStageAssignedService.AddMultipleStageAssigned(job.Id, listStage);

                        }
                    }
                };
            }
            return hs;
        }

        public ResultHandle AutoRateReplicate(string settleNo, string jobNo)
        {
            List<CsShipmentSurcharge> surchargeSells = new List<CsShipmentSurcharge>();
            var hs = new ResultHandle();
            var surchargesAddHis = new List<CsLinkCharge>();
            var chargesBuyReps = GetChargeAutoRateReplicate(settleNo, jobNo);
            {
                try
                {
                    foreach (var chargeBuy in chargesBuyReps)
                    {
                        if (chargeBuy.DebitCharge == null)
                            continue;
                        if (chargesBuyReps.Where(x => x.ChargeId == chargeBuy.DebitCharge).FirstOrDefault() != null)
                            continue;

                        CsShipmentSurcharge surcharge = MapChargeBuytoSell(chargeBuy);
                        surchargeSells.Add(surcharge);

                        var surchargesHis = new CsLinkCharge();
                        surchargesHis.Id = Guid.NewGuid();
                        surchargesHis.JobNoOrg = surcharge.JobNo;
                        surchargesHis.ChargeOrgId = chargeBuy.Id.ToString();
                        surchargesHis.JobNoLink = chargeBuy.JobNo;
                        surchargesHis.ChargeLinkId = surcharge.Id.ToString();
                        surchargesHis.DatetimeCreated = surchargesHis.DatetimeModified = DateTime.Now;
                        surchargesHis.UserCreated = surchargesHis.UserModified = DocumentConstants.USER_EFMS_SYSTEM;
                        surchargesHis.LinkChargeType = "AUTO_RATE";

                        surchargesAddHis.Add(surchargesHis);
                    }
                    var result = new HandleState();
                    if (surchargeSells.Count > 0 || surchargesAddHis.Count > 0)
                    {
                        var resultUpd = databaseUpdateService.InsertChargesAutoRateToDB(surchargeSells, surchargesAddHis);
                        if (resultUpd.TotalRowAffected > 0)
                        {
                            result = new HandleState(resultUpd.Status, (object)"AUTORATEREPLICATE SCCUESS");
                        }
                        else
                        {
                            result = new HandleState(resultUpd.Status, (object)"AUTORATEREPLICATE CHARGE EMPTY");
                        }
                    }
                    new LogHelper("eFMS_AUTORATEREPLICATE", "Success: " + result.Success + "\n Status: " + result.Message + "\n SettleNo/JobNo: " + (!string.IsNullOrEmpty(settleNo) ? settleNo : jobNo)
                        + "\n Surcharges: " + JsonConvert.SerializeObject(chargesBuyReps));
                }
                catch (Exception ex)
                {
                    new LogHelper("eFMS_AUTORATEREPLICATE", ex.ToString());
                    hs.Status = false;
                    hs.Message = "AUTORATEREPLICATE FALSE";
                }
            }
            return hs;
        }

        private CsShipmentSurcharge MapChargeBuytoSell(sp_GetChargeAutoRateReplicate chargeBuy)
        {
            CsShipmentSurcharge surcharge = new CsShipmentSurcharge();

            var propInfo = chargeBuy.GetType().GetProperties();
            foreach (var item in propInfo)
            {
                var p = surcharge.GetType().GetProperty(item.Name);
                if (p != null) { p.SetValue(surcharge, item.GetValue(chargeBuy, null), null); }
            }

            var datetimeCR = new DateTime(2023, 1, 1); // [CR:18726] update 8 -> 10% from 1/1/2023
            surcharge.Id = Guid.NewGuid();
            surcharge.Type = DocumentConstants.CHARGE_SELL_TYPE;
            surcharge.ChargeId = chargeBuy.DebitCharge ?? Guid.Empty;

            surcharge.Quantity = chargeBuy.ServiceDate.Value < datetimeCR ? 1 : chargeBuy.Quantity;
            surcharge.Vatrate = chargeBuy.ServiceDate.Value < datetimeCR ? 8 : 10; // [CR:18726] update 8 -> 10% from 1/1/2023

            surcharge.Soano = null;
            surcharge.PaySoano = null;
            surcharge.CreditNo = null;
            surcharge.DebitNo = null;
            surcharge.SettlementCode = null;
            surcharge.VoucherId = null;
            surcharge.VoucherIdre = null;
            surcharge.VoucherIddate = null;
            surcharge.VoucherIdredate = null;
            surcharge.AcctManagementId = null;
            surcharge.InvoiceNo = null;
            surcharge.InvoiceDate = null;
            surcharge.LinkFee = null;
            surcharge.LinkChargeId = null;
            surcharge.Notes = null;
            surcharge.IsFromShipment = true;
            surcharge.SyncedFrom = null;
            surcharge.PaySyncedFrom = null;
            surcharge.AdvanceNo = null;
            surcharge.CombineBillingNo = null;
            surcharge.AdvanceNoFor = null;
            surcharge.ObhcombineBillingNo = null;
            surcharge.TypeOfFee = null;
            surcharge.ReferenceNo = null;
            surcharge.SeriesNo = null;

            if (chargeBuy.CurrencyId == "VND")
            {
                var per = (chargeBuy.ServiceDate.Value < datetimeCR ? (double)chargeBuy.Total : (double)chargeBuy.UnitPrice) / (double)0.76;
                surcharge.UnitPrice = NumberHelper.RoundNumber((decimal)per / 10000, 0) * 10000;
                surcharge.NetAmount = surcharge.UnitPrice * surcharge.Quantity;
                surcharge.Total = surcharge.NetAmount + ((surcharge.NetAmount * surcharge.Vatrate) / 100) ?? 0;
            }
            else
            {
                surcharge.Vatrate = chargeBuy.Vatrate;
                surcharge.Quantity = chargeBuy.Quantity;
            }

            var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(surcharge, 0);
            surcharge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
            surcharge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
            surcharge.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
            surcharge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
            surcharge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
            surcharge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
            surcharge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)

            if (!string.IsNullOrEmpty(chargeBuy.PartnerInternal_Id))
            {
                surcharge.PaymentObjectId = chargeBuy.PartnerInternal_Id;
                surcharge.OfficeId = chargeBuy.OfficeId_JobRep;
            }
            surcharge.UserCreated = surcharge.UserModified = DocumentConstants.USER_EFMS_SYSTEM;
            surcharge.DatetimeCreated = surcharge.DatetimeModified = DateTime.Now;
            return surcharge;
        }
        public async Task<HandleState> LinkFeeJob(List<OpsTransactionModel> list)
        {
            var result = new HandleState();
            var lstCharge = new List<CsShipmentSurchargeModel>();

            foreach (var ops in list)
            {
                var lstChargeSell = surchargeRepository.Get(x => x.Hblid == ops.Hblid && x.Type == "SELL");
                if (lstChargeSell != null)
                {
                    foreach (var i in lstChargeSell)
                    {
                        if (i.LinkFee == null || i.LinkFee == false)
                        {
                            var propInfo = i.GetType().GetProperties();
                            CsShipmentSurchargeModel chargeModel = new CsShipmentSurchargeModel();
                            foreach (var item in propInfo)
                            {
                                chargeModel.GetType().GetProperty(item.Name).SetValue(chargeModel, item.GetValue(i, null), null);
                            }

                            lstCharge.Add(chargeModel);
                        }
                    }
                }
            }

            if (lstCharge.Count > 0)
                result = csShipmentSurchargeServe.UpdateChargeLinkFee(lstCharge);
            return result;
        }

        private List<sp_GetChargeReplicateToLinkCharge> GetChargesToLinkCharge(Guid userId, string arrJob)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@userId", Value = userId},
                new SqlParameter(){ ParameterName = "@jobRepNo", Value = string.IsNullOrEmpty(arrJob)?null:arrJob},
            };
            var listSurcharges = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetChargeReplicateToLinkCharge>(parameters);
            return listSurcharges;
        }

        /// <summary>
        /// Get Charge AutoRate Replicate
        /// </summary>
        /// <param name="settleNo"></param>
        /// <param name="jobNo"></param>
        /// <returns></returns>
        private List<sp_GetChargeAutoRateReplicate> GetChargeAutoRateReplicate(string settleNo, string jobNo)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@settlementNo", Value = settleNo},
                new SqlParameter(){ ParameterName = "@jobNo", Value = jobNo}
            };
            var listSurcharges = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetChargeAutoRateReplicate>(parameters);
            return listSurcharges;
        }

        public List<ExportOutsourcingRegcognisingModel> GetOutsourcingRegcognising(OpsTransactionCriteria criteria)
        {
            criteria.RangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
            var data = Query(criteria);

            data = data.OrderByDescending(x => x.DatetimeModified);
            List<string> lstJobNo = new List<string>();
            foreach (var item in data)
            {
                if (item.LinkSource == "Replicate")
                {
                    lstJobNo.Add(item.JobNo);
                }
                else if (item.ReplicatedId != null)
                {
                    lstJobNo.Add("R" + item.JobNo);
                }
            }
            var lstJobNoDistinct = lstJobNo.Distinct().ToList();
            string jobNos = "";
            lstJobNoDistinct.ForEach(x =>
            {
                jobNos += x + ";";
            });

            var outRe = GetOutsourcingRegcognising(jobNos);

            ExportOutsourcingRegcognisingModel[] result = new ExportOutsourcingRegcognisingModel[lstJobNoDistinct.Count()];

            for (int i = 0; i < lstJobNoDistinct.Count(); i++)
            {
                var jobrep = outRe.Where(x => x.JobId == lstJobNoDistinct[i] && x.ChargeType == "JobRep").ToList();
                var joborn = outRe.Where(x => x.JobId == lstJobNoDistinct[i].Substring(1) && x.ChargeType == "JobOrn").ToList();
                result[i] = new ExportOutsourcingRegcognisingModel();
                result[i].ReplicateJob = new List<sp_GetOutsourcingRegcognising>();
                result[i].OriginalJob = new List<sp_GetOutsourcingRegcognising>();
                result[i].ReplicateJob.AddRange(jobrep);
                result[i].OriginalJob.AddRange(SortChargeOrn(jobrep, joborn));
            }

            return result.Where(x => x.ReplicateJob.Count() > 0).ToList();
        }

        public async Task<HandleState> SyncGoodInforToReplicateJob(Guid jobId)
        {
            var hs = new HandleState();
            var job = await DataContext.Get(x => x.Id == jobId && x.ReplicatedId != null && x.CurrentStatus != TermData.Canceled).FirstOrDefaultAsync();
            if (job != null)
            {
                var repJob = await DataContext.Get(x => x.Id == job.ReplicatedId && x.CurrentStatus != TermData.Canceled).FirstOrDefaultAsync();
                if (job != null && repJob != null)
                {
                    repJob.SumNetWeight = job.SumNetWeight;
                    repJob.SumPackages = job.SumPackages;
                    repJob.SumCbm = job.SumCbm;
                    repJob.SumContainers = job.SumContainers;
                    repJob.SumGrossWeight = job.SumGrossWeight;
                    repJob.PackageTypeId = job.PackageTypeId;
                    repJob.ContainerDescription = job.ContainerDescription;

                    hs = DataContext.Update(repJob, x => x.Id == repJob.Id, false);
                    if (hs.Success)
                    {
                        var listConOfJob = await csMawbcontainerRepository.GetAsync(x => x.Mblid == job.Id);
                        var listCont = mapper.Map<List<CsMawbcontainerModel>>(listConOfJob);
                        listCont.ForEach(x => x.Id = Guid.Empty);
                        hs = mawbcontainerService.UpdateMasterBill(listCont, job.ReplicatedId ?? Guid.Empty);
                    }
                    hs = DataContext.SubmitChanges();

                    return hs;
                }
            }

            return new HandleState(stringLocalizer[DocumentationLanguageSub.MSG_REPLICATE_NOT_EXISTS].Value);
        }

        private List<sp_GetOutsourcingRegcognising> GetOutsourcingRegcognising(string JobNos)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@JobNos", Value = JobNos }
            };
            var OutRE = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetOutsourcingRegcognising>(parameters);
            return OutRE.ToList();
        }

        private List<sp_GetOutsourcingRegcognising> SortChargeOrn(List<sp_GetOutsourcingRegcognising> chargeReps, List<sp_GetOutsourcingRegcognising> chargeOrns)
        {
            List<sp_GetOutsourcingRegcognising> chargeOrnsChanged = new List<sp_GetOutsourcingRegcognising>();
            for (int i = 0; i < chargeReps.Count(); i++)
            {
                chargeOrnsChanged.Add(chargeOrns.Where(x => x.ChargeId.ToString().ToLower() == chargeReps[i].LinkChargeId).FirstOrDefault());
            }
            return chargeOrnsChanged;
        }

        /// <summary>
        /// Check update job link internal
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState CheckLinkedInteralShipment(OpsTransactionModel model)
        {
            var currentShipment = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
            if (!string.IsNullOrEmpty(currentShipment.ServiceNo) && (currentShipment.ServiceNo != model.ServiceNo || model.ShipmentMode != "Internal"))
            {
                var surchargesOrg = surChargeRepository.Get(x => x.Hblid == currentShipment.Hblid);
                var surchargesLink = surChargeRepository.Get(x => x.JobNo == currentShipment.ServiceNo);
                var linkCharges = csLinkChargeRepository.Get(x => x.LinkChargeType == DocumentConstants.LINK_CHARGE_TYPE_LINK_FEE);
                var hasLinkCharges = from org in surchargesOrg
                                     join linkCharge in linkCharges on org.Id.ToString() equals linkCharge.ChargeOrgId
                                     join link in surchargesLink on linkCharge.ChargeLinkId equals link.Id.ToString()
                                     select new
                                     {
                                         linkCharge,
                                         hblNoLink = link.Hblno
                                     };
                if (hasLinkCharges != null && hasLinkCharges.Any())
                {
                    var item = hasLinkCharges.FirstOrDefault();
                    if (model.ServiceNo != item.linkCharge.JobNoLink || model.ShipmentMode != "Internal")
                    {
                        return new HandleState(false, (object)("Update fail. Shipment has charges link to " + currentShipment.ServiceNo));
                    }
                }
            }
            return new HandleState();
        }
    }
}