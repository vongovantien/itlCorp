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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using eFMS.API.Common.Helpers;
using System.Data.Common;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Connection;
using System.Linq.Expressions;
using eFMS.API.Documentation.DL.Helpers;

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
        private decimal _decimalNumber = Constants.DecimalNumber;
        private decimal _decimalMinNumber = Constants.DecimalMinNumber;

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
            IContextBase<CatCharge> catChargeRepo
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

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    model.JobNo = CreateJobNoOps();
                    OpsTransaction entity = mapper.Map<OpsTransaction>(model);


                    if (model.IsReplicate) // replicate 1 job tương tự.
                    {
                        // Thông tin lv của current user bên office replicate
                        SysSettingFlow settingFlowOffice = settingFlowRepository.Get(x => x.OfficeId == currentUser.OfficeID && x.Flow == "Replicate")?.FirstOrDefault();
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
                            model.OfficeId = dataUserLevel.OfficeId;
                            model.DepartmentId = dataUserLevel.DepartmentId;
                            model.GroupId = dataUserLevel.GroupId;
                            model.CompanyId = dataUserLevel.CompanyId;

                            // mapping saleman
                            var salemanDefault = userRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault();
                            SaleManPermissionModel salemanPermissionInfoReplicate = GetAndUpdateSaleManInfo(salemanDefault.Id);
                            model.SalesGroupId = salemanPermissionInfoReplicate.SalesGroupId;
                            model.SalesDepartmentId = salemanPermissionInfoReplicate.SalesDepartmentId;
                            model.SalesOfficeId = salemanPermissionInfoReplicate.SalesOfficeId;
                            model.SalesCompanyId = salemanPermissionInfoReplicate.SalesCompanyId;

                            model.JobNo = GeneratePreFixReplicate() + entity.JobNo;
                            OpsTransaction entityReplicate = mapper.Map<OpsTransaction>(model);
                            entityReplicate.Id = Guid.NewGuid();
                            entityReplicate.Hblid = Guid.NewGuid();
                            entityReplicate.ServiceNo = entity.JobNo;
                            entityReplicate.ServiceHblId = entity.Hblid;
                            entityReplicate.LinkSource = DocumentConstants.CLEARANCE_FROM_REPLICATE;

                            DataContext.Add(entityReplicate, false);

                            entity.ReplicatedId = entityReplicate.Id;
                        }

                    };

                    DataContext.Add(entity, false);
                    result = DataContext.SubmitChanges();
                    if (result.Success)
                    {
                        trans.Commit();
                    }
                }
                catch (Exception ex)
                {
                    new LogHelper("eFMS_Add_OpsTransaction_Log", ex.ToString());
                    trans.Rollback();
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
            if (model.CsMawbcontainers?.Count > 0 && result.Success)
            {
                var hsContainer = mawbcontainerService.UpdateMasterBill(model.CsMawbcontainers, model.Id);
            }


            return result;
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
                    currentShipment = DataContext.Get(x => x.LinkSource != DocumentConstants.CLEARANCE_FROM_REPLICATE
                                                         && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                         && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                         && x.JobNo.StartsWith("H") && !x.JobNo.StartsWith("HAN-"))
                                                         .OrderByDescending(x => x.JobNo).FirstOrDefault(); //CR: HAN -> H [15202]
                }
                else if (office.Code == "ITLDAD")
                {
                    currentShipment = DataContext.Get(x => x.LinkSource != DocumentConstants.CLEARANCE_FROM_REPLICATE
                                                         && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                         && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                         && x.JobNo.StartsWith("D") && !x.JobNo.StartsWith("DAD-"))
                                                         .OrderByDescending(x => x.JobNo).FirstOrDefault(); //CR: DAD -> D [15202]
                }
                else
                {
                    currentShipment = DataContext.Get(x => x.LinkSource != DocumentConstants.CLEARANCE_FROM_REPLICATE
                                                         && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                         && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                         && !x.JobNo.StartsWith("D") && !x.JobNo.StartsWith("DAD-")
                                                         && !x.JobNo.StartsWith("H") && !x.JobNo.StartsWith("HAN-"))
                                                         .OrderByDescending(x => x.JobNo).FirstOrDefault();
                }
            }
            else
            {
                currentShipment = DataContext.Get(x => x.LinkSource != DocumentConstants.CLEARANCE_FROM_REPLICATE
                                                     && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                     && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                     && !x.JobNo.StartsWith("D") && !x.JobNo.StartsWith("DAD-")
                                                     && !x.JobNo.StartsWith("H") && !x.JobNo.StartsWith("HAN-"))
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
                    prefixCode = "H"; //HAN- >> H
                }
                else if (officeCode == "ITLDAD")
                {
                    prefixCode = "D"; //DAD- >> D
                }
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

                data.ToList().ForEach(x =>
                {
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
            return true;
        }
        public bool CheckAllowDeleteJobUsed(Guid jobId)
        {
            var detail = DataContext.Get(x => x.Id == jobId && x.CurrentStatus != TermData.Canceled)?.FirstOrDefault();
            if(detail.ReplicatedId != null || detail.IsLocked == true)
            {
                return false;
            }
            var query = surchargeRepository.Get(x => x.Hblid == detail.Hblid && (x.CreditNo != null || x.DebitNo != null || x.Soano != null || x.PaymentRefNo != null
                        || !string.IsNullOrEmpty(x.AdvanceNo)
                        || !string.IsNullOrEmpty(x.VoucherId)
                        || !string.IsNullOrEmpty(x.PaySoano)
                        || !string.IsNullOrEmpty(x.SettlementCode)
                        || !string.IsNullOrEmpty(x.SyncedFrom))
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
            Expression<Func<OpsTransaction, bool>> query = q => true;
            List<string> authorizeUserIds = permissionService.GetAuthorizedIds("CL", currentUser);
            switch (range)
            {
                case PermissionRange.All:
                    query = query.And(x => x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null);
                    break;
                case PermissionRange.Owner:
                    query = query.And(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && (x.BillingOpsId == currentUser.UserID || x.SalemanId == currentUser.UserID
                                                 || authorizeUserIds.Contains(x.BillingOpsId) || authorizeUserIds.Contains(x.SalemanId)
                                                 || x.UserCreated == currentUser.UserID));
                    break;
                case PermissionRange.Group:
                    var dataUserLevel = userlevelRepository.Get(x => x.GroupId == currentUser.GroupId).Select(t => t.UserId).ToList();
                    query = query.And(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && ((x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId)
                                                || (dataUserLevel.Contains(x.SalemanId))));
                    break;
                case PermissionRange.Department:
                    var dataUserLevelDepartment = userlevelRepository.Get(x => x.DepartmentId == currentUser.DepartmentId).Select(t => t.UserId).ToList();
                    query = query.And(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && ((x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId)
                                                || dataUserLevelDepartment.Contains(x.SalemanId)));
                    break;
                case PermissionRange.Office:
                    query = query.And(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && ((x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.BillingOpsId)
                                                || authorizeUserIds.Contains(x.SalemanId)));
                    break;
                case PermissionRange.Company:
                    query = query.And(x => (x.CurrentStatus != TermData.Canceled || x.CurrentStatus == null)
                                                && (x.CompanyId == currentUser.CompanyID
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
            return query;
        }

        public IQueryable<OpsTransactionModel> Query(OpsTransactionCriteria criteria)
        {
            if (criteria.RangeSearch == PermissionRange.None) return null;
            //IQueryable<OpsTransaction> data = QueryByPermission(criteria.RangeSearch);

            //Nếu không có điều kiện search thì load 3 tháng kể từ ngày modified mới nhất
            var queryDefault = ExpressionQueryDefault(criteria);
            var data = DataContext.Get(queryDefault);
            var queryPermission = QueryByPermission(criteria.RangeSearch);
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
                                   || (x.CustomerId == criteria.All || string.IsNullOrEmpty(criteria.All))
                                   || (x.FieldOpsId == criteria.All || string.IsNullOrEmpty(criteria.All))
                                   || (x.ShipmentMode == criteria.All || string.IsNullOrEmpty(criteria.All))
                                   || ((x.ServiceDate ?? null) >= (criteria.ServiceDateFrom ?? null) && (x.ServiceDate ?? null) <= (criteria.ServiceDateTo ?? null))
                                   || ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.CreatedDateFrom ?? null) && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.CreatedDateTo ?? null))
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
                if (model.CargoType != null && model.ServiceType == null)
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
                CatPartner customer = new CatPartner();
                CatContract customerContract = new CatContract();

                if (model.AccountNo == null)
                {
                    customer = partnerRepository.Get(x => x.TaxCode == model.PartnerTaxCode)?.FirstOrDefault();
                }
                else
                {
                    customer = partnerRepository.Get(x => x.AccountNo == model.AccountNo)?.FirstOrDefault();
                }
                if (customer == null)
                {
                    var notFoundPartnerTaxCodeMessages = "Customer '" + (model.AccountNo ?? model.PartnerTaxCode) + "' Not found";
                    return new HandleState(notFoundPartnerTaxCodeMessages);
                }

                // Check contract for that customer.
                customerContract = catContractRepository.Get(x => x.PartnerId == customer.ParentId
                && x.SaleService.Contains("CL")
                && x.Active == true
                && x.OfficeId.Contains(currentUser.OfficeID.ToString()))?.FirstOrDefault();
                if (customerContract == null)
                {
                    string officeName = sysOfficeRepo.Get(x => x.Id == currentUser.OfficeID).Select(o => o.ShortName).FirstOrDefault();
                    string errorContract = String.Format("Customer {0} not have any agreements for service in office {1}", customer.ShortName, officeName);
                    return new HandleState(errorContract);
                }

                OpsTransaction opsTransaction = GetNewShipmentToConvert(productService, model, customerContract);
                opsTransaction.JobNo = CreateJobNoOps(); //Generate JobNo [17/12/2020]

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
                    // Check if customer existed
                    var customer = new CatPartner();
                    CatContract customerContract = new CatContract();

                    if (item.AccountNo == null)
                    {
                        customer = partnerRepository.Get(x => x.TaxCode == item.PartnerTaxCode)?.FirstOrDefault();
                    }
                    else
                    {
                        customer = partnerRepository.Get(x => x.AccountNo == item.AccountNo)?.FirstOrDefault();
                    }
                    if (customer == null)
                    {
                        var notFoundPartnerTaxCodeMessages = "Customer '" + (item.AccountNo ?? item.PartnerTaxCode) + "' Not found";
                        return new HandleState(notFoundPartnerTaxCodeMessages);
                    }

                    // Check contract for that customer. TODO: TÁCH FUNCTION
                    customerContract = catContractRepository.Get(x => x.PartnerId == customer.ParentId
                    && x.SaleService.Contains("CL")
                    && x.Active == true
                    && x.OfficeId.Contains(currentUser.OfficeID.ToString()))?.FirstOrDefault();
                    if (customerContract == null)
                    {
                        string officeName = sysOfficeRepo.Get(x => x.Id == currentUser.OfficeID).Select(o => o.ShortName).FirstOrDefault();
                        string errorContract = String.Format("Customer {0} not have any agreements for service in office {1}", customer.ShortName, officeName);
                        return new HandleState(errorContract);
                    }

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
                                opsTransaction.JobNo = CreateJobNoOps(); //Generate JobNo [17/12/2020]

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
                opsTransactionReplicate.ServiceNo = opsTransaction.JobNo;
                opsTransactionReplicate.ServiceHblId = opsTransaction.Hblid;
                opsTransactionReplicate.LinkSource = DocumentConstants.CLEARANCE_FROM_REPLICATE;
                opsTransactionReplicate.OfficeId = settingFlowOffice.ReplicateOfficeId; // office của setting replicate

                // mapping saleman
                var salemanDefault = userRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault();
                SaleManPermissionModel salemanPermissionInfoReplicate = GetAndUpdateSaleManInfo(salemanDefault.Id);
                opsTransactionReplicate.SalesGroupId = salemanPermissionInfoReplicate.SalesGroupId;
                opsTransactionReplicate.SalesDepartmentId = salemanPermissionInfoReplicate.SalesDepartmentId;
                opsTransactionReplicate.SalesOfficeId = salemanPermissionInfoReplicate.SalesOfficeId;
                opsTransactionReplicate.SalesCompanyId = salemanPermissionInfoReplicate.SalesCompanyId;

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
                result = DataContext.Update(job, x => x.Id == id, false);
                if (result.Success)
                {
                    //Xóa Job OPS xóa luôn surcharge [Andy - 05/02/2021]
                    var charges = surchargeRepository.Get(x => x.Hblid == job.Hblid);
                    foreach (var item in charges)
                    {
                        surchargeRepository.Delete(x => x.Id == item.Id, false);
                    }
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
                // Xóa job rep thì xóa liên kết với job origin.
                if(job.LinkSource == DocumentConstants.CLEARANCE_FROM_REPLICATE)
                {
                    var jobOrigin = DataContext.First(x => x.ReplicatedId == job.Id);
                    if(jobOrigin != null)
                    {
                        jobOrigin.ReplicatedId = null;
                        DataContext.Update(jobOrigin, x => x.Id == jobOrigin.Id, false);
                    }
                }
            }
            result = DataContext.SubmitChanges();
            if(result.Success)
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
                );
            }
            else
            {
                var duplicateHBLMBL = DataContext.Get(x => x.Id != model.Id
                && x.CurrentStatus != TermData.Canceled
                && x.Hwbno == model.Hwbno
                && x.Mblno == model.Mblno
                ).ToList();
                if (duplicateHBLMBL.Count > 0)
                {
                    if (model.ReplicatedId == null || model.ReplicatedId == Guid.Empty)
                    {
                        if (!string.IsNullOrEmpty(model.ServiceNo))
                        {
                            existedMblHbl = duplicateHBLMBL.Any(x => x.ReplicatedId != model.Id);
                        }
                        else
                        {
                            existedMblHbl = true;
                        }
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
                CompanyName = DocumentConstants.COMPANY_NAME,
                CompanyDescription = string.Empty,
                CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1,
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
            if (code == 403) return new ResultHandle { Status = false, Message = "You can't duplicate this job." };

            List<CsMawbcontainer> newContainers = new List<CsMawbcontainer>();
            List<CsShipmentSurcharge> newSurcharges = new List<CsShipmentSurcharge>();


            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    // Create model import
                    Guid _hblId = model.Hblid;
                    model.Hblid = Guid.NewGuid();
                    model.JobNo = CreateJobNoOps();
                    model.UserModified = currentUser.UserID;
                    model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                    model.UserCreated = currentUser.UserID;
                    model.GroupId = currentUser.GroupId;
                    model.DepartmentId = currentUser.DepartmentId;
                    model.OfficeId = currentUser.OfficeID;
                    model.CompanyId = currentUser.CompanyID;

                    model.IsLocked = false; // Luôn luôn mở job khi duplicate.

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
                    // Update list SurCharge
                    List<CsShipmentSurcharge> listSurCharge = CopySurChargeToNewJob(_hblId, model);
                    if (listSurCharge?.Count() > 0)
                    {
                        newSurcharges.AddRange(listSurCharge);
                    }

                    OpsTransaction entity = mapper.Map<OpsTransaction>(model);
                    HandleState hs = DataContext.Add(entity);

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
        private List<CsShipmentSurcharge> CopySurChargeToNewJob(Guid _oldHblId, OpsTransactionModel shipment)
        {
            List<CsShipmentSurcharge> surCharges = null;
            var charges = surchargeRepository.Get(x => x.Hblid == _oldHblId && x.IsFromShipment == true);
            decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;
            if (charges.Select(x => x.Id).Count() != 0)
            {
                surCharges = new List<CsShipmentSurcharge>();
                foreach (var item in charges)
                {
                    item.Id = Guid.NewGuid();
                    item.UserCreated = currentUser.UserID;

                    item.DatetimeCreated = DateTime.Now;
                    item.Hblid = shipment.Hblid;

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

                    item.JobNo = shipment.JobNo;
                    item.Hblno = shipment.Hwbno;
                    item.Mblno = shipment.Mblno;


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

        public ResultHandle ChargeFromReplicate()
        {
            ResultHandle hs = new ResultHandle();
            List<CsShipmentSurcharge> surchargeAdds = new List<CsShipmentSurcharge>();
            CatPartner partnerInternal = new CatPartner();
            try
            {
                var lstJobRep = DataContext.Get(x => x.LinkSource == DocumentConstants.CLEARANCE_FROM_REPLICATE && x.UserCreated == currentUser.UserID);
                if (lstJobRep != null)
                {
                    foreach (var jobRep in lstJobRep)
                    {
                        var job = DataContext.Get(x => x.ReplicatedId == jobRep.Id).FirstOrDefault();
                        if (job == null)
                            continue;

                        if (job.OfficeId != null)
                        {
                            var offi = GetInfoOfficeOfUser(jobRep.OfficeId);
                            if (offi != null && string.IsNullOrEmpty(offi.InternalCode))
                                continue;
                            var part = partnerRepository.Get(x => x.InternalCode == offi.InternalCode);
                            if (part == null)
                                continue;
                            if (part.Count() > 1)
                                continue;

                            if (part.FirstOrDefault() == null)
                                continue;

                            partnerInternal = part.FirstOrDefault();
                        }

                        var charges = surchargeRepository.Get(x => x.JobNo == jobRep.JobNo && x.LinkChargeId == null);
                        if (charges != null)
                        {
                            foreach (var charge in charges)
                            {
                                if (surchargeRepository.Get(x => x.LinkChargeId == charge.Id.ToString()).FirstOrDefault() != null)
                                    continue;

                                CsShipmentSurcharge surcharge = mapper.Map<CsShipmentSurcharge>(charge);

                                if (charge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                                {
                                    surcharge.Type = DocumentConstants.CHARGE_BUY_TYPE;
                                    var catCharge = catChargeRepository.Get(x => x.DebitCharge == charge.ChargeId && x.DebitCharge != null).FirstOrDefault();
                                    if (catCharge != null) { surcharge.ChargeId = catCharge.Id; };
                                    if (!string.IsNullOrEmpty(partnerInternal.Id))
                                        surcharge.PaymentObjectId = partnerInternal.Id;
                                }
                                else if (charge.Type == DocumentConstants.CHARGE_OBH_TYPE)
                                {
                                    surcharge.Type = DocumentConstants.CHARGE_OBH_TYPE;
                                    if (!string.IsNullOrEmpty(partnerInternal.Id))
                                        surcharge.PayerId = partnerInternal.Id;
                                }
                                else { continue; }

                                surcharge.LinkChargeId = charge.Id.ToString();
                                surcharge.Id = Guid.NewGuid();
                                surcharge.JobNo = job.JobNo ?? "";
                                surcharge.Hblid = job.Hblid;
                                surcharge.Hblno = job.Hwbno;
                                surcharge.Mblno = job.Mblno;

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

                                surcharge.UserCreated = currentUser.UserID;
                                surcharge.DatetimeCreated = DateTime.Now;

                                surchargeAdds.Add(surcharge);
                            }
                        }

                        if (surchargeAdds.Count > 0)
                        {
                            foreach (var item in surchargeAdds)
                                surchargeRepository.Add(item);
                        }
                    }
                }
                return new ResultHandle { Status = true, Message = "ChargeFromReplicate sccuess", Data = null };
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_CHARGEFROMREPLICATE", ex.ToString());
                return new ResultHandle { Status = false, Message = "Job can't be charge from replicate !" };
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
                    entityReplicate.DatetimeCreated = job.DatetimeCreated;
                    entityReplicate.UserCreated = currentUser.UserID;
                    entityReplicate.UserModified = currentUser.UserID;
                    entityReplicate.DatetimeModified = DateTime.Now;
                    entityReplicate.JobNo = GeneratePreFixReplicate() + job.JobNo;
                    entityReplicate.Id = Guid.NewGuid();
                    entityReplicate.Hblid = Guid.NewGuid();
                    entityReplicate.ServiceNo = job.JobNo;
                    entityReplicate.ServiceHblId = job.Hblid;
                    entityReplicate.OfficeId = dataUserLevel.OfficeId;
                    entityReplicate.DepartmentId = dataUserLevel.DepartmentId;
                    entityReplicate.GroupId = dataUserLevel.GroupId;
                    entityReplicate.CompanyId = dataUserLevel.CompanyId;
                    entityReplicate.SalesGroupId = salemanPermissionInfoReplicate.SalesGroupId;
                    entityReplicate.SalesDepartmentId = salemanPermissionInfoReplicate.SalesDepartmentId;
                    entityReplicate.SalesOfficeId = salemanPermissionInfoReplicate.SalesOfficeId;
                    entityReplicate.SalesCompanyId = salemanPermissionInfoReplicate.SalesCompanyId;
                    entityReplicate.IsLocked = false;
                    entityReplicate.LinkSource = DocumentConstants.CLEARANCE_FROM_REPLICATE;

                    hs = DataContext.Add(entityReplicate);
                    if (hs.Success)
                    {
                        job.ReplicatedId = entityReplicate.Id;
                        hs = DataContext.Update(job, x => x.Id == Id);

                        var customD = customDeclarationRepository.First(x => x.JobNo == job.JobNo);
                        // Add tk ảo.
                        if(customD != null)
                        {
                            var cdInfo = customD.GetType().GetProperties();
                            CustomsDeclaration cdReplicate = new CustomsDeclaration();

                            foreach (var item in cdInfo)
                            {
                                if(item.Name != "Id")
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
                    }
                };
            }
            return hs;
        }
    }
}
