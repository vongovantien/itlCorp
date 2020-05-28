using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsTransactionService : RepositoryBase<CsTransaction, CsTransactionModel>, ICsTransactionService
    {
        private readonly ICurrentUser currentUser;
        readonly IContextBase<CsTransactionDetail> csTransactionDetailRepo;
        readonly IContextBase<CsMawbcontainer> csMawbcontainerRepo;
        readonly IContextBase<CsShipmentSurcharge> csShipmentSurchargeRepo;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<CsTransaction> transactionRepository;
        readonly IContextBase<CsArrivalFrieghtCharge> freighchargesRepository;
        readonly IContextBase<CatCurrencyExchange> currencyExchangeRepository;
        readonly IContextBase<CatUnit> catUnitRepo;
        readonly IContextBase<CatCountry> catCountryRepo;
        readonly ICsMawbcontainerService containerService;
        readonly ICsShipmentSurchargeService surchargeService;
        readonly ICsTransactionDetailService transactionDetailService;
        readonly ICsArrivalFrieghtChargeService csArrivalFrieghtChargeService;
        readonly ICsDimensionDetailService dimensionDetailService;
        readonly IContextBase<CsDimensionDetail> dimensionDetailRepository;
        readonly IStringLocalizer stringLocalizer;
        readonly IUserPermissionService permissionService;
        private readonly ICurrencyExchangeService currencyExchangeService;
        readonly IContextBase<SysOffice> sysOfficeRepository;
        readonly IContextBase<CsAirWayBill> airwaybillRepository;
        readonly IContextBase<SysGroup> groupRepository;
        readonly IContextBase<CatCommodity> commodityRepository;
        public CsTransactionService(IContextBase<CsTransaction> repository,
            IMapper mapper,
            ICurrentUser user,
            IStringLocalizer<LanguageSub> localizer,
            IContextBase<CsTransactionDetail> csTransactionDetail,
            IContextBase<CsMawbcontainer> csMawbcontainer,
            IContextBase<CsShipmentSurcharge> csShipmentSurcharge,
            IContextBase<CatPartner> catPartner,
            IContextBase<CatPlace> catPlace,
            IContextBase<SysUser> sysUser,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<CsTransaction> transactionRepo,
            IContextBase<CatCurrencyExchange> currencyExchangeRepo,
            IContextBase<CatUnit> catUnit,
            IContextBase<CatCountry> catCountry,
            ICsMawbcontainerService contService,
            ICsShipmentSurchargeService surService,
            ICsTransactionDetailService tranDetailService,
            ICsArrivalFrieghtChargeService arrivalFrieghtChargeService,
            ICsDimensionDetailService dimensionService,
            IContextBase<CsDimensionDetail> dimensionDetailRepo,
            IUserPermissionService perService,
            IContextBase<CsArrivalFrieghtCharge> freighchargesRepo,
            IContextBase<SysOffice> sysOfficeRepo,
            ICurrencyExchangeService currencyExchange,
            IContextBase<CsAirWayBill> airwaybillRepo,
            IContextBase<SysGroup> groupRepo,
            IContextBase<CatCommodity> commodityRepo) : base(repository, mapper)
        {
            currentUser = user;
            stringLocalizer = localizer;
            csTransactionDetailRepo = csTransactionDetail;
            csMawbcontainerRepo = csMawbcontainer;
            csShipmentSurchargeRepo = csShipmentSurcharge;
            catPartnerRepo = catPartner;
            catPlaceRepo = catPlace;
            sysUserRepo = sysUser;
            sysEmployeeRepo = sysEmployee;
            transactionRepository = transactionRepo;
            currencyExchangeRepository = currencyExchangeRepo;
            containerService = contService;
            catUnitRepo = catUnit;
            surchargeService = surService;
            catCountryRepo = catCountry;
            transactionDetailService = tranDetailService;
            csArrivalFrieghtChargeService = arrivalFrieghtChargeService;
            dimensionDetailService = dimensionService;
            dimensionDetailRepository = dimensionDetailRepo;
            permissionService = perService;
            freighchargesRepository = freighchargesRepo;
            currencyExchangeService = currencyExchange;
            sysOfficeRepository = sysOfficeRepo;
            airwaybillRepository = airwaybillRepo;
            groupRepository = groupRepo;
            commodityRepository = commodityRepo;
        }

        #region -- INSERT & UPDATE --

        /// <summary>
        /// Create JobNo by Transaction Type
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="transactionType"></param>
        /// <returns></returns>
        private string CreateJobNoByTransactionType(TransactionTypeEnum typeEnum, string transactionType)
        {
            var shipment = string.Empty;
            int countNumberJob = 0;
            switch (typeEnum)
            {
                case TransactionTypeEnum.InlandTrucking:
                    shipment = DocumentConstants.IT_SHIPMENT;
                    break;
                case TransactionTypeEnum.AirExport:
                    shipment = DocumentConstants.AE_SHIPMENT;
                    break;
                case TransactionTypeEnum.AirImport:
                    shipment = DocumentConstants.AI_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaConsolExport:
                    shipment = DocumentConstants.SEC_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaConsolImport:
                    shipment = DocumentConstants.SIC_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    shipment = DocumentConstants.SEF_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    shipment = DocumentConstants.SIF_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    shipment = DocumentConstants.SEL_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    shipment = DocumentConstants.SIL_SHIPMENT;
                    break;
                default:
                    break;
            }
            var currentShipment = DataContext.Get(x => x.TransactionType == transactionType
                                                    && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                    && x.DatetimeCreated.Value.Year == DateTime.Now.Year)
                                                    .OrderByDescending(x => x.JobNo)
                                                    .FirstOrDefault();
            if (currentShipment != null)
            {
                countNumberJob = Convert.ToInt32(currentShipment.JobNo.Substring(shipment.Length + 5, 5));
            }
            return GenerateID.GenerateJobID(shipment, countNumberJob);
        }

        public object AddCSTransaction(CsTransactionEditModel model)
        {
            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(model.TransactionType, currentUser);

            var transaction = mapper.Map<CsTransaction>(model);
            transaction.Id = Guid.NewGuid();
            if (model.CsMawbcontainers != null)
            {
                var checkDuplicateCont = containerService.ValidateContainerList(model.CsMawbcontainers, transaction.Id, null);
                if (checkDuplicateCont.Success == false)
                {
                    return checkDuplicateCont;
                }
            }
            transaction.JobNo = CreateJobNoByTransactionType(model.TransactionTypeEnum, model.TransactionType);
            transaction.DatetimeCreated = transaction.DatetimeModified = DateTime.Now;
            transaction.Active = true;
            transaction.UserModified = transaction.UserCreated;
            transaction.IsLocked = false;
            transaction.LockedDate = null;
            transaction.CurrentStatus = TermData.Processing; //Mặc định gán CurrentStatus = Processing
            transaction.GroupId = _currentUser.GroupId;
            transaction.DepartmentId = _currentUser.DepartmentId;
            transaction.OfficeId = _currentUser.OfficeID;
            transaction.CompanyId = _currentUser.CompanyID;
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hsTrans = DataContext.Add(transaction);
                    if (hsTrans.Success)
                    {
                        if (model.CsMawbcontainers != null)
                        {
                            model.CsMawbcontainers.ForEach(x =>
                            {
                                x.Id = Guid.NewGuid();
                                x.Mblid = transaction.Id;
                                x.UserModified = transaction.UserCreated;
                                x.DatetimeModified = DateTime.Now;
                            });
                            var t = containerService.Add(model.CsMawbcontainers);
                        }
                        if (model.DimensionDetails != null)
                        {
                            model.DimensionDetails.ForEach(x =>
                            {
                                x.Id = Guid.NewGuid();
                                x.Mblid = transaction.Id;
                            });

                            var d = dimensionDetailService.Add(model.DimensionDetails);
                        }
                    }
                    var result = hsTrans;
                    trans.Commit();
                    return new { model = transaction, result };
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    var result = new HandleState(ex.Message);
                    return new { model = new object { }, result };
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public HandleState UpdateCSTransaction(CsTransactionEditModel model)
        {
            var job = DataContext.First(x => x.Id == model.Id && x.CurrentStatus != TermData.Canceled);
            if (job == null)
            {
                return new HandleState("Shipment not found !");
            }

            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(job.TransactionType, currentUser);
            var permissionRange = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Write);
            int code = GetPermissionToUpdate(new ModelUpdate { PersonInCharge = job.PersonIncharge, UserCreated = job.UserCreated, CompanyId = job.CompanyId, OfficeId = job.OfficeId, DepartmentId = job.DepartmentId, GroupId = job.GroupId }, permissionRange, job.TransactionType);
            if (code == 403) return new HandleState(403, "");
            var transaction = mapper.Map<CsTransaction>(model);
            transaction.UserModified = currentUser.UserID;
            transaction.DatetimeModified = DateTime.Now;
            transaction.Active = true;
            transaction.CurrentStatus = job.CurrentStatus;
            transaction.GroupId = job.GroupId;
            transaction.DepartmentId = job.DepartmentId;
            transaction.OfficeId = job.OfficeId;
            transaction.CompanyId = job.CompanyId;
            transaction.DatetimeCreated = job.DatetimeCreated;
            transaction.UserCreated = job.UserCreated;

            if (transaction.IsLocked.HasValue)
            {
                if (transaction.IsLocked == true)
                {
                    transaction.LockedDate = DateTime.Now;
                }
            }
            var airwaybill = airwaybillRepository.Get(x => x.JobId == model.Id)?.FirstOrDefault();
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hsTrans = DataContext.Update(transaction, x => x.Id == transaction.Id);
                    if (hsTrans.Success)
                    {
                        if(airwaybill != null)
                        {
                            airwaybill.IssuedBy = model.IssuedBy;
                            airwaybill.DatetimeModified = DateTime.Now;
                            airwaybill.UserModified = currentUser.UserID;
                            var hsAirwayBill = airwaybillRepository.Update(airwaybill, x => x.Id == airwaybill.Id);
                        }
                        if (model.CsMawbcontainers != null)
                        {
                            var hscontainers = containerService.UpdateMasterBill(model.CsMawbcontainers, transaction.Id);
                        }
                        else
                        {
                            var hsContainerDetele = csMawbcontainerRepo.Delete(x => x.Mblid == model.Id);
                        }
                        if (model.DimensionDetails != null)
                        {
                            var hsdimensions = dimensionDetailService.UpdateMasterBill(model.DimensionDetails, transaction.Id);
                        }
                        else
                        {
                            var hsContainerDetele = dimensionDetailService.Delete(x => x.Mblid == model.Id);
                        }
                    }
                    trans.Commit();
                    return hsTrans;
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

        #endregion -- INSERT & UPDATE --

        #region -- DELETE --
        public bool CheckAllowDelete(Guid jobId)
        {
            var query = (from detail in csTransactionDetailRepo.Get()
                         where detail.JobId == jobId
                         join surcharge in csShipmentSurchargeRepo.Get() on detail.Id equals surcharge.Hblid
                         where !string.IsNullOrEmpty(surcharge.CreditNo)
                            || !string.IsNullOrEmpty(surcharge.DebitNo)
                            || !string.IsNullOrEmpty(surcharge.Soano)
                            || !string.IsNullOrEmpty(surcharge.PaySoano)
                            || !string.IsNullOrEmpty(surcharge.SettlementCode)
                         select detail);
            if (query.Any())
            {
                return false;
            }
            return true;
        }

        public HandleState DeleteCSTransaction(Guid jobId)
        {
            var containers = csMawbcontainerRepo.Get(x => x.Mblid == jobId);
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    if (containers.Count() > 0)
                    {
                        var hsMasterCont = csMawbcontainerRepo.Delete(x => containers.Any(s => s.Id == x.Id));
                    }
                    var details = csTransactionDetailRepo.Get(x => x.JobId == jobId);
                    if (details.Count() > 0)
                    {
                        foreach (var item in details)
                        {
                            var houseContainers = csMawbcontainerRepo.Get(x => x.Hblid == item.Id);
                            if (houseContainers.Count() > 0)
                            {
                                var hsHouseCont = csMawbcontainerRepo.Delete(x => houseContainers.Any(s => s.Id == x.Id));
                            }
                            var surcharges = csShipmentSurchargeRepo.Get(x => x.Hblid == item.Id);
                            if (surcharges.Count() > 0)
                            {
                                var hsSurcharge = csShipmentSurchargeRepo.Delete(x => surcharges.Any(s => s.Id == x.Id));
                            }
                        }
                        var hsHouseBill = csTransactionDetailRepo.Delete(x => details.Any(s => s.Id == x.Id));
                    }
                    var hs = DataContext.Delete(x => x.Id == jobId);
                    if (hs.Success)
                    {
                        trans.Commit();
                    }
                    else
                    {
                        trans.Rollback();
                    }
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

        public HandleState SoftDeleteJob(Guid jobId)
        {
            //Xóa mềm hiện tại chỉ cập nhật CurrentStatus = Canceled cho Shipment Documment.
            var hs = new HandleState();
            try
            {
                var job = DataContext.First(x => x.Id == jobId && x.CurrentStatus != TermData.Canceled);
                ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(job.TransactionType, currentUser);
                var permissionRange = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Delete);
                int code = GetPermissionToDelete(new ModelUpdate { PersonInCharge = job.PersonIncharge, UserCreated = job.UserCreated, CompanyId = job.CompanyId, OfficeId = job.OfficeId, DepartmentId = job.DepartmentId, GroupId = job.GroupId }, permissionRange);
                if (code == 403) return new HandleState(403, "");

                if (job == null)
                {
                    hs = new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);
                }
                else
                {
                    job.CurrentStatus = TermData.Canceled;
                    job.DatetimeModified = DateTime.Now;
                    job.UserModified = currentUser.UserID;
                    hs = DataContext.Update(job, x => x.Id == jobId);
                }
            }
            catch (Exception ex)
            {
                hs = new HandleState(ex.Message);
            }
            return hs;
        }

        #endregion -- DELETE --

        #region -- DETAILS --
        private CsTransactionModel GetById(Guid id)
        {
            CsTransaction data = DataContext.Get(x => x.Id == id && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED).FirstOrDefault();
            if (data == null) return null;
            else
            {
                CsTransactionModel result = mapper.Map<CsTransactionModel>(data);
                if (result.ColoaderId != null)
                {
                    CatPartner coloaderPartner = catPartnerRepo.Where(x => x.Id == result.ColoaderId)?.FirstOrDefault();
                    result.SupplierName = coloaderPartner.PartnerNameEn;
                    result.ColoaderCode = coloaderPartner.CoLoaderCode;
                    result.RoundUpMethod = coloaderPartner.RoundUpMethod;
                    result.ApplyDim = coloaderPartner.ApplyDim;
                }
                if (result.AgentId != null)
                {
                    CatPartner agent = catPartnerRepo.Get().FirstOrDefault(x => x.Id == result.AgentId);
                    result.AgentName = agent.PartnerNameEn;
                    result.AgentData = GetAgent(agent);
                }

                if (result.Pod != null)
                {
                    CatPlace portIndexPod = catPlaceRepo.Get(x => x.Id == result.Pod)?.FirstOrDefault();
                    result.PODName = portIndexPod.NameEn;
                    result.PODCode = portIndexPod.Code;

                    if (portIndexPod.WarehouseId != null)
                    {
                        CatPlace warehouse = catPlaceRepo.Get(x => x.Id == portIndexPod.WarehouseId)?.FirstOrDefault();
                        result.WarehousePOD = new WarehouseData {
                            NameEn = warehouse.NameEn,
                            NameVn = warehouse.NameEn,
                            NameAbbr = warehouse.DisplayName,
                        };
                    }
                }

                if (result.Pol != null)
                {
                    CatPlace portIndexPol = catPlaceRepo.Get(x => x.Id == result.Pol)?.FirstOrDefault();
                    result.POLCode = portIndexPol.Code;
                    result.POLName = portIndexPol.NameEn;

                    if(portIndexPol.CountryId != null)
                    {
                        CatCountry country = catCountryRepo.Get(c => c.Id == portIndexPol.CountryId)?.FirstOrDefault();

                        result.POLCountryCode = country.Code;
                        result.POLCountryNameEn = country.NameEn;
                        result.POLCountryNameVn = country.NameVn;
                    }

                    if (portIndexPol.WarehouseId != null)
                    {
                        CatPlace warehouse = catPlaceRepo.Get(x => x.Id == portIndexPol.WarehouseId)?.FirstOrDefault();
                        result.WarehousePOL = new WarehouseData
                        {
                            NameEn = warehouse.NameEn,
                            NameVn = warehouse.NameEn,
                            NameAbbr = warehouse.DisplayName,
                        };
                    }
                }

                if (result.DeliveryPlace != null) result.PlaceDeliveryName = catPlaceRepo.Get(x => x.Id == result.DeliveryPlace)?.FirstOrDefault().NameEn;
                result.UserNameCreated = sysUserRepo.Get(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                result.UserNameModified = sysUserRepo.Get(x => x.Id == result.UserModified).FirstOrDefault()?.Username;

                if(result.OfficeId != null)
                {
                    result.CreatorOffice = GetOfficeOfCreator(result.OfficeId);
                }
                if(result.GroupId != null)
                {
                    var group = groupRepository.Get(x => x.Id == result.GroupId).FirstOrDefault();
                    result.GroupEmail = group != null ? group.Email : string.Empty;
                }

                return result;
            }
        }

        private AgentData GetAgent(CatPartner agent)
        {
            var agentData = new AgentData
            {
                NameEn = agent.PartnerNameEn,
                NameVn = agent.PartnerNameVn,
                Fax = agent.Fax,
                Tel = agent.Tel,
                Address = agent.AddressEn
            };
            return agentData;
        }

        private OfficeData GetOfficeOfCreator(Guid? officeId)
        {
            SysOffice office = sysOfficeRepository.Get(x => x.Id == officeId)?.FirstOrDefault();
            var creatorOffice = new OfficeData
            {
                NameEn = office.BranchNameEn,
                NameVn = office.BranchNameVn,
                Location = office.Location,
                AddressEn = office.AddressEn,
                Tel = office.Tel,
                Fax = office.Fax
            };
            return creatorOffice;
        }

        public int CheckDetailPermission(Guid id)
        {
            var detail = GetById(id);
            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(detail.TransactionType, currentUser);
            var permissionRange = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Detail);
            int code = GetPermissionToUpdate(new ModelUpdate { PersonInCharge = detail.PersonIncharge, UserCreated = detail.UserCreated, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId }, permissionRange, detail.TransactionType);
            return code;
        }

        public int CheckDeletePermission(Guid id)
        {
            var detail = GetById(id);
            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(detail.TransactionType, currentUser);
            var permissionRange = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Delete);
            int code = GetPermissionToDelete(new ModelUpdate { PersonInCharge = detail.PersonIncharge, UserCreated = detail.UserCreated, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId }, permissionRange);
            return code;
        }

        private int GetPermissionToUpdate(ModelUpdate model, PermissionRange permissionRange, string transactionType)
        {
            int code = 0;
            if (permissionRange == PermissionRange.None)
            {
                code = 403;
                return code;
            }

            List<string> authorizeUserIds = permissionService.GetAuthorizedIds(transactionType, currentUser);
            code = PermissionEx.GetPermissionToUpdateShipmentDocumentation(model, permissionRange, currentUser, authorizeUserIds);
            return code;
        }

        private int GetPermissionToDelete(ModelUpdate model, PermissionRange permissionRange)
        {
            int code = PermissionEx.GetPermissionToDeleteShipmentDocumentation(model, permissionRange, currentUser);
            return code;
        }

        public CsTransactionModel GetDetails(Guid id)
        {
            CsTransactionModel detail = GetById(id);
            if (detail == null) return null;
            List<string> authorizeUserIds = permissionService.GetAuthorizedIds(detail.TransactionType, currentUser);
            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(detail.TransactionType, currentUser);

            var permissionRangeWrite = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Write);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Delete);
            detail.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionDetail(permissionRangeWrite, authorizeUserIds, detail),
                AllowDelete = GetPermissionDetail(permissionRangeDelete, authorizeUserIds, detail)
            };
            var specialActions = _currentUser.UserMenuPermission.SpecialActions;
            detail.Permission = PermissionEx.GetSpecialActions(detail.Permission, specialActions);
            return detail;
        }

        private bool GetPermissionDetail(PermissionRange permissionRangeWrite, List<string> authorizeUserIds, CsTransactionModel detail)
        {
            bool result = false;
            switch (permissionRangeWrite)
            {
                case PermissionRange.All:
                    result = true;
                    break;
                case PermissionRange.Owner:
                    if (detail.PersonIncharge == currentUser.UserID || detail.UserCreated == currentUser.UserID || authorizeUserIds.Contains(detail.PersonIncharge))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Group:
                    if ((detail.GroupId == currentUser.GroupId && detail.GroupId != null)
                        || authorizeUserIds.Contains(detail.PersonIncharge))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Department:
                    if ((detail.DepartmentId == currentUser.DepartmentId && detail.DepartmentId != null) || authorizeUserIds.Contains(detail.PersonIncharge))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Office:
                    if ((detail.OfficeId == currentUser.OfficeID && detail.OfficeId != null) || authorizeUserIds.Contains(detail.PersonIncharge))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case PermissionRange.Company:
                    if (detail.CompanyId == currentUser.CompanyID || authorizeUserIds.Contains(detail.PersonIncharge))
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
        #endregion -- DETAILS --

        #region -- LIST & PAGING --       

        private IQueryable<CsTransactionModel> GetTransaction(string transactionType)
        {
            ICurrentUser _user = PermissionEx.GetUserMenuPermissionTransaction(transactionType, currentUser);

            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);

            var masterBills = DataContext.Get(x => x.TransactionType == transactionType && x.CurrentStatus != TermData.Canceled);
            if (masterBills == null) return null;
            List<string> authorizeUserIds = permissionService.GetAuthorizedIds(transactionType, currentUser);
            switch (rangeSearch)
            {
                case PermissionRange.None:
                    masterBills = null;
                    break;
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    masterBills = masterBills.Where(x => x.PersonIncharge == currentUser.UserID
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    masterBills = masterBills.Where(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Department:
                    masterBills = masterBills.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Office:
                    masterBills = masterBills.Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Company:
                    masterBills = masterBills.Where(x => x.CompanyId == currentUser.CompanyID
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID);
                    break;
            }
            if (masterBills == null)
                return null;

            var coloaders = catPartnerRepo.Get(x => x.PartnerGroup.Contains("CARRIER"));
            var agents = catPartnerRepo.Get(x => x.PartnerGroup.Contains("AGENT"));
            var pols = catPlaceRepo.Get(x => x.PlaceTypeId == "Port");
            var pods = catPlaceRepo.Get(x => x.PlaceTypeId == "Port");
            var creators = sysUserRepo.Get();
            IQueryable<CsTransactionModel> query = null;

            query = from masterBill in masterBills
                    join coloader in coloaders on masterBill.ColoaderId equals coloader.Id into coloader2
                    from coloader in coloader2.DefaultIfEmpty()
                    join agent in agents on masterBill.AgentId equals agent.Id into agent2
                    from agent in agent2.DefaultIfEmpty()
                    join pod in pods on masterBill.Pod equals pod.Id into pod2
                    from pod in pod2.DefaultIfEmpty()
                    join pol in pols on masterBill.Pol equals pol.Id into pol2
                    from pol in pol2.DefaultIfEmpty()
                    join creator in creators on masterBill.UserCreated equals creator.Id into creator2
                    from creator in creator2.DefaultIfEmpty()
                    select new CsTransactionModel
                    {
                        Id = masterBill.Id,
                        JobNo = masterBill.JobNo,
                        Mawb = masterBill.Mawb,
                        Etd = masterBill.Etd,
                        Eta = masterBill.Eta,
                        ServiceDate = masterBill.ServiceDate,
                        ColoaderId = masterBill.ColoaderId,
                        AgentId = masterBill.AgentId,
                        Pol = masterBill.Pol,
                        Pod = masterBill.Pod,
                        PackageContainer = masterBill.PackageContainer,
                        NetWeight = masterBill.NetWeight,
                        GrossWeight = masterBill.GrossWeight,
                        ChargeWeight = masterBill.ChargeWeight,
                        Cbm = masterBill.Cbm,
                        TransactionType = masterBill.TransactionType,
                        UserCreated = masterBill.UserCreated,
                        IsLocked = masterBill.IsLocked,
                        DatetimeCreated = masterBill.DatetimeCreated,
                        UserModified = masterBill.UserModified,
                        DatetimeModified = masterBill.DatetimeModified,
                        SupplierName = coloader.ShortName,
                        AgentName = agent.ShortName,
                        PODName = pod.NameEn,
                        POLName = pol.NameEn,
                        CreatorName = creator.Username,
                        PackageQty = masterBill.PackageQty
                    };

            return query;
        }

        public List<CsTransactionModel> Paging(CsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            var results = new List<CsTransactionModel>();
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0;
                return results;
            }
            var tempList = list;
            rowsCount = tempList.Select(s => s.Id).Count();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                tempList = tempList.Skip((page - 1) * size).Take(size);
                results = tempList.ToList();
                results.ForEach(fe =>
                {
                    fe.SumCont = csMawbcontainerRepo.Get(x => x.Mblid == fe.Id).Sum(s => s.Quantity);
                    fe.SumPackage = csMawbcontainerRepo.Get(x => x.Mblid == fe.Id).Sum(s => s.PackageQuantity);
                });
            }
            return results;
        }

        public IQueryable<CsTransactionModel> Query(CsTransactionCriteria criteria)
        {
            var transactionType = DataTypeEx.GetType(criteria.TransactionType);
            var listSearch = GetTransaction(transactionType);
            if (listSearch == null || listSearch.Any() == false) return null;

            IQueryable<CsTransactionModel> results = null;

            switch (criteria.TransactionType)
            {
                case TransactionTypeEnum.InlandTrucking:
                    results = QueryIT(criteria, listSearch);
                    break;
                case TransactionTypeEnum.AirExport:
                    results = QueryAE(criteria, listSearch);
                    break;
                case TransactionTypeEnum.AirImport:
                    results = QueryAI(criteria, listSearch);
                    break;
                case TransactionTypeEnum.SeaConsolExport:
                    results = QuerySEC(criteria, listSearch);
                    break;
                case TransactionTypeEnum.SeaConsolImport:
                    results = QuerySIC(criteria, listSearch);
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    results = QuerySEF(criteria, listSearch);
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    results = QuerySIF(criteria, listSearch);
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    results = QuerySEL(criteria, listSearch);
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    results = QuerySIL(criteria, listSearch);
                    break;
                default:
                    break;
            }
            return results;
        }

        /// <summary>
        /// Query Inland Trucking
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QueryIT(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            return null;
        }

        /// <summary>
        /// Query Air Export
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QueryAE(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            var queryTrans = listSearch;
            if (criteria.All == null)
            {
                queryTrans = queryTrans.Where(x =>
                       (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && ((x.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                queryTrans = queryTrans.Where(x =>
                       (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || ((x.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || ((x.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }

            //Search with HouseBill & Surcharge
            IQueryable<CsTransactionModel> dataQuerySur = null;
            if (!string.IsNullOrEmpty(criteria.HWBNo)
                || !string.IsNullOrEmpty(criteria.CustomerId)
                || !string.IsNullOrEmpty(criteria.SaleManId)
                || !string.IsNullOrEmpty(criteria.NotifyPartyId)
                || !string.IsNullOrEmpty(criteria.CreditDebitNo)
                || !string.IsNullOrEmpty(criteria.SoaNo))
            {
                var transactionType = DataTypeEx.GetType(criteria.TransactionType);
                var surcharges = csShipmentSurchargeRepo.Get();
                var houseBills = transactionDetailService.GetHouseBill(transactionType);//csTransactionDetailRepo.Get();
                houseBills = houseBills.Where(x => x.ParentId == null);
                var querySur = from transaction in queryTrans
                               join houseBill in houseBills on transaction.Id equals houseBill.JobId into houseBill2
                               from houseBill in houseBill2.DefaultIfEmpty()
                               join surcharge in surcharges on houseBill.Id equals surcharge.Hblid into surchargeTrans
                               from sur in surchargeTrans.DefaultIfEmpty()
                               select new
                               {
                                   transaction,
                                   HWBNo = houseBill.Hwbno,
                                   houseBill.CustomerId,
                                   houseBill.NotifyPartyId,
                                   houseBill.SaleManId,
                                   sur.CreditNo,
                                   sur.DebitNo,
                                   sur.Soano,
                                   sur.PaySoano
                               };
                if (criteria.All == null)
                {
                    querySur = querySur.Where(x =>
                            (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        &&
                            ((x.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                        &&
                            ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                        &&
                        (
                            (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                        &&
                        (
                            (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    );
                    queryTrans = querySur.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
                else
                {
                    querySur = querySur.Where(x =>
                            (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        ||
                            ((x.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                        ||
                            ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                        ||
                        (
                            (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                        ||
                        (
                            (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    );
                    dataQuerySur = querySur.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }

            IQueryable<CsTransactionModel> result = queryTrans;
            if (dataQuerySur != null)
            {
                if (criteria.All != null)
                {
                    result = result.Union(dataQuerySur);
                    result = result.GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }
            //Sort Array sẽ nhanh hơn
            result = result.ToArray().OrderByDescending(o => o.DatetimeModified).AsQueryable();
            return result;
        }

        /// <summary>
        /// Query Air Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QueryAI(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            var queryTrans = listSearch;
            if (criteria.All == null)
            {
                queryTrans = queryTrans.Where(x =>
                       (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && ((x.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.Eta ?? null) >= (criteria.FromDate ?? null)) && ((x.Eta ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                queryTrans = queryTrans.Where(x =>
                       (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || ((x.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || ((x.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.Eta ?? null) >= (criteria.FromDate ?? null)) && ((x.Eta ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }

            //Search with HouseBill & Surcharge
            IQueryable<CsTransactionModel> dataQuerySur = null;
            if (!string.IsNullOrEmpty(criteria.HWBNo)
                || !string.IsNullOrEmpty(criteria.CustomerId)
                || !string.IsNullOrEmpty(criteria.SaleManId)
                || !string.IsNullOrEmpty(criteria.NotifyPartyId)
                || !string.IsNullOrEmpty(criteria.CreditDebitNo)
                || !string.IsNullOrEmpty(criteria.SoaNo))
            {
                var transactionType = DataTypeEx.GetType(criteria.TransactionType);
                var surcharges = csShipmentSurchargeRepo.Get();
                var houseBills = transactionDetailService.GetHouseBill(transactionType);//csTransactionDetailRepo.Get();
                houseBills = houseBills.Where(x => x.ParentId == null);
                var querySur = from transaction in queryTrans
                               join houseBill in houseBills on transaction.Id equals houseBill.JobId into houseBill2
                               from houseBill in houseBill2.DefaultIfEmpty()
                               join surcharge in surcharges on houseBill.Id equals surcharge.Hblid into surchargeTrans
                               from sur in surchargeTrans.DefaultIfEmpty()
                               select new
                               {
                                   transaction,
                                   HWBNo = houseBill.Hwbno,
                                   houseBill.CustomerId,
                                   houseBill.NotifyPartyId,
                                   houseBill.SaleManId,
                                   sur.CreditNo,
                                   sur.DebitNo,
                                   sur.Soano,
                                   sur.PaySoano
                               };
                if (criteria.All == null)
                {
                    querySur = querySur.Where(x =>
                            (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        &&
                            ((x.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                        &&
                            ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                        &&
                        (
                            (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                        &&
                        (
                            (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    );
                    queryTrans = querySur.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
                else
                {
                    querySur = querySur.Where(x =>
                            (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        ||
                            ((x.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                        ||
                            ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                        ||
                        (
                            (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                        ||
                        (
                            (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    );
                    dataQuerySur = querySur.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }

            IQueryable<CsTransactionModel> result = queryTrans;
            if (dataQuerySur != null)
            {
                if (criteria.All != null)
                {
                    result = result.Union(dataQuerySur);
                    result = result.GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }
            //Sort Array sẽ nhanh hơn
            result = result.ToArray().OrderByDescending(o => o.DatetimeModified).AsQueryable();
            return result;
        }

        /// <summary>
        /// Query Sea Consol Export
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySEC(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            return null;
        }

        /// <summary>
        /// Query Sea Consol Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySIC(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            return null;
        }

        /// <summary>
        /// Query Sea FCL Export
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySEF(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            var queryTrans = listSearch;
            if (criteria.All == null)
            {
                queryTrans = queryTrans.Where(x =>
                       (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && ((x.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                queryTrans = queryTrans.Where(x =>
                       (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || ((x.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || ((x.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }

            //Search with Container Of Shipment
            IQueryable<CsTransactionModel> dataQueryCont = null;
            if (!string.IsNullOrEmpty(criteria.ContainerNo)
                || !string.IsNullOrEmpty(criteria.SealNo)
                || !string.IsNullOrEmpty(criteria.MarkNo))
            {
                var containers = csMawbcontainerRepo.Get();
                var queryCont = from transaction in queryTrans
                                join container in containers on transaction.Id equals container.Mblid into containerTrans
                                from cont in containerTrans.DefaultIfEmpty()
                                select new
                                {
                                    transaction,
                                    cont.ContainerNo,
                                    cont.SealNo,
                                    cont.MarkNo
                                };
                if (criteria.All == null)
                {
                    queryCont = queryCont.Where(x =>
                           (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        && (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
                    queryTrans = queryCont.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
                else
                {
                    queryCont = queryCont.Where(x =>
                           (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
                    dataQueryCont = queryCont.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }

            //Search with HouseBill & Surcharge
            IQueryable<CsTransactionModel> dataQuerySur = null;
            if (!string.IsNullOrEmpty(criteria.HWBNo)
                || !string.IsNullOrEmpty(criteria.CustomerId)
                || !string.IsNullOrEmpty(criteria.SaleManId)
                || !string.IsNullOrEmpty(criteria.NotifyPartyId)
                || !string.IsNullOrEmpty(criteria.CreditDebitNo)
                || !string.IsNullOrEmpty(criteria.SoaNo))
            {
                var transactionType = DataTypeEx.GetType(criteria.TransactionType);
                var surcharges = csShipmentSurchargeRepo.Get();
                var houseBills = transactionDetailService.GetHouseBill(transactionType);//csTransactionDetailRepo.Get();
                houseBills = houseBills.Where(x => x.ParentId == null);
                var querySur = from transaction in queryTrans
                               join houseBill in houseBills on transaction.Id equals houseBill.JobId into houseBill2
                               from houseBill in houseBill2.DefaultIfEmpty()
                               join surcharge in surcharges on houseBill.Id equals surcharge.Hblid into surchargeTrans
                               from sur in surchargeTrans.DefaultIfEmpty()
                               select new
                               {
                                   transaction,
                                   HWBNo = houseBill.Hwbno,
                                   houseBill.CustomerId,
                                   houseBill.NotifyPartyId,
                                   houseBill.SaleManId,
                                   sur.CreditNo,
                                   sur.DebitNo,
                                   sur.Soano,
                                   sur.PaySoano
                               };
                if (criteria.All == null)
                {
                    querySur = querySur.Where(x =>
                            (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        &&
                            ((x.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                        &&
                            ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                        &&
                        (
                            (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                        &&
                        (
                            (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    );
                    queryTrans = querySur.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
                else
                {
                    querySur = querySur.Where(x =>
                            (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        ||
                            ((x.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                        ||
                            ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                        ||
                        (
                            (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                        ||
                        (
                            (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    );
                    dataQuerySur = querySur.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }

            IQueryable<CsTransactionModel> result = queryTrans;
            if (dataQueryCont != null)
            {
                if (criteria.All != null)
                {
                    result = result.Union(dataQueryCont);
                    result = result.GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }
            if (dataQuerySur != null)
            {
                if (criteria.All != null)
                {
                    result = result.Union(dataQuerySur);
                    result = result.GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }
            //Sort Array sẽ nhanh hơn
            result = result.ToArray().OrderByDescending(o => o.DatetimeModified).AsQueryable();
            return result;
        }

        /// <summary>
        /// Query Sea FCL Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySIF(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            var queryTrans = listSearch;
            if (criteria.All == null)
            {
                queryTrans = queryTrans.Where(x =>
                       (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.Eta ?? null) >= (criteria.FromDate ?? null)) && ((x.Eta ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                queryTrans = queryTrans.Where(x =>
                       (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || ((x.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || ((x.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.Eta ?? null) >= (criteria.FromDate ?? null)) && ((x.Eta ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }

            //Search with Container Of Shipment
            IQueryable<CsTransactionModel> dataQueryCont = null;
            if (!string.IsNullOrEmpty(criteria.ContainerNo)
                || !string.IsNullOrEmpty(criteria.SealNo)
                || !string.IsNullOrEmpty(criteria.MarkNo))
            {
                var containers = csMawbcontainerRepo.Get();
                var queryCont = from transaction in queryTrans
                                join container in containers on transaction.Id equals container.Mblid into containerTrans
                                from cont in containerTrans.DefaultIfEmpty()
                                select new
                                {
                                    transaction,
                                    cont.ContainerNo,
                                    cont.SealNo,
                                    cont.MarkNo
                                };
                if (criteria.All == null)
                {
                    queryCont = queryCont.Where(x =>
                           (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        && (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
                    queryTrans = queryCont.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
                else
                {
                    queryCont = queryCont.Where(x =>
                           (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
                    dataQueryCont = queryCont.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }

            //Search with HouseBill & Surcharge
            IQueryable<CsTransactionModel> dataQuerySur = null;
            if (!string.IsNullOrEmpty(criteria.HWBNo)
                || !string.IsNullOrEmpty(criteria.CustomerId)
                || !string.IsNullOrEmpty(criteria.SaleManId)
                || !string.IsNullOrEmpty(criteria.NotifyPartyId)
                || !string.IsNullOrEmpty(criteria.CreditDebitNo)
                || !string.IsNullOrEmpty(criteria.SoaNo))
            {
                var transactionType = DataTypeEx.GetType(criteria.TransactionType);
                var surcharges = csShipmentSurchargeRepo.Get();
                var houseBills = transactionDetailService.GetHouseBill(transactionType);//csTransactionDetailRepo.Get();
                houseBills = houseBills.Where(x => x.ParentId == null);
                var querySur = from transaction in queryTrans
                               join houseBill in houseBills on transaction.Id equals houseBill.JobId into houseBill2
                               from houseBill in houseBill2.DefaultIfEmpty()
                               join surcharge in surcharges on houseBill.Id equals surcharge.Hblid into surchargeTrans
                               from sur in surchargeTrans.DefaultIfEmpty()
                               select new
                               {
                                   transaction,
                                   HWBNo = houseBill.Hwbno,
                                   houseBill.CustomerId,
                                   houseBill.NotifyPartyId,
                                   houseBill.SaleManId,
                                   sur.CreditNo,
                                   sur.DebitNo,
                                   sur.Soano,
                                   sur.PaySoano
                               };
                if (criteria.All == null)
                {
                    querySur = querySur.Where(x =>
                            (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        &&
                            ((x.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                        &&
                            ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                        &&
                        (
                            (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                        &&
                        (
                            (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    );
                    queryTrans = querySur.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
                else
                {
                    querySur = querySur.Where(x =>
                            (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        ||
                            ((x.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                        ||
                            ((x.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                        ||
                        (
                            (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                        ||
                        (
                            (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    );
                    dataQuerySur = querySur.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }

            IQueryable<CsTransactionModel> result = queryTrans;
            if (dataQueryCont != null)
            {
                if (criteria.All != null)
                {
                    result = result.Union(dataQueryCont);
                    result = result.GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }
            if (dataQuerySur != null)
            {
                if (criteria.All != null)
                {
                    result = result.Union(dataQuerySur);
                    result = result.GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault());
                }
            }
            //Sort Array sẽ nhanh hơn
            result = result.ToArray().OrderByDescending(o => o.DatetimeModified).AsQueryable();
            return result;
        }

        /// <summary>
        /// Query Sea LCL Export
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySEL(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            //Sử dụng lại của service Sea FCL Export
            var result = QuerySEF(criteria, listSearch);
            return result;
        }

        /// <summary>
        /// Query Sea LCL Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySIL(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            //Sử dụng lại của service Sea FCL Import
            var result = QuerySIF(criteria, listSearch);
            return result;
        }

        #endregion -- LIST & PAGING --

        public List<object> GetListTotalHB(Guid JobId)
        {
            var shipment = DataContext.Get(x => x.Id == JobId).FirstOrDefault();
            var houseBills = transactionDetailService.GetHouseBill(shipment.TransactionType);

            List<object> returnList = new List<object>();
            var housebills = houseBills.Where(x => x.JobId == JobId && x.ParentId == null);//csTransactionDetailRepo.Get(x => x.JobId == JobId).ToList();
            foreach (var item in housebills)
            {
                var totalBuying = (decimal?)0;
                var totalSelling = (decimal?)0;
                var totalobh = (decimal?)0;
                var totallogistic = (decimal?)0;


                var totalBuyingUSD = (decimal?)0;
                var totalSellingUSD = (decimal?)0;
                var totalobhUSD = (decimal?)0;
                var totallogisticUSD = (decimal?)0;

                var charges = csShipmentSurchargeRepo.Get(x => x.Hblid == item.Id).ToList();

                foreach (var c in charges)
                {
                    var exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(c.FinalExchangeRate, c.ExchangeDate, c.CurrencyId, DocumentConstants.CURRENCY_LOCAL);//currencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == c.ExchangeDate.Value.Date && x.CurrencyFromId == c.CurrencyId && x.CurrencyToId == "VND")).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    var UsdToVnd = currencyExchangeService.CurrencyExchangeRateConvert(c.FinalExchangeRate, c.ExchangeDate, DocumentConstants.CURRENCY_USD, DocumentConstants.CURRENCY_LOCAL);//currencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == c.ExchangeDate.Value.Date && x.CurrencyFromId == "USD" && x.CurrencyToId == "VND")).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    var rate = exchangeRate;
                    var usdToVndRate = UsdToVnd;
                    if (c.Type.ToLower() == DocumentConstants.CHARGE_BUY_TYPE.ToLower())
                    {
                        totalBuying += c.Total * rate;
                        totalBuyingUSD += (totalBuying / usdToVndRate);
                    }
                    if (c.Type.ToLower() == DocumentConstants.CHARGE_SELL_TYPE.ToLower())
                    {
                        totalSelling += c.Total * rate;
                        totalSellingUSD += (totalSelling / usdToVndRate);
                    }
                    if (c.Type.ToLower() == DocumentConstants.CHARGE_OBH_TYPE.ToLower())
                    {
                        totalobh += c.Total * rate;
                        totalobhUSD += (totalobh / usdToVndRate);
                    }
                }

                var totalVND = totalSelling - totalBuying - totallogistic;
                var totalUSD = totalSellingUSD - totalBuyingUSD - totallogisticUSD;
                var obj = new { item.Hwbno, totalVND, totalUSD };
                returnList.Add(obj);
            }
            return returnList;
        }


        public ResultHandle ImportMulti()
        {
            try
            {

                Guid jobId = new Guid("b254dbf4-0edb-4044-a53d-8df5165d8987");
                var transaction = DataContext.Get(x => x.Id == jobId).FirstOrDefault();
                for (int i = 0; i < 500; i++)
                {
                    transaction.Id = Guid.NewGuid();
                    transaction.JobNo = CreateJobNoByTransactionType(TransactionTypeEnum.SeaFCLImport, transaction.TransactionType);
                    transaction.Mawb = transaction.JobNo;
                    transaction.UserCreated = transaction.UserModified = currentUser.UserID;
                    transaction.DatetimeCreated = transaction.DatetimeModified = DateTime.Now;
                    transaction.GroupId = currentUser.GroupId;
                    transaction.DepartmentId = currentUser.DepartmentId;
                    transaction.OfficeId = currentUser.OfficeID;
                    transaction.CompanyId = currentUser.CompanyID;
                    transaction.Active = true;
                    var hsTrans = transactionRepository.Add(transaction, true);
                    var containers = csMawbcontainerRepo.Get(x => x.Mblid == jobId).ToList();
                    if (containers != null)
                    {
                        containers.ForEach(x =>
                        {
                            x.Id = Guid.NewGuid();
                            x.Mblid = transaction.Id;
                            x.UserModified = transaction.UserCreated;
                            x.DatetimeModified = DateTime.Now;
                        });
                        var hsCont = csMawbcontainerRepo.Add(containers, true);
                    }
                    var dimensions = dimensionDetailRepository.Get(x => x.Mblid == jobId).ToList();
                    if (dimensions != null)
                    {
                        dimensions.ForEach(x =>
                        {
                            x.Id = Guid.NewGuid();
                            x.Mblid = transaction.Id;
                            x.UserCreated = transaction.UserCreated;
                            x.DatetimeCreated = DateTime.Now;
                        });
                        dimensionDetailRepository.Add(dimensions, true);
                    }
                    var detailTrans = csTransactionDetailRepo.Get(x => x.JobId == jobId);
                    if (detailTrans != null)
                    {
                        int countDetail = csTransactionDetailRepo.Count(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                                            && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                                            && x.DatetimeCreated.Value.Day == DateTime.Now.Day);
                        string generatePrefixHouse = GenerateID.GeneratePrefixHousbillNo();

                        if (csTransactionDetailRepo.Any(x => x.Hwbno.IndexOf(generatePrefixHouse, StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            generatePrefixHouse = DocumentConstants.SEF_HBL
                                + GenerateID.GeneratePrefixHousbillNo();
                        }
                        foreach (var item in detailTrans)
                        {
                            var houseId = item.Id;
                            item.Id = Guid.NewGuid();
                            item.JobId = transaction.Id;
                            item.Hwbno = GenerateID.GenerateHousebillNo(generatePrefixHouse, countDetail);
                            countDetail = countDetail + 1;
                            item.Active = true;
                            item.UserCreated = transaction.UserCreated;  //ChangeTrackerHelper.currentUser;
                            item.DatetimeCreated = item.DatetimeModified = DateTime.Now;

                            item.GroupId = currentUser.GroupId;
                            item.DepartmentId = currentUser.DepartmentId;
                            item.OfficeId = currentUser.OfficeID;
                            item.CompanyId = currentUser.CompanyID;

                            csTransactionDetailRepo.Add(item, true);
                            var houseContainers = csMawbcontainerRepo.Get(x => x.Hblid == houseId);
                            if (houseContainers != null)
                            {
                                foreach (var x in houseContainers)
                                {
                                    x.Id = Guid.NewGuid();
                                    x.Hblid = item.Id;
                                    x.ContainerNo = string.Empty;
                                    x.SealNo = string.Empty;
                                    x.MarkNo = string.Empty;
                                    x.UserModified = transaction.UserCreated;
                                    x.DatetimeModified = DateTime.Now;
                                    csMawbcontainerRepo.Add(x, true);
                                }
                            }
                            var houseDimensions = dimensionDetailRepository.Get(x => x.Hblid == item.Id);
                            if (houseDimensions != null)
                            {
                                foreach (var x in houseDimensions)
                                {
                                    x.Id = Guid.NewGuid();
                                    x.Hblid = item.Id;
                                    x.UserCreated = transaction.UserCreated;
                                    x.DatetimeCreated = DateTime.Now;
                                    dimensionDetailRepository.Add(x, true);
                                }
                            }
                            var charges = csShipmentSurchargeRepo.Get(x => x.Hblid == houseId);
                            if (charges != null)
                            {
                                foreach (var charge in charges)
                                {
                                    charge.Id = Guid.NewGuid();
                                    charge.UserCreated = transaction.UserCreated;
                                    charge.DatetimeCreated = DateTime.Now;
                                    charge.Hblid = item.Id;
                                    charge.Soano = null;
                                    charge.PaySoano = null;
                                    charge.CreditNo = null;
                                    charge.DebitNo = null;
                                    charge.Soaclosed = null;
                                    charge.SettlementCode = null;
                                    csShipmentSurchargeRepo.Add(charge, true);
                                }
                            }
                            var freightCharge = csArrivalFrieghtChargeService.Get(x => x.Hblid == houseId);
                            if (freightCharge != null)
                            {
                                foreach (var freight in freightCharge)
                                {
                                    freight.Id = Guid.NewGuid();
                                    freight.UserCreated = transaction.UserCreated;
                                    freight.Hblid = item.Id;
                                    csArrivalFrieghtChargeService.Add(freight, true);
                                }
                            }
                        }
                    }
                    transactionRepository.SubmitChanges();
                    csTransactionDetailRepo.SubmitChanges();
                    csMawbcontainerRepo.SubmitChanges();
                    containerService.SubmitChanges();
                    csShipmentSurchargeRepo.SubmitChanges();
                    csArrivalFrieghtChargeService.SubmitChanges();
                    dimensionDetailRepository.SubmitChanges();
                }
                return new ResultHandle { Status = true, Message = "Import successfully!!!", Data = transaction };
            }
            catch (Exception ex)
            {
                var result = new HandleState(ex.Message);
                return new ResultHandle { Data = new object { }, Message = ex.Message, Status = true };
            }
        }
        public ResultHandle ImportCSTransaction(CsTransactionEditModel model)
        {
            IQueryable<CsTransactionDetail> detailTrans = csTransactionDetailRepo.Get(x => x.JobId == model.Id && x.ParentId == null);
            if (string.IsNullOrEmpty(model.Mawb) && detailTrans.Select(x => x.Id).Count() > 0)
                return new ResultHandle { Status = false, Message = "This shipment did't have MBL No. You can't import or duplicate it." };

            var transaction = GetDefaultJob(model);
            List<CsMawbcontainer> containers = null;
            List<CsDimensionDetail> dimensionDetails = null;
            List<CsShipmentSurcharge> surcharges = null;
            List<CsArrivalFrieghtCharge> freightCharges = null;
            if (model.CsMawbcontainers != null)
            {
                containers = new List<CsMawbcontainer>();
                var masterContainers = GetMasterBillcontainer(transaction.Id, model.CsMawbcontainers);
                containers.AddRange(masterContainers);
            }
            if (model.DimensionDetails != null)
            {
                dimensionDetails = new List<CsDimensionDetail>();
                var masterDimensionDetails = GetMasterDimensiondetails(transaction.Id, model.DimensionDetails);
                dimensionDetails.AddRange(masterDimensionDetails);
            }
            if (model.TransactionType == "AI" || model.TransactionType == "AE")
            {
                detailTrans = detailTrans.Where(x => x.JobId == model.Id && x.ParentId == null);
            }
            if (detailTrans != null)
            {
                int countDetail = csTransactionDetailRepo.Count(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                                    && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                                    && x.DatetimeCreated.Value.Day == DateTime.Now.Day);
                string generatePrefixHouse = GenerateID.GeneratePrefixHousbillNo();

                if (csTransactionDetailRepo.Any(x => x.Hwbno.IndexOf(generatePrefixHouse, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    generatePrefixHouse = DocumentConstants.SEF_HBL
                        + GenerateID.GeneratePrefixHousbillNo();
                }
                freightCharges = new List<CsArrivalFrieghtCharge>();
                surcharges = new List<CsShipmentSurcharge>();
                foreach (var item in detailTrans)
                {
                    var oldHouseId = item.Id;
                    item.Id = Guid.NewGuid();
                    item.JobId = transaction.Id;
                    item.ManifestRefNo = null;
                    item.DeliveryOrderNo = null;
                    item.DeliveryOrderPrintedDate = null;
                    item.DosentTo1 = null;
                    item.DosentTo2 = null;
                    item.Dofooter = null;
                    item.ArrivalNo = null;
                    item.ArrivalFirstNotice = null;
                    item.ArrivalSecondNotice = null;
                    item.ArrivalHeader = null;
                    item.ArrivalFooter = null;
                    item.ArrivalDate = null;
                    item.Hwbno = GenerateID.GenerateHousebillNo(generatePrefixHouse, countDetail);
                    item.Active = true;
                    item.UserCreated = transaction.UserCreated;
                    item.DatetimeCreated = DateTime.Now;
                    item.GroupId = currentUser.GroupId;
                    item.DepartmentId = currentUser.DepartmentId;
                    item.OfficeId = currentUser.OfficeID;
                    item.CompanyId = currentUser.CompanyID;
                    var housebillcontainers = GetHouseBillContainers(oldHouseId, item.Id);
                    if (housebillcontainers != null) containers.AddRange(housebillcontainers);
                    var housebillDimensions = GetHouseBillDimensions(oldHouseId, item.Id);
                    if (housebillDimensions != null) dimensionDetails.AddRange(housebillDimensions);
                    var houseSurcharges = GetCharges(oldHouseId, item.Id);
                    if (houseSurcharges != null)
                    {
                        surcharges.AddRange(houseSurcharges);
                    }
                    var houseFreigcharges = GetFreightCharges(oldHouseId, item.Id);
                    if (houseFreigcharges != null)
                    {
                        freightCharges.AddRange(houseFreigcharges);
                    }
                    countDetail = countDetail + 1;
                }
            }
            try
            {
                var hsTrans = transactionRepository.Add(transaction, false);
                if (hsTrans.Success)
                {
                    var hsTransDetails = csTransactionDetailRepo.Add(detailTrans, false);
                    var hsContainers = csMawbcontainerRepo.Add(containers, false);
                    var hsDimentions = dimensionDetailRepository.Add(dimensionDetails, false);
                    var hsSurcharges = csShipmentSurchargeRepo.Add(surcharges, false);
                    var hsFreighcharges = freighchargesRepository.Add(freightCharges, false);
                    transactionRepository.SubmitChanges();
                    csTransactionDetailRepo.SubmitChanges();
                    csMawbcontainerRepo.SubmitChanges();
                    dimensionDetailRepository.SubmitChanges();
                    csShipmentSurchargeRepo.SubmitChanges();
                    freighchargesRepository.SubmitChanges();
                    return new ResultHandle { Status = true, Message = "Import successfully!!!", Data = transaction };
                }
                return new ResultHandle { Status = hsTrans.Success, Message = hsTrans.Message.ToString() };
            }
            catch (Exception ex)
            {
                return new ResultHandle { Status = false, Message = ex.Message };
            }
        }

        private CsTransaction GetDefaultJob(CsTransactionEditModel model)
        {
            var transaction = mapper.Map<CsTransaction>(model);
            transaction.Id = Guid.NewGuid();
            transaction.JobNo = CreateJobNoByTransactionType(model.TransactionTypeEnum, model.TransactionType);
            transaction.UserCreated = transaction.UserModified = currentUser.UserID;
            transaction.DatetimeCreated = transaction.DatetimeModified = DateTime.Now;
            transaction.GroupId = currentUser.GroupId;
            transaction.DepartmentId = currentUser.DepartmentId;
            transaction.OfficeId = currentUser.OfficeID;
            transaction.CompanyId = currentUser.CompanyID;
            transaction.CurrentStatus = TermData.Processing;
            transaction.Active = true;
            return transaction;
        }

        private List<CsArrivalFrieghtCharge> GetFreightCharges(Guid oldHouseId, Guid newHouseId)
        {
            List<CsArrivalFrieghtCharge> charges = null;
            var freightCharge = csArrivalFrieghtChargeService.Get(x => x.Hblid == oldHouseId);
            if (freightCharge.Select(x => x.Id).Count() != 0)
            {
                charges = new List<CsArrivalFrieghtCharge>();
                foreach (var item in freightCharge)
                {
                    item.Id = Guid.NewGuid();
                    item.UserCreated = currentUser.UserID;
                    item.DatetimeCreated = DateTime.Now;
                    item.Hblid = newHouseId;
                    charges.Add(item);
                }
            }
            return charges;
        }

        private List<CsShipmentSurcharge> GetCharges(Guid oldHouseId, Guid newHouseId)
        {
            List<CsShipmentSurcharge> surCharges = null;
            var charges = csShipmentSurchargeRepo.Get(x => x.Hblid == oldHouseId);
            if (charges.Select(x => x.Id).Count() != 0)
            {
                surCharges = new List<CsShipmentSurcharge>();
                foreach (var item in charges)
                {
                    item.Id = Guid.NewGuid();
                    item.UserCreated = currentUser.UserID;
                    item.DatetimeCreated = DateTime.Now;
                    item.Hblid = newHouseId;
                    item.Soano = null;
                    item.PaySoano = null;
                    item.CreditNo = null;
                    item.DebitNo = null;
                    item.Soaclosed = null;
                    item.SettlementCode = null;
                    surCharges.Add(item);
                }
            }
            return surCharges;
        }

        private List<CsDimensionDetail> GetHouseBillDimensions(Guid oldHouseId, Guid newHouseId)
        {
            List<CsDimensionDetail> dimensionDetails = null;
            var houseDimensions = dimensionDetailRepository.Get(x => x.Hblid == oldHouseId);
            if (houseDimensions.Select(x => x.Id).Count() != 0)
            {
                dimensionDetails = new List<CsDimensionDetail>();
                foreach (var item in houseDimensions)
                {
                    item.Id = Guid.NewGuid();
                    item.Mblid = null;
                    item.AirWayBillId = null;
                    item.Hblid = newHouseId;
                    item.UserCreated = currentUser.UserID;
                    item.DatetimeCreated = DateTime.Now;
                    dimensionDetails.Add(item);
                }
            }
            return dimensionDetails;
        }

        private List<CsMawbcontainer> GetHouseBillContainers(Guid oldHouseId, Guid newHouseId)
        {
            List<CsMawbcontainer> containers = null;
            var houseContainers = csMawbcontainerRepo.Get(x => x.Hblid == oldHouseId);
            if (houseContainers.Select(x => x.Id).Count() != 0)
            {
                containers = new List<CsMawbcontainer>();
                foreach (var x in houseContainers)
                {
                    x.Id = Guid.NewGuid();
                    x.Mblid = null;
                    x.Hblid = newHouseId;
                    x.ContainerNo = string.Empty;
                    x.SealNo = string.Empty;
                    x.MarkNo = string.Empty;
                    x.UserModified = currentUser.UserID;
                    x.DatetimeModified = DateTime.Now;
                    containers.Add(x);
                }
            }
            return containers;
        }

        private List<CsDimensionDetail> GetMasterDimensiondetails(Guid jobId, List<CsDimensionDetailModel> dimensionDetails)
        {
            List<CsDimensionDetail> dimensions = new List<CsDimensionDetail>();
            foreach (var item in dimensionDetails)
            {
                item.Id = Guid.NewGuid();
                item.Mblid = jobId;
                item.Hblid = null;
                item.UserCreated = currentUser.UserID;
                item.DatetimeCreated = DateTime.Now;
                dimensions.Add(item);
            }
            return dimensions;
        }

        private List<CsMawbcontainer> GetMasterBillcontainer(Guid jobId, List<CsMawbcontainerModel> csMawbcontainers)
        {
            var containers = new List<CsMawbcontainer>();
            foreach (var item in csMawbcontainers)
            {
                item.Id = Guid.NewGuid();
                item.Mblid = jobId;
                item.Hblid = null;
                item.UserModified = currentUser.UserID;
                item.DatetimeModified = DateTime.Now;
                containers.Add(item);
            }
            return containers;
        }

        #region -- PREVIEW --       

        public Crystal PreviewSIFFormPLsheet(Guid jobId, Guid hblId, string currency)
        {
            Crystal result = null;
            var _currentUser = currentUser.UserName;
            var shipment = DataContext.First(x => x.Id == jobId);
            if (shipment == null) return result;
            var agent = catPartnerRepo.Get(x => x.Id == shipment.AgentId).FirstOrDefault();
            var supplier = catPartnerRepo.Get(x => x.Id == shipment.ColoaderId).FirstOrDefault();

            var pol = catPlaceRepo.Get(x => x.Id == shipment.Pol).FirstOrDefault();
            var pod = catPlaceRepo.Get(x => x.Id == shipment.Pod).FirstOrDefault();
            var polCountry = string.Empty;
            var podCountry = string.Empty;
            if (pol != null)
            {
                polCountry = catCountryRepo.Get(x => x.Id == pol.CountryId).FirstOrDefault()?.NameEn;
            }
            if (pod != null)
            {
                podCountry = catCountryRepo.Get(x => x.Id == pod.CountryId).FirstOrDefault()?.NameEn;
            }
            var _polFull = pol?.NameEn + (!string.IsNullOrEmpty(polCountry) ? ", " + polCountry : string.Empty);
            var _podFull = pod?.NameEn + (!string.IsNullOrEmpty(podCountry) ? ", " + podCountry : string.Empty);

            //CsMawbcontainerCriteria contCriteria = new CsMawbcontainerCriteria { Mblid = jobId };
            //var containerList = containerService.Query(contCriteria);
            //var _containerNoList = string.Empty;
            //if (containerList.Count() > 0)
            //{
            //    _containerNoList = String.Join("\r\n", containerList.Select(x => !string.IsNullOrEmpty(x.ContainerNo) || !string.IsNullOrEmpty(x.SealNo) ? x.ContainerNo + "/" + x.SealNo : string.Empty));
            //}

            var _transDate = shipment.DatetimeCreated != null ? shipment.DatetimeCreated.Value : DateTime.Now; //CreatedDate of shipment
            var _etdDate = shipment.Etd != null ? shipment.Etd.Value.ToString("dd MMM yyyy") : string.Empty; //ETD
            var _etaDate = shipment.Eta != null ? shipment.Eta.Value.ToString("dd MMM yyyy") : string.Empty; //ETA
            var _grossWeight = shipment.GrossWeight ?? 0;//Đang lấy GrossWeight của Shipment
            var _netWeight = shipment.NetWeight ?? 0;//Đang lấy NetWeight của Shipment
            var _chargeWeight = shipment.ChargeWeight ?? 0;//Đang lấy ChargeWeight của Shipment
            var _dateNow = DateTime.Now.ToString("dd MMMM yyyy");
            var commondity = shipment.Commodity?.ToUpper();
            if (shipment.TransactionType == "AI" || shipment.TransactionType == "AE")
            {
                commondity = commodityRepository.Get(x => x.Code == shipment.Commodity).FirstOrDefault()?.CommodityNameEn;
            }
            var _containerNoList = string.Empty;

            var listCharge = new List<FormPLsheetReport>();

            CsTransactionDetailCriteria criteria = new CsTransactionDetailCriteria { JobId = jobId };
            var listHousebill = transactionDetailService.GetByJob(criteria);
            if (hblId != Guid.Empty)
            {
                listHousebill = listHousebill.Where(x => x.Id == hblId).ToList();
            }
            var _hblNoList = string.Empty;
            var _shipmentType = GetShipmentTypeForPreviewPL(shipment.TransactionType) + shipment.TypeOfService;
            if (listHousebill.Count > 0)
            {
                _hblNoList = String.Join(";", listHousebill.Select(x => x.Hwbno));

                var housebillFirst = listHousebill.First();
                var userSaleman = sysUserRepo.Get(x => x.Id == housebillFirst.SaleManId).FirstOrDefault();
                var shipper = catPartnerRepo.Get(x => x.Id == housebillFirst.ShipperId).FirstOrDefault()?.PartnerNameEn;
                var consignee = catPartnerRepo.Get(x => x.Id == housebillFirst.ConsigneeId).FirstOrDefault()?.PartnerNameEn;

                var surcharges = new List<CsShipmentSurchargeDetailsModel>();
                foreach (var housebill in listHousebill)
                {
                    var surcharge = surchargeService.GetByHB(housebill.Id);
                    surcharges.AddRange(surcharge);

                    CsMawbcontainerCriteria contCriteria = new CsMawbcontainerCriteria { Hblid = housebill.Id };
                    var containerList = containerService.Query(contCriteria);                    
                    if (containerList.Count() > 0)
                    {
                        _containerNoList += (!string.IsNullOrEmpty(_containerNoList) ? "\r\n" : "") + String.Join("\r\n", containerList.Select(x => !string.IsNullOrEmpty(x.ContainerNo) || !string.IsNullOrEmpty(x.SealNo) ? x.ContainerNo + "/" + x.SealNo : string.Empty));
                }
                }

                if (surcharges.Count > 0)
                {
                    var units = catUnitRepo.Get();
                    foreach (var surcharge in surcharges)
                    {
                        var unitCode = units.FirstOrDefault(x => x.Id == surcharge.UnitId)?.Code;
                        bool isOBH = false;
                        decimal cost = 0;
                        decimal revenue = 0;
                        decimal costNonVat = 0;
                        decimal revenueNonVat = 0;
                        decimal saleProfitIncludeVAT = 0;
                        decimal saleProfitNonVAT = 0;
                        string partnerName = string.Empty;
                        if (surcharge.Type == DocumentConstants.CHARGE_OBH_TYPE)
                        {
                            isOBH = true;
                            partnerName = surcharge.PayerName;
                        }
                        if (surcharge.Type == DocumentConstants.CHARGE_BUY_TYPE)
                        {
                            cost = surcharge.Total;
                            costNonVat = (surcharge.Quantity * surcharge.UnitPrice) ?? 0;
                        }
                        if (surcharge.Type == DocumentConstants.CHARGE_SELL_TYPE)
                        {
                            revenue = surcharge.Total;
                            revenueNonVat = (surcharge.Quantity * surcharge.UnitPrice) ?? 0;
                        }
                        saleProfitIncludeVAT = cost + revenue;
                        saleProfitNonVAT = costNonVat + revenueNonVat;

                        decimal _exchangeRateUSD = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, DocumentConstants.CURRENCY_USD);
                        decimal _exchangeRateLocal = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, DocumentConstants.CURRENCY_LOCAL);

                        var charge = new FormPLsheetReport();
                        charge.COSTING = "COSTING";
                        charge.TransID = shipment.JobNo?.ToUpper(); //JobNo of shipment
                        charge.TransDate = _transDate;
                        charge.HWBNO = surcharge.Hwbno?.ToUpper();
                        charge.MAWB = shipment.Mawb?.ToUpper(); //MasterBill of shipment
                        charge.PartnerName = string.Empty; //NOT USE
                        charge.ContactName = userSaleman?.Username; //Saleman đầu tiên của list housebill
                        charge.ShipmentType = _shipmentType;
                        charge.NominationParty = string.Empty;
                        charge.Nominated = true; //Gán cứng
                        charge.POL = _polFull?.ToUpper();
                        charge.POD = _podFull?.ToUpper();
                        charge.Commodity = commondity?.ToUpper();
                        charge.Volumne = string.Empty; //Gán rỗng
                        charge.Carrier = supplier?.PartnerNameEn?.ToUpper();
                        charge.Agent = agent?.PartnerNameEn?.ToUpper();
                        charge.ATTN = shipper?.ToUpper(); //Shipper đầu tiên của list housebill
                        charge.Consignee = consignee?.ToUpper(); //Consignee đầu tiên của list housebill
                        charge.ContainerNo = _containerNoList?.ToUpper(); //Danh sách container của Shipment (Format: contNo/SealNo)
                        charge.OceanVessel = shipment.FlightVesselName?.ToUpper(); //Tên chuyến bay
                        charge.LocalVessel = shipment.FlightVesselName?.ToUpper(); //Tên chuyến bay
                        charge.FlightNo = shipment.VoyNo?.ToUpper(); //Mã chuyến bay
                        charge.SeaImpVoy = string.Empty;//Gán rỗng
                        charge.LoadingDate = _etdDate; //ETD
                        charge.ArrivalDate = _etaDate; //ETA
                        charge.FreightCustomer = string.Empty; //NOT USE
                        charge.FreightColoader = 0; //NOT USE
                        charge.PayableAccount = surcharge.PartnerName?.ToUpper();//Partner name of charge
                        charge.Description = surcharge.ChargeNameEn; //Charge name of charge
                        charge.Curr = surcharge.CurrencyId; //Currency of charge
                        charge.VAT = 0; //NOT USE
                        charge.VATAmount = 0; //NOT USE
                        charge.Cost = cost; //Phí chi của charge
                        charge.Revenue = revenue; //Phí thu của charge
                        charge.Exchange = currency == DocumentConstants.CURRENCY_USD ? _exchangeRateUSD * saleProfitIncludeVAT : 0; //Exchange phí của charge về USD
                        charge.VNDExchange = currency == DocumentConstants.CURRENCY_LOCAL ? _exchangeRateLocal : 0;
                        charge.Paid = (revenue > 0 || cost < 0) && isOBH == false ? false : true;
                        charge.DatePaid = DateTime.Now; //NOT USE
                        charge.Docs = surcharge.InvoiceNo; //InvoiceNo of charge
                        charge.Notes = surcharge.Notes;
                        charge.InputData = string.Empty; //Gán rỗng
                        charge.SalesProfit = currency == DocumentConstants.CURRENCY_USD ? _exchangeRateUSD * saleProfitNonVAT : _exchangeRateLocal * saleProfitNonVAT; //Non VAT
                        charge.Quantity = surcharge.Quantity;
                        charge.UnitPrice = surcharge.UnitPrice ?? 0;
                        charge.Unit = unitCode;
                        charge.LastRevised = _dateNow;
                        charge.OBH = isOBH;
                        charge.ExtRateVND = 0; //NOT USE
                        charge.KBck = true; //NOT USE
                        charge.NoInv = true; //NOT USE
                        charge.Approvedby = string.Empty; //Gán rỗng
                        charge.ApproveDate = DateTime.Now; //NOT USE
                        charge.SalesCurr = currency;
                        charge.GW = _grossWeight;//Đang lấy GrossWeight của Shipment
                        charge.MCW = _netWeight;//Đang lấy NetWeight của Shipment
                        charge.HCW = _chargeWeight;//Đang lấy ChargeWeight của Shipment
                        charge.PaymentTerm = string.Empty; //NOT USE
                        charge.DetailNotes = string.Empty; //Gán rỗng
                        charge.ExpressNotes = string.Empty; //Gán rỗng
                        charge.InvoiceNo = string.Empty; //NOT USE
                        charge.CodeVender = string.Empty; //NOT USE
                        charge.CodeCus = string.Empty; //NOT USE
                        charge.Freight = true; //NOT USE
                        charge.Collect = true; //NOT USE
                        charge.FreightPayableAt = string.Empty; //NOT USE
                        charge.PaymentTime = 0; //NOT USE
                        charge.PaymentTimeCus = 0; //NOT USE
                        charge.Noofpieces = 0; //NOT USE
                        charge.UnitPieaces = string.Empty; //NOT USE
                        charge.TpyeofService = string.Empty; //NOT USE
                        charge.ShipmentSource = shipment.ShipmentType?.ToUpper();
                        charge.RealCost = true;

                        listCharge.Add(charge);
                    }
                }
                else
                {
                    var charge = new FormPLsheetReport();
                    charge.COSTING = "COSTING";
                    charge.TransID = shipment.JobNo?.ToUpper(); //JobNo of shipment
                    charge.TransDate = _transDate;
                    charge.MAWB = shipment.Mawb?.ToUpper(); //MasterBill of shipment
                    charge.ContactName = userSaleman?.Username; //Saleman đầu tiên của list housebill
                    charge.ShipmentType = _shipmentType;
                    charge.Nominated = true;
                    charge.POL = _polFull?.ToUpper();
                    charge.POD = _podFull?.ToUpper();
                    charge.Commodity = commondity?.ToUpper();
                    charge.Carrier = supplier?.PartnerNameEn?.ToUpper();
                    charge.Agent = agent?.PartnerNameEn?.ToUpper();
                    charge.ATTN = shipper?.ToUpper(); //Shipper đầu tiên của list housebill
                    charge.Consignee = consignee?.ToUpper(); //Consignee đầu tiên của list housebill
                    charge.ContainerNo = _containerNoList?.ToUpper(); //Danh sách container của Shipment (Format: contNo/SealNo)
                    charge.OceanVessel = shipment.FlightVesselName?.ToUpper(); //Tên chuyến bay
                    charge.LocalVessel = shipment.FlightVesselName?.ToUpper(); //Tên chuyến bay
                    charge.FlightNo = shipment.VoyNo?.ToUpper(); //Mã chuyến bay
                    charge.SeaImpVoy = string.Empty;
                    charge.LoadingDate = _etdDate; //ETD
                    charge.ArrivalDate = _etaDate; //ETA
                    charge.LastRevised = _dateNow;
                    charge.SalesCurr = currency;
                    charge.GW = _grossWeight;//Đang lấy GrossWeight của Shipment
                    charge.MCW = _netWeight;//Đang lấy NetWeight của Shipment
                    charge.HCW = _chargeWeight;//Đang lấy ChargeWeight của Shipment
                    charge.ShipmentSource = shipment.ShipmentType?.ToUpper();
                    charge.RealCost = true;
                    listCharge.Add(charge);
                }
            }
            else
            {
                var charge = new FormPLsheetReport();
                charge.COSTING = "COSTING";
                charge.TransID = shipment.JobNo?.ToUpper(); //JobNo of shipment
                charge.TransDate = _transDate;
                charge.MAWB = shipment.Mawb?.ToUpper(); //MasterBill of shipment
                charge.ShipmentType = _shipmentType;
                charge.Nominated = true;
                charge.POL = _polFull?.ToUpper();
                charge.POD = _podFull?.ToUpper();
                charge.Commodity = commondity?.ToUpper();
                charge.Carrier = supplier?.PartnerNameEn?.ToUpper();
                charge.Agent = agent?.PartnerNameEn?.ToUpper();
                charge.ContainerNo = _containerNoList?.ToUpper(); //Danh sách container của Shipment (Format: contNo/SealNo)
                charge.OceanVessel = shipment.FlightVesselName?.ToUpper(); //Tên chuyến bay
                charge.LocalVessel = shipment.FlightVesselName?.ToUpper(); //Tên chuyến bay
                charge.FlightNo = shipment.VoyNo?.ToUpper(); //Mã chuyến bay
                charge.SeaImpVoy = string.Empty;
                charge.LoadingDate = _etdDate; //ETD
                charge.ArrivalDate = _etaDate; //ETA
                charge.LastRevised = _dateNow;
                charge.SalesCurr = currency;
                charge.GW = _grossWeight;//Đang lấy GrossWeight của Shipment
                charge.MCW = _netWeight;//Đang lấy NetWeight của Shipment
                charge.HCW = _chargeWeight;//Đang lấy ChargeWeight của Shipment
                charge.ShipmentSource = shipment.ShipmentType?.ToUpper();
                charge.RealCost = true;
                listCharge.Add(charge);
            }

            var parameter = new FormPLsheetReportParameter();
            parameter.Contact = _currentUser;//Get user name login
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.CurrDecimalNo = 2;
            parameter.DecimalNo = 2;
            parameter.HBLList = _hblNoList?.ToUpper();

            result = new Crystal
            {
                ReportName = "FormPLsheet.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            string folderDownloadReport = CrystalEx.GetFolderDownloadReports();
            var _pathReportGenerate = folderDownloadReport + "\\PLSheet" + DateTime.Now.ToString("ddMMyyHHssmm") + ".pdf";
            result.PathReportGenerate = _pathReportGenerate;

            result.AddDataSource(listCharge);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public string GetShipmentTypeForPreviewPL(string transactionType)
        {
            string shipmentType = string.Empty;
            if (transactionType == TermData.InlandTrucking)
            {
                shipmentType = "Inland Trucking ";
            }
            if (transactionType == TermData.AirExport)
            {
                shipmentType = "Export (Air) ";
            }
            if (transactionType == TermData.AirImport)
            {
                shipmentType = "Import (Air) ";
            }
            if (transactionType == TermData.SeaConsolExport)
            {
                shipmentType = "Export (Sea Consol) ";
            }
            if (transactionType == TermData.SeaConsolImport)
            {
                shipmentType = "Import (Sea Consol) ";
            }
            if (transactionType == TermData.SeaFCLExport)
            {
                shipmentType = "Export (Sea FCL) ";
            }
            if (transactionType == TermData.SeaFCLImport)
            {
                shipmentType = "Import (Sea FCL) ";
            }
            if (transactionType == TermData.SeaLCLExport)
            {
                shipmentType = "Export (Sea LCL) ";
            }
            if (transactionType == TermData.SeaLCLImport)
            {
                shipmentType = "Import (Sea LCL) ";
            }
            return shipmentType;
        }
        #endregion -- PREVIEW --

        public ResultHandle SyncHouseBills(Guid JobId, CsTransactionSyncHBLCriteria model)
        {
            try
            {
                var shipment = DataContext.Get(x => x.Id == JobId).FirstOrDefault();
                if (shipment == null) return null;

                if (shipment.TransactionType != DocumentConstants.AE_SHIPMENT && shipment.TransactionType != DocumentConstants.AI_SHIPMENT)
                    return null;
                // Lấy ds HBL
                var housebills = csTransactionDetailRepo.Get(x => x.JobId == JobId).ToList();

                if (housebills.Count() == 0)
                {
                    return new ResultHandle { Status = false, Message = "Not found housebill", Data = null };
                }
                else
                {
                    foreach (var hbl in housebills)
                    {
                        hbl.FlightNo = model.FlightVesselName;
                        hbl.UserModified = currentUser.UserID;
                        hbl.Eta = model.Eta;
                        hbl.Etd = model.Etd;
                        hbl.Pod = model.Pod;
                        hbl.Pol = model.Pol;
                        hbl.IssuedBy = model.IssuedBy;
                        hbl.FlightDate = model.FlightDate;
                        hbl.ForwardingAgentId = model.AgentId;
                        hbl.WarehouseNotice = model.WarehouseId;
                        hbl.Route = model.Route;

                        string agentDescription = catPartnerRepo.Get(c => c.Id == model.AgentId).Select(s => s.PartnerNameEn + "\r\n" + s.AddressEn + "\r\nTel No: " + s.Tel + "\r\nFax No: " + s.Fax).FirstOrDefault();
                        hbl.ForwardingAgentDescription = agentDescription;

                        csTransactionDetailRepo.Update(hbl, x => x.Id == hbl.Id);
                    }

                    return new ResultHandle { Status = true, Message = "Sync House Bill " + String.Join(", ", housebills.Select(s => s.Hwbno).Distinct()) + " successfully!", Data = housebills.Select(s => s.Hwbno).Distinct() };
                }
            }
            catch (Exception ex)
            {
                var result = new HandleState(ex.Message);
                return new ResultHandle { Data = new object { }, Message = ex.Message, Status = true };
            }
        }
    }
}


