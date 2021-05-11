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
using System.Linq.Expressions;

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
        readonly IContextBase<SysUserLevel> userlevelRepository;
        private readonly IContextBase<AcctAdvanceRequest> accAdvanceRequestRepository;
        private readonly IContextBase<AcctAdvancePayment> accAdvancePaymentRepository;
        private readonly ICsShipmentOtherChargeService shipmentOtherChargeService;
        private IContextBase<CsShippingInstruction> shippingInstructionServiceRepo;

        private decimal _decimalNumber = Constants.DecimalNumber;
        private decimal _decimalMinNumber = Constants.DecimalMinNumber;

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
            IContextBase<SysUserLevel> userlevelRepo,
            IContextBase<AcctAdvanceRequest> accAdvanceRequestRepo,
            IContextBase<AcctAdvancePayment> accAdvancePaymentRepo,
            ICsShipmentOtherChargeService otherChargeService,
            IContextBase<CsShippingInstruction> shippingInstruction,
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
            dimensionDetailRepository = dimensionDetailRepo;
            permissionService = perService;
            freighchargesRepository = freighchargesRepo;
            currencyExchangeService = currencyExchange;
            sysOfficeRepository = sysOfficeRepo;
            airwaybillRepository = airwaybillRepo;
            groupRepository = groupRepo;
            commodityRepository = commodityRepo;
            userlevelRepository = userlevelRepo;
            accAdvanceRequestRepository = accAdvanceRequestRepo;
            accAdvancePaymentRepository = accAdvancePaymentRepo;
            shipmentOtherChargeService = otherChargeService;
            dimensionDetailService = dimensionService;
            shippingInstructionServiceRepo = shippingInstruction;
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
            SysOffice office = null;
            var currentUserOffice = currentUser?.OfficeID ?? null;
            if (currentUserOffice != null)
            {
                office = sysOfficeRepository.Get(x => x.Id == currentUserOffice).FirstOrDefault();
                shipment = SetPrefixJobIdByOfficeCode(office?.Code);
            }
            var currentShipment = GetTransactionToGenerateJobNo(office, transactionType);

            int countNumberJob = 0;
            switch (typeEnum)
            {
                case TransactionTypeEnum.InlandTrucking:
                    shipment += DocumentConstants.IT_SHIPMENT;
                    break;
                case TransactionTypeEnum.AirExport:
                    shipment += DocumentConstants.AE_SHIPMENT;
                    break;
                case TransactionTypeEnum.AirImport:
                    shipment += DocumentConstants.AI_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaConsolExport:
                    shipment += DocumentConstants.SEC_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaConsolImport:
                    shipment += DocumentConstants.SIC_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    shipment += DocumentConstants.SEF_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    shipment += DocumentConstants.SIF_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    shipment += DocumentConstants.SEL_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    shipment += DocumentConstants.SIL_SHIPMENT;
                    break;
                default:
                    break;
            }

            if (currentShipment != null)
            {
                countNumberJob = Convert.ToInt32(currentShipment.JobNo.Substring(shipment.Length + 5, 5));
            }
            return GenerateID.GenerateJobID(shipment, countNumberJob);
        }

        private CsTransaction GetTransactionToGenerateJobNo(SysOffice office, string transactionType)
        {
            CsTransaction currentShipment = null;
            if (office != null)
            {
                if (office.Code == "ITLHAN")
                {
                    currentShipment = DataContext.Get(x => x.TransactionType == transactionType
                                                    && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                    && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                    && x.JobNo.StartsWith("H") && !x.JobNo.StartsWith("HAN-"))
                                                    .OrderByDescending(x => x.JobNo)
                                                    .FirstOrDefault(); //CR: HAN -> H [15202]
                }
                else if (office.Code == "ITLDAD")
                {
                    currentShipment = DataContext.Get(x => x.TransactionType == transactionType
                                                        && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                        && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                        && x.JobNo.StartsWith("D") && !x.JobNo.StartsWith("DAD-"))
                                                    .OrderByDescending(x => x.JobNo)
                                                    .FirstOrDefault(); //CR: DAD -> D [15202]
                }
                else
                {
                    currentShipment = DataContext.Get(x => x.TransactionType == transactionType
                                                        && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                        && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                        && !x.JobNo.StartsWith("D") && !x.JobNo.StartsWith("DAD-")
                                                        && !x.JobNo.StartsWith("H") && !x.JobNo.StartsWith("HAN-"))
                                                    .OrderByDescending(x => x.JobNo)
                                                    .FirstOrDefault();
                }
            }
            else
            {
                currentShipment = DataContext.Get(x => x.TransactionType == transactionType
                                                    && x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                    && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                    && !x.JobNo.StartsWith("D") && !x.JobNo.StartsWith("DAD-")
                                                    && !x.JobNo.StartsWith("H") && !x.JobNo.StartsWith("HAN-"))
                                                    .OrderByDescending(x => x.JobNo)
                                                    .FirstOrDefault();
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
                    transaction.JobNo = CreateJobNoByTransactionType(model.TransactionTypeEnum, model.TransactionType);
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
            transaction.TransactionType = job.TransactionType;
            transaction.JobNo = job.JobNo; //JobNo is unique

            if (transaction.IsLocked.HasValue)
            {
                if (transaction.IsLocked == true)
                {
                    transaction.LockedDate = DateTime.Now;
                }
            }
            try
            {
                var hsTrans = DataContext.Update(transaction, x => x.Id == transaction.Id, false);
                if (hsTrans.Success)
                {
                    var airwaybill = airwaybillRepository.Get(x => x.JobId == model.Id)?.FirstOrDefault();

                    if (airwaybill != null)
                    {
                        csTransactionSyncAirWayBill modelSyncAirWayBill = new csTransactionSyncAirWayBill
                        {
                            Etd = model.Etd,
                            Eta = model.Eta,
                            Pol = model.Pol,
                            Pod = model.Pod,
                            FlightNo = model.FlightVesselName,
                            IssuedBy = model.IssuedBy,
                            WarehouseId = model.WarehouseId,
                            ChargeWeight = model.ChargeWeight,
                            GrossWeight = model.GrossWeight,
                            Hw = model.Hw,
                            DimensionDetails = model.DimensionDetails,
                            FlightDate = model.FlightDate,
                            Mawb = model.Mawb,
                            Cbm = model.Cbm,
                            PackageQty = model.PackageQty

                        };
                        HandleState hsAirWayBill = UpdateCsAirWayBill(airwaybill, modelSyncAirWayBill);
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

                    //Cập nhật JobNo, Mbl, Hbl cho các charge của các housebill thuộc job
                    var houseBills = csTransactionDetailRepo.Get(x => x.JobId == transaction.Id);
                    foreach (var houseBill in houseBills)
                    {
                        var modelHouse = mapper.Map<CsTransactionDetailModel>(houseBill);
                        var hsSurcharge = transactionDetailService.UpdateSurchargeOfHousebill(modelHouse);
                    }

                    // Update MBL Advance

                    IQueryable<AcctAdvanceRequest> advR = accAdvanceRequestRepository.Where(x => x.JobId == transaction.JobNo);
                    if (advR != null && advR.Count() > 0)
                    {
                        foreach (var item in advR)
                        {
                            item.Mbl = transaction.Mawb;
                            item.DatetimeModified = DateTime.Now;
                            item.UserModified = currentUser.UserID;

                            accAdvanceRequestRepository.Update(item, x => x.Id == item.Id, false);
                        }
                    }

                    hsTrans = DataContext.SubmitChanges();

                    if (hsTrans.Success)
                    {
                        accAdvanceRequestRepository.SubmitChanges();
                    }
                }

                return hsTrans;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_Update_CsTransaction_Log", ex.ToString());
                return new HandleState(ex.Message);
            }
        }

        private HandleState UpdateCsAirWayBill(CsAirWayBill airwaybill, csTransactionSyncAirWayBill model)
        {
            HandleState result = new HandleState();

            if (airwaybill != null)
            {
                airwaybill.IssuedBy = model.IssuedBy;
                airwaybill.Pol = model.Pol;
                airwaybill.Pod = model.Pod;
                airwaybill.Eta = model.Eta;
                airwaybill.Etd = model.Etd;
                airwaybill.FlightNo = model.FlightNo;
                airwaybill.FlightDate = model.FlightDate;
                airwaybill.WarehouseId = model.WarehouseId;
                airwaybill.ChargeWeight = model.ChargeWeight;
                airwaybill.GrossWeight = model.GrossWeight;
                airwaybill.Hw = model.Hw;
                airwaybill.PackageQty = model.PackageQty;

                airwaybill.DatetimeModified = DateTime.Now;
                airwaybill.UserModified = currentUser.UserID;

                result = airwaybillRepository.Update(airwaybill, x => x.Id == airwaybill.Id, false);
                if (model.DimensionDetails != null)
                {

                    List<CsDimensionDetailModel> dimCsAirWaybill = model.DimensionDetails;
                    foreach (var item in dimCsAirWaybill)
                    {
                        item.Mblid = null;
                    }
                    HandleState dimAirWayBill = dimensionDetailService.UpdateAirWayBill(dimCsAirWaybill, airwaybill.Id);
                }
                airwaybillRepository.SubmitChanges();
                return result;
            }
            return result;

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
                            || !string.IsNullOrEmpty(surcharge.AdvanceNo)
                            || !string.IsNullOrEmpty(surcharge.VoucherId)
                            || !string.IsNullOrEmpty(surcharge.SyncedFrom)
                            || surcharge.AcctManagementId != null
                         select detail);
            var data = DataContext.Get(x => x.Id == jobId).FirstOrDefault();

            if (query.Any() || accAdvanceRequestRepository.Any(x => x.JobId == data.JobNo))
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

                    if (hs.Success)
                    {
                        var houseBills = csTransactionDetailRepo.Get(x => x.JobId == jobId);
                        foreach (var houseBill in houseBills)
                        {
                            //Xóa Job >> Xóa tất cả các phí của Job [Andy - 15611 - 06/04/2021]
                            var surcharges = csShipmentSurchargeRepo.Get(x => x.Hblid == houseBill.Id);
                            foreach (var surcharge in surcharges)
                            {
                                csShipmentSurchargeRepo.Delete(x => x.Id == surcharge.Id);
                            }
                        }
                    }
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
                    if (coloaderPartner != null)
                    {
                        result.SupplierName = coloaderPartner.ShortName;
                        result.ColoaderCode = coloaderPartner.CoLoaderCode;
                        result.RoundUpMethod = coloaderPartner.RoundUpMethod;
                        result.ApplyDim = coloaderPartner.ApplyDim;
                    }
                }
                if (result.AgentId != null)
                {
                    CatPartner agent = catPartnerRepo.Get().FirstOrDefault(x => x.Id == result.AgentId);
                    if (agent != null)
                    {
                        result.AgentName = agent.ShortName;
                        result.AgentData = GetAgent(agent);
                    }
                }

                if (result.Pod != null)
                {
                    CatPlace portIndexPod = catPlaceRepo.Get(x => x.Id == result.Pod)?.FirstOrDefault();
                    result.PODName = portIndexPod.NameEn;
                    result.PODCode = portIndexPod.Code;

                    if (portIndexPod.WarehouseId != null)
                    {
                        CatPlace warehouse = catPlaceRepo.Get(x => x.Id == portIndexPod.WarehouseId)?.FirstOrDefault();
                        if (warehouse != null)
                        {
                            result.WarehousePOD = new WarehouseData
                            {
                                NameEn = warehouse.NameEn,
                                NameVn = warehouse.NameVn,
                                NameAbbr = warehouse.DisplayName,
                            };
                        }

                    }
                }

                if (result.Pol != null)
                {
                    CatPlace portIndexPol = catPlaceRepo.Get(x => x.Id == result.Pol)?.FirstOrDefault();
                    result.POLCode = portIndexPol.Code;
                    result.POLName = portIndexPol.NameEn;

                    if (portIndexPol.CountryId != null)
                    {
                        CatCountry country = catCountryRepo.Get(c => c.Id == portIndexPol.CountryId)?.FirstOrDefault();
                        if (country != null)
                        {
                            result.POLCountryCode = country.Code;
                            result.POLCountryNameEn = country.NameEn;
                            result.POLCountryNameVn = country.NameVn;
                        }
                    }

                    if (portIndexPol.WarehouseId != null)
                    {
                        CatPlace warehouse = catPlaceRepo.Get(x => x.Id == portIndexPol.WarehouseId)?.FirstOrDefault();
                        if (warehouse != null)
                        {
                            result.WarehousePOL = new WarehouseData
                            {
                                NameEn = warehouse.NameEn,
                                NameVn = warehouse.NameVn,
                                NameAbbr = warehouse.DisplayName,
                            };
                        }
                    }
                }

                if (result.DeliveryPlace != null) result.PlaceDeliveryName = catPlaceRepo.Get(x => x.Id == result.DeliveryPlace)?.FirstOrDefault().NameEn;
                result.UserNameCreated = sysUserRepo.Get(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                result.UserNameModified = sysUserRepo.Get(x => x.Id == result.UserModified).FirstOrDefault()?.Username;

                if (result.OfficeId != null)
                {
                    result.CreatorOffice = GetOfficeOfCreator(result.OfficeId);
                }
                if (result.GroupId != null)
                {
                    var group = groupRepository.Get(x => x.Id == result.GroupId).FirstOrDefault();
                    result.GroupEmail = group != null ? group.Email : string.Empty;
                }
                var airwayBill = airwaybillRepository.Get(x => x.JobId == id).FirstOrDefault();
                result.MawbShipper = airwayBill?.ShipperDescription;

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
            if (office == null) return new OfficeData();
            var creatorOffice = new OfficeData
            {
                NameEn = office.BranchNameEn,
                NameVn = office.BranchNameVn,
                Location = office.Location,
                AddressEn = office.AddressEn,
                Tel = office.Tel,
                Fax = office.Fax,
                Email = office.Email
            };
            return creatorOffice;
        }

        public int CheckDetailPermission(Guid id)
        {
            var detail = GetById(id);
            var lstGroups = userlevelRepository.Get(x => x.GroupId == currentUser.GroupId).Select(t => t.UserId).ToList();
            var lstDepartments = userlevelRepository.Get(x => x.DepartmentId == currentUser.DepartmentId).Select(t => t.UserId).ToList();

            var SalemansIds = csTransactionDetailRepo.Get(x => x.JobId == id).Select(t => t.SaleManId).ToArray();
            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(detail.TransactionType, currentUser);
            var permissionRange = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Detail);
            int code = GetPermissionToUpdate(new ModelUpdate { PersonInCharge = detail.PersonIncharge, SalemanIds = SalemansIds, UserCreated = detail.UserCreated, CompanyId = detail.CompanyId, OfficeId = detail.OfficeId, DepartmentId = detail.DepartmentId, GroupId = detail.GroupId, Groups = lstGroups, Departments = lstDepartments }, permissionRange, detail.TransactionType);
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

        /// <summary>
        /// Get air/sea information when link from ops
        /// </summary>
        /// <param name="mblNo">HBL No's ops</param>
        /// <param name="hblNo">HBL No's ops</param>
        /// <param name="serviceName">product service</param>
        /// <param name="serviceMode">service mode</param>
        /// <returns></returns>
        public LinkAirSeaInfoModel GetLinkASInfomation(string mblNo, string hblNo, string serviceName, string serviceMode)
        {
            string jobNo = null;
            string hblid = null;
            string jobId = null;
            decimal? cw = null;
            decimal? gw = null;
            decimal? pkgQty = null;

            string shipmentType = GetServiceType(serviceName, serviceMode);
            if (!string.IsNullOrEmpty(shipmentType))
            {
                var houseDetail = string.IsNullOrEmpty(hblNo) ? null : csTransactionDetailRepo.Get(x => x.Hwbno == hblNo);
                var transaction = houseDetail != null ?
                    transactionRepository
                    .Get(x => x.TransactionType == shipmentType)
                    .Join(houseDetail, x => x.Id, y => y.JobId, (x, y) => new { x.JobNo, jobId = x.Id, y.Id, x.Mawb, x.BookingNo, y.GrossWeight,y.ChargeWeight,y.PackageQty })
                    : null;

                if (transaction?.Count() == 1)
                {
                    jobNo = transaction.FirstOrDefault()?.JobNo.ToString();
                    jobId = transaction.FirstOrDefault()?.jobId.ToString();
                    hblid = transaction.FirstOrDefault().Id.ToString();
                }
                else
                {
                    if (transaction?.Count() > 1) // Có nhiều hbl
                    {
                        var masDetail = transaction == null ? null : transaction.Where(x => x.Mawb == mblNo).FirstOrDefault();
                        if (masDetail == null) // Tìm theo BookingNo
                        {
                            masDetail = transaction.Where(x => x.BookingNo == mblNo).FirstOrDefault();
                            masDetail = masDetail == null ? transaction?.FirstOrDefault() : masDetail;
                        }
                        jobNo = masDetail?.JobNo.ToString();
                        jobId = masDetail?.jobId.ToString();
                        hblid = null;
                    }
                    else // không có hbl nào -> tìm theo mawb
                    {
                        var masDetail = transactionRepository.Get(x => x.TransactionType == shipmentType && x.Mawb == mblNo).FirstOrDefault();
                        if (masDetail == null)
                        {
                            masDetail = transactionRepository.Get(x => x.TransactionType == shipmentType && x.BookingNo == mblNo).FirstOrDefault();
                        }
                        jobNo = masDetail?.JobNo.ToString();
                        jobId = masDetail?.Id.ToString();
                        hblid = null;
                    }
                }
            }
            IQueryable<CsTransactionDetail> hbls = Enumerable.Empty<CsTransactionDetail>().AsQueryable();
            List<CsMawbcontainer> containers = new List<CsMawbcontainer>();

            if (!string.IsNullOrEmpty(jobId))
            {
                if (!string.IsNullOrEmpty(hblid))
                {
                    hbls = csTransactionDetailRepo.Get(x => x.Id.ToString() == hblid);

                    containers = csMawbcontainerRepo.Get(x => x.Hblid.ToString() == hblid).ToList();
                }
                else
                {
                    hbls = csTransactionDetailRepo.Get(x => x.JobId.ToString() == jobId);

                    containers = csMawbcontainerRepo.Get(x => x.Mblid.ToString() == jobId).ToList();
                }

                if (hbls != null && hbls.Count() > 0)
                {
                    gw = hbls.Sum(x => x.GrossWeight);
                    cw = hbls.Sum(x => x.GrossWeight);
                    pkgQty = hbls.Sum(x => x.PackageQty);
                }
            }
            return new LinkAirSeaInfoModel
            {
                JobNo = jobNo,
                HblId = hblid,
                JobId = jobId,
                GW = gw,
                CW = cw,
                PackageQty = pkgQty,
                Containers = containers
            };
        }

        private string GetServiceType(string serviceName, string serviceMode)
        {
            var type = string.Empty;
            switch (serviceName)
            {
                case "Sea":
                    if (serviceMode == "Import")
                    {
                        type = TermData.SeaConsolImport;
                    }
                    else
                    {
                        type = TermData.SeaConsolExport;
                    }
                    break;
                case "Sea FCL":
                    if (serviceMode == "Import")
                    {
                        type = TermData.SeaFCLImport;
                    }
                    else
                    {
                        type = TermData.SeaFCLExport;
                    }
                    break;
                case "Sea LCL":
                    if (serviceMode == "Import")
                    {
                        type = TermData.SeaLCLImport;
                    }
                    else
                    {
                        type = TermData.SeaLCLExport;
                    }
                    break;
                case "Air":
                    if (serviceMode == "Import")
                    {
                        type = TermData.AirImport;
                    }
                    else
                    {
                        type = TermData.AirExport;
                    }
                    break;
            }
            return type;
        }
        #endregion -- DETAILS --

        #region -- LIST & PAGING --       

        private IQueryable<CsTransactionModel> GetTransaction(string transactionType, CsTransactionCriteria criteria)
        {
            ICurrentUser _user = PermissionEx.GetUserMenuPermissionTransaction(transactionType, currentUser);

            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);

            //Nếu không có điều kiện search thì load 3 tháng kể từ ngày modified mới nhất
            var queryDefault = ExpressionQueryDefault(transactionType, criteria);

            var masterBills = DataContext.Get(x => x.TransactionType == transactionType && x.CurrentStatus != TermData.Canceled).Where(queryDefault);
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
                    var jobsInHouseLevOwn = csTransactionDetailRepo.Get(y => y.SaleManId == currentUser.UserID).Select(s => s.JobId).ToList();
                    masterBills = masterBills.Where(x => x.PersonIncharge == currentUser.UserID
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID //|| csTransactionDetailRepo.Any(y => y.SaleManId == currentUser.UserID && y.JobId.Equals(x.Id))
                                                || jobsInHouseLevOwn.Any(jobId => jobId == x.Id)
                                                );

                    break;
                case PermissionRange.Group:
                    var dataUserLevel = userlevelRepository.Get(x => x.GroupId == currentUser.GroupId).Select(t => t.UserId).ToList();
                    var jobsInHouseLevGrp = csTransactionDetailRepo.Get(y => y.SaleManId == currentUser.UserID || dataUserLevel.Contains(y.SaleManId)).Select(s => s.JobId).ToList();
                    masterBills = masterBills.Where(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID //|| csTransactionDetailRepo.Any(y => y.SaleManId == currentUser.UserID && y.JobId.Equals(x.Id))
                                                //|| csTransactionDetailRepo.Any(t => t.JobId.Equals(x.Id) && dataUserLevel.Contains(t.SaleManId))
                                                || jobsInHouseLevGrp.Any(jobId => jobId == x.Id)
                                                );
                    break;
                case PermissionRange.Department:
                    var dataUserLevelDepartment = userlevelRepository.Get(x => x.DepartmentId == currentUser.DepartmentId).Select(t => t.UserId).ToList();
                    var jobsInHouseLevDept = csTransactionDetailRepo.Get(y => y.SaleManId == currentUser.UserID || dataUserLevelDepartment.Contains(y.SaleManId)).Select(s => s.JobId).ToList();
                    masterBills = masterBills.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID //|| csTransactionDetailRepo.Any(y => y.SaleManId == currentUser.UserID && y.JobId.Equals(x.Id))
                                                //|| csTransactionDetailRepo.Any(t => t.JobId.Equals(x.Id) && dataUserLevelDepartment.Contains(t.SaleManId))
                                                || jobsInHouseLevDept.Any(jobId => jobId == x.Id)
                                                );
                    break;
                case PermissionRange.Office:
                    var jobsInHouseLevOfi = csTransactionDetailRepo.Get(y => y.SaleManId == currentUser.UserID).Select(s => s.JobId).ToList();
                    masterBills = masterBills.Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID //|| csTransactionDetailRepo.Any(y => y.SaleManId == currentUser.UserID && y.JobId.Equals(x.Id))
                                                || jobsInHouseLevOfi.Any(jobId => jobId == x.Id)
                                                );
                    break;
                case PermissionRange.Company:
                    var jobsInHouseLevCom = csTransactionDetailRepo.Get(y => y.SaleManId == currentUser.UserID).Select(s => s.JobId).ToList();
                    masterBills = masterBills.Where(x => x.CompanyId == currentUser.CompanyID
                                                || authorizeUserIds.Contains(x.PersonIncharge)
                                                || x.UserCreated == currentUser.UserID //|| csTransactionDetailRepo.Any(y => y.SaleManId == currentUser.UserID && y.JobId.Equals(x.Id))
                                                || jobsInHouseLevCom.Any(jobId => jobId == x.Id)
                                                );
                    break;
            }
            if (masterBills == null)
                return null;

            //var coloaders = catPartnerRepo.Get(x => x.PartnerGroup.Contains("CARRIER"));
            //var agents = catPartnerRepo.Get(x => x.PartnerGroup.Contains("AGENT"));
            //var pols = catPlaceRepo.Get(x => x.PlaceTypeId == "Port");
            //var pods = catPlaceRepo.Get(x => x.PlaceTypeId == "Port");
            //var creators = sysUserRepo.Get();
            IQueryable<CsTransactionModel> query = null;

            query = from masterBill in masterBills
                    //join coloader in coloaders on masterBill.ColoaderId equals coloader.Id into coloader2
                    //from coloader in coloader2.DefaultIfEmpty()
                    //join agent in agents on masterBill.AgentId equals agent.Id into agent2
                    //from agent in agent2.DefaultIfEmpty()
                    //join pod in pods on masterBill.Pod equals pod.Id into pod2
                    //from pod in pod2.DefaultIfEmpty()
                    //join pol in pols on masterBill.Pol equals pol.Id into pol2
                    //from pol in pol2.DefaultIfEmpty()
                    //join creator in creators on masterBill.UserCreated equals creator.Id into creator2
                    //from creator in creator2.DefaultIfEmpty()
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
                        //SupplierName = coloader.ShortName,
                        //AgentName = agent.ShortName,
                        //PODName = pod.NameEn,
                        //POLName = pol.NameEn,
                        //CreatorName = creator.Username,
                        PackageQty = masterBill.PackageQty,
                        BookingNo = masterBill.BookingNo
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
                results = TakeShipments(tempList).ToList();
                results.ForEach(fe =>
                {
                    fe.SumCont = csMawbcontainerRepo.Get(x => x.Mblid == fe.Id).Sum(s => s.Quantity);
                    fe.SumPackage = csMawbcontainerRepo.Get(x => x.Mblid == fe.Id).Sum(s => s.PackageQuantity);
                });
            }
            return results;
        }

        /// <summary>
        /// Nếu không có điều kiện search thì load list Job 3 tháng kể từ ngày modified mới nhất trở về trước
        /// </summary>
        /// <returns></returns>
        private Expression<Func<CsTransaction, bool>> ExpressionQueryDefault(string transactionType, CsTransactionCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> query = q => true;
            if (string.IsNullOrEmpty(criteria.All) && string.IsNullOrEmpty(criteria.JobNo)
                && string.IsNullOrEmpty(criteria.MAWB) && string.IsNullOrEmpty(criteria.HWBNo) 
                && string.IsNullOrEmpty(criteria.CustomerId) && string.IsNullOrEmpty(criteria.SaleManId)
                && string.IsNullOrEmpty(criteria.SealNo) && string.IsNullOrEmpty(criteria.ContainerNo)
                && criteria.FromDate == null && criteria.ToDate == null
                && string.IsNullOrEmpty(criteria.MarkNo) && string.IsNullOrEmpty(criteria.CreditDebitNo)
                && string.IsNullOrEmpty(criteria.SoaNo) && string.IsNullOrEmpty(criteria.ColoaderId) 
                && string.IsNullOrEmpty(criteria.AgentId) && string.IsNullOrEmpty(criteria.BookingNo)
                && string.IsNullOrEmpty(criteria.UserCreated)
                && criteria.FromServiceDate == null && criteria.ToServiceDate == null)
            {
                var maxDate = (DataContext.Get(x => x.TransactionType == transactionType).Max(x => x.DatetimeModified) ?? DateTime.Now).AddDays(1).Date;
                var minDate = maxDate.AddMonths(-3).AddDays(-1).Date; //Bắt đầu từ ngày MaxDate trở về trước 3 tháng
                query = query.And(x => x.DatetimeModified.Value > minDate && x.DatetimeModified.Value < maxDate);
            }
            return query;
        }

        public IQueryable<CsTransactionModel> TakeShipments(IQueryable<CsTransactionModel> masterBills)
        {            
            var coloaders = catPartnerRepo.Get(x => x.PartnerGroup.Contains("CARRIER"));
            var agents = catPartnerRepo.Get(x => x.PartnerGroup.Contains("AGENT"));
            var pols = catPlaceRepo.Get(x => x.PlaceTypeId == "Port");
            var pods = catPlaceRepo.Get(x => x.PlaceTypeId == "Port");
            var creators = sysUserRepo.Get();

            masterBills.ToList().ForEach(fe => {
                fe.SupplierName = coloaders.FirstOrDefault(x => x.Id == fe.ColoaderId)?.ShortName;
                fe.AgentName = agents.FirstOrDefault(x => x.Id == fe.AgentId)?.ShortName;
                fe.PODName = pods.FirstOrDefault(x => x.Id == fe.Pod)?.NameEn;
                fe.POLName = pols.FirstOrDefault(x => x.Id == fe.Pol)?.NameEn;
                fe.CreatorName = creators.FirstOrDefault(x => x.Id == fe.UserCreated)?.Username;
            });
            
            return masterBills;
        }

        public IQueryable<CsTransactionModel> Query(CsTransactionCriteria criteria)
        {
            var transactionType = DataTypeEx.GetType(criteria.TransactionType);
            var listSearch = GetTransaction(transactionType, criteria);
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
                           (((x.Etd ?? null) >= (criteria.FromServiceDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToServiceDate ?? null)))
                        || (criteria.FromServiceDate == null && criteria.ToServiceDate == null)
                    )
                    &&
                    (
                           (((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.FromDate ?? null)) && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.ToDate ?? null)))
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
                           (((x.Etd ?? null) >= (criteria.FromServiceDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToServiceDate ?? null)))
                        || (criteria.FromServiceDate == null && criteria.ToServiceDate == null)
                    )
                    ||
                    (
                           (((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.FromDate ?? null)) && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.ToDate ?? null)))
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
                           (((x.Eta ?? null) >= (criteria.FromServiceDate ?? null)) && ((x.Eta ?? null) <= (criteria.ToServiceDate ?? null)))
                        || (criteria.FromServiceDate == null && criteria.ToServiceDate == null)
                    )
                    &&
                    (
                           (((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.FromDate ?? null)) && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.ToDate ?? null)))
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
                           (((x.Eta ?? null) >= (criteria.FromServiceDate ?? null)) && ((x.Eta ?? null) <= (criteria.ToServiceDate ?? null)))
                        || (criteria.FromServiceDate == null && criteria.ToServiceDate == null)
                    )
                    ||
                    (
                           (((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.FromDate ?? null)) && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.ToDate ?? null)))
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
            //Sử dụng lại của service Sea FCL Export
            var result = QuerySEF(criteria, listSearch);
            return result;
        }

        /// <summary>
        /// Query Sea Consol Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySIC(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch)
        {
            //Sử dụng lại của service Sea FCL Import
            var result = QuerySIF(criteria, listSearch);
            return result;
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
                    && ((x.BookingNo ?? "") == criteria.BookingNo || string.IsNullOrEmpty(criteria.BookingNo))
                    && ((x.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.Etd ?? null) >= (criteria.FromServiceDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToServiceDate ?? null)))
                        || (criteria.FromServiceDate == null && criteria.ToServiceDate == null)
                    )
                    &&
                    (
                           (((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.FromDate ?? null)) && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.ToDate ?? null)))
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
                    || ((x.BookingNo ?? "") == criteria.BookingNo || string.IsNullOrEmpty(criteria.BookingNo))
                    ||
                    (
                           (((x.Etd ?? null) >= (criteria.FromServiceDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToServiceDate ?? null)))
                        || (criteria.FromServiceDate == null && criteria.ToServiceDate == null)
                    )
                    ||
                    (
                           (((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.FromDate ?? null)) && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.ToDate ?? null)))
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
                    && ((x.BookingNo ?? "") == criteria.BookingNo || string.IsNullOrEmpty(criteria.BookingNo))
                    &&
                    (
                           (((x.Eta ?? null) >= (criteria.FromServiceDate ?? null)) && ((x.Eta ?? null) <= (criteria.ToServiceDate ?? null)))
                        || (criteria.FromServiceDate == null && criteria.ToServiceDate == null)
                    )
                    &&
                    (
                           (((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.FromDate ?? null)) && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.ToDate ?? null)))
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
                    || ((x.BookingNo ?? "") == criteria.BookingNo || string.IsNullOrEmpty(criteria.BookingNo))
                    ||
                    (
                           (((x.Eta ?? null) >= (criteria.FromServiceDate ?? null)) && ((x.Eta ?? null) <= (criteria.ToServiceDate ?? null)))
                        || (criteria.FromServiceDate == null && criteria.ToServiceDate == null)
                    )
                    ||
                    (
                           (((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) >= (criteria.FromDate ?? null)) && ((x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Date : x.DatetimeCreated) <= (criteria.ToDate ?? null)))
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
                    var exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(c.FinalExchangeRate, c.ExchangeDate, c.CurrencyId, DocumentConstants.CURRENCY_LOCAL);//currencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == c.ExchangeDate.Value.Date && x.CurrencyFromId == c.CurrencyId && x.CurrencyToId == DocumentConstants.CURRENCY_LOCAL)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    var UsdToVnd = currencyExchangeService.CurrencyExchangeRateConvert(c.FinalExchangeRate, c.ExchangeDate, DocumentConstants.CURRENCY_USD, DocumentConstants.CURRENCY_LOCAL);//currencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == c.ExchangeDate.Value.Date && x.CurrencyFromId == "USD" && x.CurrencyToId == DocumentConstants.CURRENCY_LOCAL)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
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
            if (string.IsNullOrEmpty(model.Mawb) && detailTrans.Select(x => x.Id).Count() > 0 && model.TransactionType != "SFE" && model.TransactionType != "SLE" && model.TransactionType != "SCE")
                return new ResultHandle { Status = false, Message = "This shipment did't have MBL No. You can't import or duplicate it." };

            CsTransaction transaction = GetDefaultJob(model);
            List<CsMawbcontainer> containers = new List<CsMawbcontainer>();
            List<CsDimensionDetail> dimensionDetails = new List<CsDimensionDetail>();
            List<CsShipmentSurcharge> surcharges = new List<CsShipmentSurcharge>();
            List<CsArrivalFrieghtCharge> freightCharges = new List<CsArrivalFrieghtCharge>();
            var siDetail = shippingInstructionServiceRepo.Get(x => x.JobId == model.Id).FirstOrDefault();

            try
            {
                if (model.CsMawbcontainers != null && model.CsMawbcontainers.Count() > 0)
                {
                    List<CsMawbcontainer> masterContainers = GetMasterBillcontainer(transaction.Id, model.CsMawbcontainers);
                    containers.AddRange(masterContainers);
                }
                if (model.DimensionDetails != null && model.DimensionDetails.Count() > 0)
                {
                    List<CsDimensionDetail> masterDimensionDetails = GetMasterDimensiondetails(transaction.Id, model.DimensionDetails);
                    dimensionDetails.AddRange(masterDimensionDetails);
                }

                if (detailTrans != null)
                {
                    int countDetail = csTransactionDetailRepo.Count(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                                        && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                                        && x.DatetimeCreated.Value.Day == DateTime.Now.Day);
                    string generatePrefixHouse = GenerateID.GeneratePrefixHousbillNo();

                    if (csTransactionDetailRepo.Any(x => (x.Hwbno ?? "").IndexOf(generatePrefixHouse, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        generatePrefixHouse = DocumentConstants.SEF_HBL
                            + GenerateID.GeneratePrefixHousbillNo();
                    }
                    string hawbCurrentMax = GetMaxHAWB();

                    string hawbSeaExportCurrent = string.Empty;
                    foreach (var item in detailTrans)
                    {
                        Guid oldHouseId = item.Id;
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


                        if (model.TransactionType == "SFE" || model.TransactionType == "SLE" || model.TransactionType == "SCE")
                        {
                            CatPlace pod = catPlaceRepo.Get(x => x.Id == model.Pod)?.FirstOrDefault();

                            string podCode = pod?.Code;
                            
                            if (string.IsNullOrEmpty(podCode))
                            {
                                item.Hwbno = GenerateID.GenerateHousebillNo(generatePrefixHouse, countDetail);
                            }
                            else
                            {
                                item.Hwbno = GenerateHBLNoSeaExport(podCode, hawbSeaExportCurrent);
                                hawbSeaExportCurrent = item.Hwbno;
                            }

                            if (model.Etd != null && model.Pol != null)
                            {
                                CatPlace pol = catPlaceRepo.Get(x => x.Id == model.Pol)?.FirstOrDefault();
                                string polName = pol?.NameEn;
                                string polCountryName = string.Empty;

                                CatCountry country = catCountryRepo.Get(c => c.Id == pol.CountryId)?.FirstOrDefault();
                                if (country != null)
                                {
                                    polCountryName = country.NameEn;
                                }
                                item.OnBoardStatus = SetDefaultOnboard(polName, polCountryName, model.Etd);
                                item.IssueHbldate = model.Etd;
                            }
                        }
                        else if (model.TransactionType == DocumentConstants.AE_SHIPMENT)
                        {
                     
                            if (item.Hwbno == "N/H") {
                                item.Hwbno = item.Hwbno;
                            }
                            else
                            {
                                item.Hwbno = GenerateAirHBLNo(hawbCurrentMax);
                                hawbCurrentMax = item.Hwbno;
                            }
                        }
                        else
                        {
                            item.Hwbno = GenerateID.GenerateHousebillNo(generatePrefixHouse, countDetail);
                        }
                        if (model.TransactionType == DocumentConstants.AI_SHIPMENT || model.TransactionType == DocumentConstants.AE_SHIPMENT)
                        {
                            item.Mawb = model.Mawb;
                        }
                        if (model.TransactionType == DocumentConstants.AI_SHIPMENT)
                        {
                            item.Hwbno = null;
                        }

                        item.Active = true;
                        item.UserCreated = transaction.UserCreated;
                        item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                        item.GroupId = currentUser.GroupId;
                        item.DepartmentId = currentUser.DepartmentId;
                        item.OfficeId = currentUser.OfficeID;
                        item.CompanyId = currentUser.CompanyID;

                        List<CsMawbcontainer> housebillcontainers = GetHouseBillContainers(oldHouseId, item.Id);
                        if (housebillcontainers != null) containers.AddRange(housebillcontainers);

                        List<CsDimensionDetail> housebillDimensions = GetHouseBillDimensions(oldHouseId, item.Id);
                        if (housebillDimensions != null) dimensionDetails.AddRange(housebillDimensions);

                        List<CsShipmentSurcharge> houseSurcharges = GetCharges(oldHouseId, item, transaction);
                        if (houseSurcharges != null) surcharges.AddRange(houseSurcharges);

                        List<CsArrivalFrieghtCharge> houseFreigcharges = GetFreightCharges(oldHouseId, item.Id);
                        if (houseFreigcharges != null) freightCharges.AddRange(houseFreigcharges);

                        countDetail = countDetail + 1;
                    }
                }

                // Bill Instruction
                if(siDetail != null)
                {
                    siDetail.JobId = transaction.Id;
                    siDetail.IssuedUser = transaction.UserCreated;
                }

                HandleState hsTrans = transactionRepository.Add(transaction);
                if (hsTrans.Success)
                {
                    if (detailTrans != null && detailTrans.Count() > 0)
                    {
                        HandleState hsTransDetails = csTransactionDetailRepo.Add(detailTrans, false);
                        HandleState hs = csTransactionDetailRepo.SubmitChanges();
                    }

                    if (containers != null && containers.Count() > 0)
                    {
                        HandleState hsContainers = csMawbcontainerRepo.Add(containers, false);
                        csMawbcontainerRepo.SubmitChanges();
                    }

                    if (dimensionDetails != null && dimensionDetails.Count() > 0)
                    {
                        HandleState hsDimentions = dimensionDetailRepository.Add(dimensionDetails, false);
                        dimensionDetailRepository.SubmitChanges();
                    }

                    if (surcharges != null && surcharges.Count() > 0)
                    {
                        HandleState hsSurcharges = csShipmentSurchargeRepo.Add(surcharges, false);
                        csShipmentSurchargeRepo.SubmitChanges();
                    }

                    if (freightCharges != null && freightCharges.Count() > 0)
                    {
                        HandleState hsFreighcharges = freighchargesRepository.Add(freightCharges, false);
                        freighchargesRepository.SubmitChanges();
                    }

                    if(siDetail != null)
                    {
                        HandleState hsBillSI = shippingInstructionServiceRepo.Add(siDetail, false);
                        shippingInstructionServiceRepo.SubmitChanges();
                    }
                    if (model.TransactionType == DocumentConstants.AE_SHIPMENT)
                    {
                        var DataAirwayBilss = airwaybillRepository.Get(x => x.JobId == model.Id).FirstOrDefault();
                        if(DataAirwayBilss != null)
                        {
                            var DataDimensionDetail = dimensionDetailRepository.Get(x => x.AirWayBillId == DataAirwayBilss.Id).ToList();
                            var DataOtherCharge = shipmentOtherChargeService.Get(x => x.JobId == model.Id).ToList();
                            DataAirwayBilss.Id = Guid.NewGuid();
                            DataAirwayBilss.DatetimeModified = DateTime.Now;
                            DataAirwayBilss.UserModified = currentUser.UserID;
                            DataAirwayBilss.JobId = transaction.Id;
                            HandleState hsAirwayBilss = airwaybillRepository.Add(DataAirwayBilss);
                            if (hsAirwayBilss.Success)
                            {
                                if (DataDimensionDetail != null)
                                {
                                    DataDimensionDetail.ForEach(x =>
                                    {
                                        x.UserCreated = currentUser.UserID;
                                        x.DatetimeCreated = DateTime.Now;
                                        x.Id = Guid.NewGuid();
                                        x.AirWayBillId = DataAirwayBilss.Id;
                                    });

                                    var hsDimensions = dimensionDetailRepository.Add(DataDimensionDetail);
                                }
                                if (DataOtherCharge != null)
                                {
                                    DataOtherCharge.ForEach(x => {
                                        x.UserModified = currentUser.UserID;
                                        x.DatetimeModified = DateTime.Now;
                                        x.Id = Guid.NewGuid();
                                        x.JobId = DataAirwayBilss.JobId;
                                    });
                                    var hsOtherCharges = shipmentOtherChargeService.Add(DataOtherCharge);
                                }
                            }
                        }
                    }
                    return new ResultHandle { Status = true, Message = "Import successfully!!!", Data = transaction };
                }

                return new ResultHandle { Status = hsTrans.Success, Message = hsTrans.Message.ToString() };
            }
            catch (Exception ex)
            {

                new LogHelper("eFMS_DUPLICATE_JOB_LOG", ex.ToString());
                return new ResultHandle { Status = false, Message = ex.Message };
            }


        }

        private string SetDefaultOnboard(string polName, string country,DateTime? etd)
        {
            string value = string.Empty;
            if (etd != null && etd.HasValue)
            {
                value = string.Format("SHIPPED ON BOARD \n{0},{1}\n{2}", polName,country, etd.Value.ToString("MMM dd, yyyy"));
            }
            else
            {
                value = string.Format("SHIPPED ON BOARD \n{0},{1}", polName, country);
            }
            return value;
        }

        public string GetMaxHAWB()
        {
            var hblNos = csTransactionDetailRepo.Get(x => x.Hwbno.Contains(DocumentConstants.CODE_ITL)).ToArray()
               .OrderByDescending(o => o.DatetimeCreated)
               .ThenByDescending(o => o.Hwbno)
               .Select(s => s.Hwbno);
            int count = 0;
            List<int> oders = new List<int>();
            foreach (var hbl in hblNos)
            {
                string _hbl = hbl;
                _hbl = _hbl.Substring(DocumentConstants.CODE_ITL.Length, _hbl.Length - DocumentConstants.CODE_ITL.Length);
                Int32.TryParse(_hbl, out count);
                oders.Add(count);

            }
            int maxCurrentOder = oders.Max();
            maxCurrentOder -= 1;
            return GenerateID.GenerateHBLNo(maxCurrentOder);
        }
        public string GenerateAirHBLNo(string hawb)
        {
            string hblNo = string.Empty;

            string _hbl = hawb;
            _hbl = _hbl.Substring(DocumentConstants.CODE_ITL.Length, _hbl.Length - DocumentConstants.CODE_ITL.Length);
            int count = int.Parse(_hbl);
            //Reset về 0
            if (count == 9999)
            {
                count = 0;
            }
            hblNo = GenerateID.GenerateHBLNo(count);
            return hblNo;
        }

        public string GenerateHBLNoSeaExport(string podCode, string currentHwbNo)
        {
            if (string.IsNullOrEmpty(podCode) || podCode == "null")
            {
                return null;
            }
            string keyword = ((string.IsNullOrEmpty(podCode) || podCode == "null") ? "" : podCode) + DateTime.Now.ToString("yyMM");
            string hbl = "ITL" + keyword;

            var codes = csTransactionDetailRepo.Where(x => x.Hwbno.Contains(keyword)).Select(x => x.Hwbno).ToList();
            var oders = new List<int>();

            if (!string.IsNullOrEmpty(currentHwbNo))
            {
                codes.Add(currentHwbNo);
            }
            if (codes != null & codes.Count > 0)
            {
                foreach (var code in codes)
                {
                    // Lấy 3 ký tự cuối
                    if (code.Length > 7 && isNumeric(code.Substring(code.Length - 3)))
                    {
                        oders.Add(int.Parse(code.Substring(code.Length - 3)));
                    }
                }
                if (oders.Count() > 0)
                {
                    int maxCurrentOder = oders.Max();

                    hbl += (maxCurrentOder + 1).ToString("000");
                }
                else
                {
                    hbl += "001";
                }

            }
            else
            {
                hbl += "001";
            }

            return hbl;
        }

        private bool isNumeric(string n)
        {
            return int.TryParse(n, out int _);
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
            transaction.IsLocked = false; // allow duplicate job was locked.
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

        private List<CsShipmentSurcharge> GetCharges(Guid oldHouseId, CsTransactionDetail newHouse, CsTransaction shipment)
        {
            List<CsShipmentSurcharge> surCharges = null;
            IQueryable<CsShipmentSurcharge> charges = csShipmentSurchargeRepo.Get(x => x.Hblid == oldHouseId && x.IsFromShipment == true); // Không lấy phí hiện trường
            decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;
            if (charges.Select(x => x.Id).Count() != 0)
            {
                surCharges = new List<CsShipmentSurcharge>();
                foreach (var item in charges)
                {
                    item.Id = Guid.NewGuid();
                    item.UserCreated = currentUser.UserID;
                    item.DatetimeCreated = DateTime.Now;
                    item.Hblid = newHouse.Id;

                    item.JobNo = shipment.JobNo;
                    item.Hblno = newHouse.Hwbno;
                    item.Mblno = newHouse.Mawb;

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
                _hblNoList = String.Join(";", listHousebill.Where(x => !string.IsNullOrEmpty(x.Hwbno)).Select(x => x.Hwbno));

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
                        
                        var charge = new FormPLsheetReport();
                        charge.COSTING = "COSTING";
                        charge.TransID = shipment.JobNo?.ToUpper(); //JobNo of shipment
                        charge.TransDate = _transDate;
                        charge.HWBNO = surcharge.Hblno?.ToUpper();
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
                        charge.PayableAccount = surcharge.PartnerName?.ToUpper();//Partner name of charge
                        charge.Description = surcharge.ChargeNameEn; //Charge name of charge
                        charge.Curr = !string.IsNullOrEmpty(surcharge.CurrencyId) ? surcharge.CurrencyId.Trim() : string.Empty; //Currency of charge
                        charge.Cost = cost + _decimalNumber; //Phí chi của charge
                        charge.Revenue = revenue + _decimalNumber; //Phí thu của charge
                        charge.Exchange = 0;
                        charge.Paid = (revenue > 0 || cost < 0) && isOBH == false ? false : true;
                        charge.Docs = !string.IsNullOrEmpty(surcharge.InvoiceNo) ? surcharge.InvoiceNo : (surcharge.CreditNo ?? surcharge.DebitNo); //Ưu tiên: InvoiceNo >> CD Note Code  of charge
                        charge.Notes = surcharge.Notes;
                        charge.InputData = string.Empty; //Gán rỗng
                        charge.Quantity = surcharge.Quantity + _decimalNumber; //Cộng thêm phần thập phân
                        charge.UnitPrice = (surcharge.UnitPrice ?? 0) + _decimalMinNumber; //Cộng thêm phần thập phân nhỏ riêng trường hợp này
                        charge.UnitPriceStr = surcharge.CurrencyId == DocumentConstants.CURRENCY_LOCAL ? string.Format("{0:n0}", (surcharge.UnitPrice ?? 0)) : string.Format("{0:n3}", (surcharge.UnitPrice ?? 0));
                        charge.Unit = unitCode;
                        charge.LastRevised = _dateNow;
                        charge.OBH = isOBH;
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
                        charge.TypeOfService = surcharge.TransactionType; //NOT USE
                        charge.ShipmentSource = shipment.ShipmentType?.ToUpper();
                        charge.RealCost = true;
                        //Đối với phí OBH thì NetAmountCurr gán bằng 0
                        charge.NetAmountCurr = (surcharge.Type != DocumentConstants.CHARGE_OBH_TYPE ? currencyExchangeService.ConvertNetAmountChargeToNetAmountObj(surcharge, currency) : 0) + _decimalMinNumber; //NetAmount quy đổi về currency preview
                        charge.GrossAmountCurr = currencyExchangeService.ConvertAmountChargeToAmountObj(surcharge, currency) + _decimalMinNumber; //GrossAmount quy đổi về currency preview

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
            parameter.DecimalNo = 0;
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

                List<CsTransactionDetail> housebills = csTransactionDetailRepo.Get(x => x.JobId == JobId).ToList();

                if (shipment.TransactionType == DocumentConstants.AE_SHIPMENT || shipment.TransactionType == DocumentConstants.AI_SHIPMENT)
                {
                    if (housebills.Count() == 0)
                    {
                        return new ResultHandle { Status = false, Message = "Not found housebill", Data = null };
                    }
                    else
                    {
                        foreach (var hbl in housebills)
                        {
                            hbl.UserModified = currentUser.UserID;
                            hbl.DatetimeModified = DateTime.Now;

                            hbl.FlightNo = model.FlightVesselName;
                            hbl.Eta = model.Eta;
                            hbl.Etd = model.Etd;
                            hbl.Pod = model.Pod;
                            hbl.Pol = model.Pol;
                            hbl.IssuedBy = model.IssuedBy;
                            hbl.FlightDate = model.FlightDate;
                            hbl.ForwardingAgentId = model.AgentId;
                            hbl.WarehouseId = string.IsNullOrEmpty(model.WarehouseId) ? hbl.WarehouseId : Guid.Parse(model.WarehouseId);
                            hbl.Route = model.Route;
                            hbl.Mawb = model.MblNo;
                            string agentDescription = catPartnerRepo.Get(c => c.Id == model.AgentId).Select(s => s.PartnerNameEn + "\r\n" + s.AddressEn + "\r\nTel No: " + s.Tel + "\r\nFax No: " + s.Fax).FirstOrDefault();
                            hbl.ForwardingAgentDescription = agentDescription;


                            // CR 14501
                            hbl.PackageQty = model.PackageQty;
                            hbl.GrossWeight = model.GrossWeight;
                            hbl.Hw = model.Hw;
                            hbl.ChargeWeight = model.ChargeWeight;

                            csTransactionDetailRepo.Update(hbl, x => x.Id == hbl.Id, false);
                        }
                        csTransactionDetailRepo.SubmitChanges();

                        return new ResultHandle { Status = true, Message = "Sync House Bill " + String.Join(", ", housebills.Select(s => s.Hwbno).Distinct()) + " successfully!", Data = housebills.Select(s => s.Hwbno).Distinct() };
                    }
                }
                else
                {
                    if (housebills.Count() == 0)
                    {
                        return new ResultHandle { Status = false, Message = "Not found housebill", Data = null };
                    }
                    else
                    {
                        foreach (var hbl in housebills)
                        {
                            hbl.UserModified = currentUser.UserID;
                            hbl.DatetimeModified = DateTime.Now;

                            if (model.Eta != null)
                            {
                                hbl.Eta = model.Eta;
                            }
                            if (model.Etd != null)
                            {
                                hbl.Etd = model.Etd;
                            }
                            if (model.Pod != null)
                            {
                                hbl.Pod = model.Pod;
                            }
                            if (model.Pol != null)
                            {
                                hbl.Pol = model.Pol;
                            }

                            hbl.CustomsBookingNo = model.BookingNo;
                            hbl.Mawb = model.MblNo;
                            hbl.OceanVoyNo = model.FlightVesselName + " - " + model.VoyNo;

                            // date of issue
                            if (shipment.TransactionType.Contains("E"))
                            {
                                hbl.IssueHbldate = model.Etd;
                                var _onBoardStatus = hbl.OnBoardStatus.Split('\n');
                                var status = string.Empty;
                                foreach (var st in _onBoardStatus)
                                {
                                    if (DateTime.TryParse(st, out DateTime result))
                                    {
                                        status += model.Etd?.ToString("MMM dd, yyyy");
                                    }
                                    else
                                    {
                                        status += (st + "\n");
                                    }
                                }
                                hbl.OnBoardStatus = status;
                            }
                            csTransactionDetailRepo.Update(hbl, x => x.Id == hbl.Id, false);
                        }
                        csTransactionDetailRepo.SubmitChanges();

                        return new ResultHandle { Status = true, Message = "Sync House Bill " + String.Join(", ", housebills.Select(s => s.Hwbno).Distinct()) + " successfully!", Data = housebills.Select(s => s.Hwbno).Distinct() };
                    }
                }
            }
            catch (Exception ex)
            {
                var result = new HandleState(ex.Message);
                return new ResultHandle { Data = new object { }, Message = ex.Message, Status = true };
            }
        }

        public HandleState SyncShipmentByAirWayBill(Guid JobId, csTransactionSyncAirWayBill model)
        {
            CsTransaction shipment = DataContext.Where(x => x.Id == JobId)?.FirstOrDefault();
            if (shipment == null)
            {
                return new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);
            }

            shipment.UserModified = currentUser.UserID;
            shipment.DatetimeModified = DateTime.Now;

            shipment.Etd = model.Etd;
            shipment.Eta = model.Eta;
            shipment.Pol = model.Pol;
            shipment.Pod = model.Pod;
            shipment.IssuedBy = model.IssuedBy;
            shipment.WarehouseId = model.WarehouseId;
            shipment.FlightDate = model.FlightDate;
            shipment.FlightVesselName = model.FlightNo;
            shipment.ChargeWeight = model.ChargeWeight;
            shipment.Hw = model.Hw;
            shipment.GrossWeight = model.GrossWeight;
            shipment.Mawb = model.Mawb;
            shipment.PackageQty = model.PackageQty;
            shipment.Cbm = model.Cbm;

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    HandleState hsTrans = DataContext.Update(shipment, x => x.Id == JobId);
                    if (hsTrans.Success)
                    {
                        if (model.DimensionDetails != null)
                        {
                            HandleState hsdimensions = dimensionDetailService.UpdateMasterBill(model.DimensionDetails, JobId);
                        }
                        else
                        {
                            HandleState hsContainerDetele = dimensionDetailService.Delete(x => x.Mblid == JobId);
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

        public HandleState LockCsTransaction(Guid jobId)
        {
            CsTransaction job = DataContext.First(x => x.Id == jobId && x.CurrentStatus != TermData.Canceled);
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
        #region preview
        public Crystal PreviewShipmentCoverPage(Guid Id)
        {
            var dataShipment = DataContext.Get(x => x.Id == Id).FirstOrDefault();
            var dataHouseBills = csTransactionDetailRepo.Get(x => x.JobId == dataShipment.Id && x.ParentId == null);
            var dataContainers = csMawbcontainerRepo.Get(x => x.Mblid == dataShipment.Id);

            var listShipment = new List<ShimentConverPageSeaReport>();
            Crystal result = null;
            //var _currentUser = currentUser.UserName;
            if (dataShipment != null)
            {
                var obj = new ShimentConverPageSeaReport();
                obj.TransID = dataShipment.JobNo;
                obj.TransDate = dataShipment.ServiceDate;
                var _containerNoList = string.Empty;
                CsMawbcontainerCriteria contCriteria = new CsMawbcontainerCriteria { Mblid = dataShipment.Id };
                var _shipmentType = GetShipmentTypeForPreviewPL(dataShipment.TransactionType);
                if (!string.IsNullOrEmpty(dataShipment.AgentId))
                {
                    obj.Agent = obj.NominationParty = catPartnerRepo.Get(x => x.Id == dataShipment.AgentId).Select(x => x.PartnerNameEn).FirstOrDefault()?.ToUpper();
                }
                obj.ShipmentSource = dataShipment.ShipmentType;
                obj.Vessel = dataShipment.FlightVesselName;
                if (!string.IsNullOrEmpty(dataShipment.ColoaderId))
                {
                    obj.Carrier = catPartnerRepo.Get(x => x.Id == dataShipment.ColoaderId).Select(x => x.PartnerNameEn).FirstOrDefault()?.ToUpper();
                }
                if (dataShipment.Pol != Guid.Empty)
                {
                    obj.POL = catPlaceRepo.Get(x => x.Id == dataShipment.Pol).Select(x => x.NameEn)?.FirstOrDefault();
                }
                obj.POD = catPlaceRepo.Get(x => x.Id == dataShipment.Pod).Select(x => x.NameEn)?.FirstOrDefault();
                obj.ETA = dataShipment.Eta;
                obj.ETD = dataShipment.Etd;
                obj.MAWB = dataShipment.Mawb;
                obj.ContainerNo = dataShipment.PackageContainer;
                obj.ShipmentType = _shipmentType;
                string salesmanName = string.Empty;
                string Container = string.Empty;
                string ConstSealNo = string.Empty;
                if (dataContainers.Any())
                {
                    foreach (var item in dataContainers)
                    {
                        ConstSealNo += item.ContainerNo + "/" + item.SealNo + " ";
                    }
                }
                if (dataHouseBills.Any())
                {
                    foreach (var item in dataHouseBills)
                    {
                        var objInHbl = new ShimentConverPageSeaReport();
                        var shipper = catPartnerRepo.Get(x => x.Id == item.ShipperId).FirstOrDefault()?.PartnerNameEn.ToUpper();
                        var consignee = catPartnerRepo.Get(x => x.Id == item.ConsigneeId).FirstOrDefault()?.PartnerNameEn.ToUpper();
                        objInHbl.HWBNO = item.Hwbno;
                        objInHbl.ATTN = shipper?.ToUpper();
                        objInHbl.Consignee = consignee?.ToUpper();
                        objInHbl.FreightTerm = item.FreightPayment;
                        objInHbl.NoPieces = item.PackageQty != null ? item.PackageQty.ToString() : string.Empty; //Số kiện (Pieces)
                        objInHbl.GW = (item.GrossWeight ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                        objInHbl.CW = (item.ChargeWeight ?? 0) + _decimalNumber; //Cộng thêm phần thập phân
                        objInHbl.DocsReleaseDate = item.DocumentDate.ToString();
                        objInHbl.TransID = obj.TransID;
                        objInHbl.TransDate = obj.TransDate;
                        objInHbl.Agent = obj.Agent;
                        objInHbl.ShipmentSource = obj.ShipmentSource;
                        objInHbl.Vessel = obj.Vessel;
                        objInHbl.Carrier = obj.Carrier;
                        objInHbl.POL = obj.POL;
                        objInHbl.POD = obj.POD;
                        objInHbl.ETA = obj.ETA;
                        objInHbl.ETD = obj.ETD;
                        objInHbl.MAWB = obj.MAWB;
                        objInHbl.ContainerNo = item.PackageContainer;
                        objInHbl.ShipmentType = obj.ShipmentType;
                        objInHbl.Prepairedby = currentUser.UserName;
                        Container += !string.IsNullOrEmpty(item.PackageContainer) ? item.PackageContainer + " & " : string.Empty;
                        salesmanName += sysUserRepo.Get(x => x.Id == item.SaleManId).Select(t => t.Username)?.FirstOrDefault() + ",";
                        listShipment.Add(objInHbl);
                    }

                }
                else
                {
                    listShipment.Add(obj);
                }
                if (salesmanName.Length > 0)
                {
                    listShipment.ForEach(x => x.ContactName = salesmanName.Remove(salesmanName.Length - 1));
                }
                if (Container.Length > 0)
                {
                    listShipment.ForEach(x => x.ContainerNo = Container.Remove(Container.Length - 2) + (ConstSealNo.Length > 0 ? ConstSealNo.Remove(ConstSealNo.Length - 1) : String.Empty));
                }
            }

            var parameter = new ShimentConverPageSeaReportParams();
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.Contact = string.Empty;//Get user name login
            parameter.DecimalNo = 0; // set 0  temporary
            parameter.HBLList = string.Empty;

            result = new Crystal
            {
                ReportName = "ShipmentCoverPage.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listShipment);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;

        }

        public int CheckUpdateMBL(CsTransactionEditModel model, out string mblNo, out List<string> advs)
        {
            mblNo = string.Empty;
            advs = new List<string>();
            int errorCode = 0;  // 1|2
            bool hasChargeSynced = false;
            bool hasAdvanceRequest = false;

            if (DataContext.Any(x => x.Id == model.Id && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && (x.Mawb ?? "").ToLower() != (model.Mawb ?? "")))
            {
                CsTransaction shipment = DataContext.Get(x => x.Id == model.Id)?.FirstOrDefault();
                if (shipment != null)
                {
                    hasChargeSynced = csShipmentSurchargeRepo.Any(x => x.JobNo == shipment.JobNo && x.Mblno == shipment.Mawb && (!string.IsNullOrEmpty(x.SyncedFrom) || !string.IsNullOrEmpty(x.PaySyncedFrom)));
                }

                if (hasChargeSynced)
                {
                    errorCode = 1;
                    mblNo = shipment.Mawb;
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
                        mblNo = shipment.Mawb;
                    }
                }
            }

            return errorCode;
        }
    }
    #endregion

}


