using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common;
using Microsoft.Extensions.Localization;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models.ReportResults;

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
            IContextBase<CsDimensionDetail> dimensionDetailRepo) : base(repository, mapper)
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
                    shipment = Constants.IT_SHIPMENT;
                    break;
                case TransactionTypeEnum.AirExport:
                    shipment = Constants.AE_SHIPMENT;
                    break;
                case TransactionTypeEnum.AirImport:
                    shipment = Constants.AI_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaConsolExport:
                    shipment = Constants.SEC_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaConsolImport:
                    shipment = Constants.SIC_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    shipment = Constants.SEF_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    shipment = Constants.SIF_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    shipment = Constants.SEL_SHIPMENT;
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    shipment = Constants.SIL_SHIPMENT;
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
            var employeeId = sysUserRepo.Get(x => x.Id == transaction.UserCreated).FirstOrDefault()?.EmployeeId;
            if (!string.IsNullOrEmpty(employeeId))
            {
                var branchOfUser = sysEmployeeRepo.Get(x => x.Id == employeeId)?.FirstOrDefault().WorkPlaceId;
                if (branchOfUser != null)
                {
                    transaction.BranchId = (Guid)branchOfUser;
                }
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hsTrans = DataContext.Add(transaction);
                    if (hsTrans.Success)
                    {
                        if(model.CsMawbcontainers != null)
                        {
                            model.CsMawbcontainers.ForEach(x => {
                                x.Id = Guid.NewGuid();
                                x.Mblid = transaction.Id;
                                x.UserModified = transaction.UserCreated;
                                x.DatetimeModified = DateTime.Now;
                            });
                            var t = containerService.Add(model.CsMawbcontainers);
                        }
                        if(model.DimensionDetails != null)
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
            if (model.CsMawbcontainers.Count > 0)
            {
                var checkDuplicateCont = containerService.ValidateContainerList(model.CsMawbcontainers, model.Id, null);
                if (checkDuplicateCont.Success == false)
                {
                    return checkDuplicateCont;
                }
            }
            var transaction = mapper.Map<CsTransaction>(model);
            transaction.DatetimeModified = DateTime.Now;
            if (transaction.IsLocked.HasValue)
            {
                if (transaction.IsLocked == true)
                {
                    transaction.LockedDate = DateTime.Now;
                }
            }
            var employeeId = sysUserRepo.Get(x => x.Id == transaction.UserCreated).FirstOrDefault()?.EmployeeId;
            if (!string.IsNullOrEmpty(employeeId))
            {
                var branchOfUser = sysEmployeeRepo.Get(x => x.Id == employeeId)?.FirstOrDefault().WorkPlaceId;
                if (branchOfUser != null)
                {
                    transaction.BranchId = (Guid)branchOfUser;
                }
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hsTrans = DataContext.Update(transaction, x => x.Id == transaction.Id);
                    if (hsTrans.Success)
                    {
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
                            var hscontainers = dimensionDetailService.UpdateMasterBill(model.DimensionDetails, transaction.Id);
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
            ChangeTrackerHelper.currentUser = currentUser.UserID;
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
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = new HandleState();
            try
            {
                var job = DataContext.First(x => x.Id == jobId && x.CurrentStatus != TermData.Canceled);
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
        public CsTransactionModel GetById(Guid id)
        {
            var data = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (data == null) return null;
            else
            {
                var result = mapper.Map<CsTransactionModel>(data);
                if (result.ColoaderId != null) result.SupplierName = catPartnerRepo.Get().FirstOrDefault(x => x.Id == result.ColoaderId)?.PartnerNameEn;
                if (result.AgentId != null) result.AgentName = catPartnerRepo.Get().FirstOrDefault(x => x.Id == result.AgentId)?.PartnerNameEn;
                if (result.Pod != null) result.PODName = catPlaceRepo.Get(x => x.Id == result.Pod)?.FirstOrDefault().NameEn;
                if (result.Pol != null) result.POLName = catPlaceRepo.Get(x => x.Id == result.Pol)?.FirstOrDefault().NameEn;
                if (result.DeliveryPlace != null) result.PlaceDeliveryName = catPlaceRepo.Get(x => x.Id == result.DeliveryPlace)?.FirstOrDefault().NameEn;
                return result;
            }
        }
        #endregion -- DETAILS --

        #region -- LIST & PAGING --
        private IQueryable<CsTransactionModel> GetTransaction(string transactionType)
        {
            var masterBills = DataContext.Get(x => x.TransactionType == transactionType && x.CurrentStatus != TermData.Canceled);

            var coloaders = catPartnerRepo.Get();
            var agents = catPartnerRepo.Get();
            var pols = catPlaceRepo.Get();
            var pods = catPlaceRepo.Get();
            var creators = sysUserRepo.Get();
            var houseBills = csTransactionDetailRepo.Get();
            IQueryable<CsTransactionModel> query = null;

            query = from masterBill in masterBills
                    join houseBill in houseBills on masterBill.Id equals houseBill.JobId into houseBill2
                    from houseBill in houseBill2.DefaultIfEmpty()
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
                        BranchId = masterBill.BranchId,
                        JobNo = masterBill.JobNo,
                        Mawb = masterBill.Mawb,
                        TypeOfService = masterBill.TypeOfService,
                        Etd = masterBill.Etd,
                        Eta = masterBill.Eta,
                        ServiceDate = masterBill.ServiceDate,
                        Mbltype = masterBill.Mbltype,
                        ColoaderId = masterBill.ColoaderId,
                        SubColoader = masterBill.SubColoader,
                        BookingNo = masterBill.BookingNo,
                        AgentId = masterBill.AgentId,
                        Pol = masterBill.Pol,
                        Pod = masterBill.Pod,
                        DeliveryPlace = masterBill.DeliveryPlace,
                        PaymentTerm = masterBill.PaymentTerm,
                        FlightVesselName = masterBill.FlightVesselName,
                        VoyNo = masterBill.VoyNo,
                        ShipmentType = masterBill.ShipmentType,
                        Commodity = masterBill.Commodity,
                        DesOfGoods = masterBill.DesOfGoods,
                        PackageContainer = masterBill.PackageContainer,
                        Pono = masterBill.Pono,
                        PersonIncharge = masterBill.PersonIncharge,
                        NetWeight = masterBill.NetWeight,
                        GrossWeight = masterBill.GrossWeight,
                        ChargeWeight = masterBill.ChargeWeight,
                        Cbm = masterBill.Cbm,
                        Notes = masterBill.Notes,
                        TransactionType = masterBill.TransactionType,
                        UserCreated = masterBill.UserCreated,
                        IsLocked = masterBill.IsLocked,
                        LockedDate = masterBill.LockedDate,
                        DatetimeCreated = masterBill.DatetimeCreated,
                        UserModified = masterBill.UserModified,
                        DatetimeModified = masterBill.DatetimeModified,
                        Active = masterBill.Active,
                        InactiveOn = masterBill.InactiveOn,
                        SupplierName = coloader.ShortName,
                        AgentName = agent.ShortName,
                        HWBNo = houseBill.Hwbno,
                        CustomerId = houseBill.CustomerId,
                        NotifyPartyId = houseBill.NotifyPartyId,
                        SaleManId = houseBill.SaleManId,
                        PODName = pod.NameEn,
                        POLName = pol.NameEn,
                        CreatorName = creator.Username,
                        HblId = houseBill.Id == Guid.Empty || false ? Guid.Empty : houseBill.Id,
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
            rowsCount = tempList.Count();
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
                    //results = QueryIT(criteria, listSearch);
                    break;
                case TransactionTypeEnum.AirExport:
                    results = QueryAE(criteria, listSearch);
                    break;
                case TransactionTypeEnum.AirImport:
                    results = QueryAI(criteria, listSearch);
                    break;
                case TransactionTypeEnum.SeaConsolExport:
                    //results = QuerySEC(criteria, listSearch);
                    break;
                case TransactionTypeEnum.SeaConsolImport:
                    //results = QuerySIC(criteria, listSearch);
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
            var containers = csMawbcontainerRepo.Get();
            var surcharges = csShipmentSurchargeRepo.Get();
            var query = (from transaction in listSearch
                         join container in containers on transaction.Id equals container.Mblid into containerTrans
                         from cont in containerTrans.DefaultIfEmpty()
                         join surcharge in surcharges on transaction.HblId equals surcharge.Hblid into surchargeTrans
                         from sur in surchargeTrans.DefaultIfEmpty()
                         select new
                         {
                             transaction,
                             sur.CreditNo,
                             sur.DebitNo,
                             sur.Soano,
                             sur.PaySoano
                         });
            if (criteria.All == null)
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    &&
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    &&
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    && ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    && ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    && ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    ||
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    ||
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    || ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    || ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    || ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            var result = query.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault()).OrderByDescending(o => o.DatetimeModified);
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
            var containers = csMawbcontainerRepo.Get();
            var surcharges = csShipmentSurchargeRepo.Get();
            var query = (from transaction in listSearch
                         join container in containers on transaction.Id equals container.Mblid into containerTrans
                         from cont in containerTrans.DefaultIfEmpty()
                         join surcharge in surcharges on transaction.HblId equals surcharge.Hblid into surchargeTrans
                         from sur in surchargeTrans.DefaultIfEmpty()
                         select new
                         {
                             transaction,
                             sur.CreditNo,
                             sur.DebitNo,
                             sur.Soano,
                             sur.PaySoano
                         });
            if (criteria.All == null)
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    &&
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    &&
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    && ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    && ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    && ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.transaction.Eta ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Eta ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    ||
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    ||
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    || ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    || ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    || ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.transaction.Eta ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Eta ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            var result = query.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault()).OrderByDescending(o => o.DatetimeModified);
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
            var containers = csMawbcontainerRepo.Get();
            var surcharges = csShipmentSurchargeRepo.Get();
            var query = (from transaction in listSearch
                         join container in containers on transaction.Id equals container.Mblid into containerTrans
                         from cont in containerTrans.DefaultIfEmpty()
                         join surcharge in surcharges on transaction.HblId equals surcharge.Hblid into surchargeTrans
                         from sur in surchargeTrans.DefaultIfEmpty()
                         select new
                         {
                             transaction,
                             cont.ContainerNo,
                             cont.SealNo,
                             cont.MarkNo,
                             sur.CreditNo,
                             sur.DebitNo,
                             sur.Soano,
                             sur.PaySoano
                         });
            if (criteria.All == null)
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    &&
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    &&
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    && ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    && ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    && ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    ||
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    ||
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    || ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    || ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    || ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            var result = query.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault()).OrderByDescending(o => o.DatetimeModified);
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
            var containers = csMawbcontainerRepo.Get();
            var surcharges = csShipmentSurchargeRepo.Get();
            var query = (from transaction in listSearch
                         join container in containers on transaction.Id equals container.Mblid into containerTrans
                         from cont in containerTrans.DefaultIfEmpty()
                         join surcharge in surcharges on transaction.HblId equals surcharge.Hblid into surchargeTrans
                         from sur in surchargeTrans.DefaultIfEmpty()
                         select new
                         {
                             transaction,
                             cont.ContainerNo,
                             cont.SealNo,
                             cont.MarkNo,
                             sur.CreditNo,
                             sur.DebitNo,
                             sur.Soano,
                             sur.PaySoano
                         });
            if (criteria.All == null)
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    &&
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    &&
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    && ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    && ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    && ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    ||
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    ||
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    || ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    || ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    || ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            var result = query.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault()).OrderByDescending(o => o.DatetimeModified);
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
            var containers = csMawbcontainerRepo.Get();
            var surcharges = csShipmentSurchargeRepo.Get();
            var query = (from transaction in listSearch
                         join container in containers on transaction.Id equals container.Mblid into containerTrans
                         from cont in containerTrans.DefaultIfEmpty()
                         join surcharge in surcharges on transaction.HblId equals surcharge.Hblid into surchargeTrans
                         from sur in surchargeTrans.DefaultIfEmpty()
                         select new
                         {
                             transaction,
                             cont.ContainerNo,
                             cont.SealNo,
                             cont.MarkNo,
                             sur.CreditNo,
                             sur.DebitNo,
                             sur.Soano,
                             sur.PaySoano
                         });
            if (criteria.All == null)
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    &&
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    &&
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    && ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    && ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    && ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    ||
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    ||
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    || ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    || ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    || ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            var result = query.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault()).OrderByDescending(o => o.DatetimeModified);
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
            var containers = csMawbcontainerRepo.Get();
            var surcharges = csShipmentSurchargeRepo.Get();
            var query = (from transaction in listSearch
                         join container in containers on transaction.Id equals container.Mblid into containerTrans
                         from cont in containerTrans.DefaultIfEmpty()
                         join surcharge in surcharges on transaction.HblId equals surcharge.Hblid into surchargeTrans
                         from sur in surchargeTrans.DefaultIfEmpty()
                         select new
                         {
                             transaction,
                             cont.ContainerNo,
                             cont.SealNo,
                             cont.MarkNo,
                             sur.CreditNo,
                             sur.DebitNo,
                             sur.Soano,
                             sur.PaySoano
                         });
            if (criteria.All == null)
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    &&
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    &&
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    && ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    && ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    && ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    && ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    && ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    &&
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            else
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.MarkNo ?? "").IndexOf(criteria.MarkNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    ||
                    (
                        (x.CreditNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.DebitNo ?? "").IndexOf(criteria.CreditDebitNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    ||
                    (
                        (x.Soano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.PaySoano ?? "").IndexOf(criteria.SoaNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    )
                    || ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    || ((x.transaction.ColoaderId ?? "") == criteria.ColoaderId || string.IsNullOrEmpty(criteria.ColoaderId))
                    || ((x.transaction.AgentId ?? "") == criteria.AgentId || string.IsNullOrEmpty(criteria.AgentId))
                    || ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    || ((x.transaction.UserCreated ?? "") == criteria.UserCreated || string.IsNullOrEmpty(criteria.UserCreated))
                    ||
                    (
                           (((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null)
                    )
                );
            }
            var result = query.Select(s => s.transaction).GroupBy(g => g.JobNo).Select(s => s.FirstOrDefault()).OrderByDescending(o => o.DatetimeModified);
            return result;
        }

        #endregion -- LIST & PAGING --

        public List<object> GetListTotalHB(Guid JobId)
        {
            List<object> returnList = new List<object>();
            var housebills = csTransactionDetailRepo.Get(x => x.JobId == JobId).ToList();
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
                    var exchangeRate = currencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == c.ExchangeDate.Value.Date && x.CurrencyFromId == c.CurrencyId && x.CurrencyToId == "VND" && x.Active == true)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    var UsdToVnd = currencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == c.ExchangeDate.Value.Date && x.CurrencyFromId == "USD" && x.CurrencyToId == "VND" && x.Active == true)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    var rate = exchangeRate == null ? 1 : exchangeRate.Rate;
                    var usdToVndRate = UsdToVnd == null ? 1 : UsdToVnd.Rate;
                    if (c.Type.ToLower() == Constants.CHARGE_BUY_TYPE.ToLower())
                    {
                        totalBuying += c.Total * rate;
                        totalBuyingUSD += (totalBuying / usdToVndRate);
                    }
                    if (c.Type.ToLower() == Constants.CHARGE_SELL_TYPE.ToLower())
                    {
                        totalSelling += c.Total * rate;
                        totalSellingUSD += (totalSelling / usdToVndRate);
                    }
                    if (c.Type.ToLower() == Constants.CHARGE_OBH_TYPE.ToLower())
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

                Guid jobId = new Guid("3c4fdc20-85b7-4d66-9b5c-adb0a08aeb20");
                var transaction = DataContext.Get(x => x.Id == jobId).FirstOrDefault();
                for (int i = 0; i < 5000; i++)
                {
                    transaction.Id = Guid.NewGuid();
                    transaction.JobNo = CreateJobNoByTransactionType(TransactionTypeEnum.SeaFCLImport, transaction.TransactionType);
                    transaction.Mawb = transaction.JobNo;
                    transaction.UserCreated = "admin";
                    transaction.DatetimeCreated = transaction.DatetimeModified = DateTime.Now;
                    transaction.UserModified = transaction.UserCreated;
                    transaction.Active = true;
                    var hsTrans = transactionRepository.Add(transaction, true);
                    var containers = csMawbcontainerRepo.Get(x => x.Mblid == jobId).ToList();
                    if (containers != null)
                    {
                        containers.ForEach(x => {
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
                            generatePrefixHouse = Constants.SEF_HBL
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
                            item.DatetimeCreated = DateTime.Now;
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
            try
            {
                var transaction = mapper.Map<CsTransaction>(model);
                transaction.Id = Guid.NewGuid();
                transaction.JobNo = CreateJobNoByTransactionType(model.TransactionTypeEnum, model.TransactionType);
                transaction.UserCreated = currentUser.UserID;
                transaction.DatetimeCreated = transaction.DatetimeModified = DateTime.Now;
                transaction.UserModified = model.UserCreated;
                transaction.Active = true;
                var hsTrans = transactionRepository.Add(transaction, false);
                if (model.CsMawbcontainers != null)
                {
                    model.CsMawbcontainers.ForEach(x => {
                        x.Id = Guid.NewGuid();
                        x.Mblid = transaction.Id;
                        x.UserModified = transaction.UserCreated;
                        x.DatetimeModified = DateTime.Now;
                    });
                    var hsCont = containerService.Add(model.CsMawbcontainers, false);
                }
                if(model.DimensionDetails != null)
                {
                    model.DimensionDetails.ForEach(x =>
                    {
                        x.Id = Guid.NewGuid();
                        x.Mblid = transaction.Id;
                        x.UserCreated = transaction.UserCreated;
                        x.DatetimeCreated = DateTime.Now;
                    });
                    dimensionDetailService.Add(model.DimensionDetails, false);
                }
                var detailTrans = csTransactionDetailRepo.Get(x => x.JobId == model.Id);
                if (detailTrans != null)
                {
                    int countDetail = csTransactionDetailRepo.Count(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month
                                                                        && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                                        && x.DatetimeCreated.Value.Day == DateTime.Now.Day);
                    string generatePrefixHouse = GenerateID.GeneratePrefixHousbillNo();

                    if (csTransactionDetailRepo.Any(x => x.Hwbno.IndexOf(generatePrefixHouse, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        generatePrefixHouse = Constants.SEF_HBL
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
                        item.DatetimeCreated = DateTime.Now;
                        csTransactionDetailRepo.Add(item, false);
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
                                csMawbcontainerRepo.Add(x, false);
                            }
                        }
                        var houseDimensions = dimensionDetailRepository.Get(x => x.Hblid == item.Id);
                        if(houseDimensions != null)
                        {
                            foreach(var x in houseDimensions)
                            {
                                x.Id = Guid.NewGuid();
                                x.Hblid = item.Id;
                                x.UserCreated = transaction.UserCreated;
                                x.DatetimeCreated = DateTime.Now;
                                dimensionDetailRepository.Add(x, false);
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
                                csShipmentSurchargeRepo.Add(charge, false);
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
                                csArrivalFrieghtChargeService.Add(freight, false);
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
                return new ResultHandle { Status = true, Message = "Import successfully!!!", Data = transaction };
            }
            catch (Exception ex)
            {
                var result = new HandleState(ex.Message);
                return new ResultHandle { Data = new object { }, Message = ex.Message, Status = true };
            }
        }

        #region -- PREVIEW --
        public Crystal PreviewSIFFormPLsheet(Guid jobId, string currency)
        {
            Crystal result = null;
            var _currentUser = currentUser.UserID;
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

            CsMawbcontainerCriteria contCriteria = new CsMawbcontainerCriteria { Mblid = jobId };
            var containerList = containerService.Query(contCriteria);
            var _containerNoList = string.Empty;
            if (containerList.Count() > 0)
            {
                _containerNoList = String.Join("\r\n", containerList.Select(x => !string.IsNullOrEmpty(x.ContainerNo) || !string.IsNullOrEmpty(x.SealNo) ? x.ContainerNo + "/" + x.SealNo : string.Empty));
            }

            var _transDate = shipment.DatetimeCreated != null ? shipment.DatetimeCreated.Value : DateTime.Now; //CreatedDate of shipment
            var _etdDate = shipment.Etd != null ? shipment.Etd.Value.ToString("dd MMM yyyy") : string.Empty; //ETD
            var _etaDate = shipment.Eta != null ? shipment.Eta.Value.ToString("dd MMM yyyy") : string.Empty; //ETA
            var _grossWeight = shipment.GrossWeight != null ? shipment.GrossWeight : 0;//Đang lấy GrossWeight của Shipment
            var _netWeight = shipment.NetWeight != null ? shipment.NetWeight.Value : 0;//Đang lấy NetWeight của Shipment
            var _chargeWeight = shipment.ChargeWeight != null ? shipment.ChargeWeight : 0;//Đang lấy ChargeWeight của Shipment
            var _dateNow = DateTime.Now.ToString("dd MMMM yyyy");

            var listCharge = new List<FormPLsheetReport>();

            CsTransactionDetailCriteria criteria = new CsTransactionDetailCriteria { JobId = jobId };
            var listHousebill = transactionDetailService.GetByJob(criteria);
            var _hblNoList = string.Empty;
            var _shipmentType = GetShipmentTypeForPreviewPL(shipment.TransactionType) + shipment.TypeOfService;
            if (listHousebill.Count > 0)
            {
                _hblNoList = String.Join("; ", listHousebill.Select(x => x.Hwbno));

                var housebillFirst = listHousebill.First();
                var userSaleman = sysUserRepo.Get(x => x.Id == housebillFirst.SaleManId).FirstOrDefault();
                var shipper = catPartnerRepo.Get(x => x.Id == housebillFirst.ShipperId).FirstOrDefault()?.PartnerNameEn;
                var consignee = catPartnerRepo.Get(x => x.Id == housebillFirst.ConsigneeId).FirstOrDefault()?.PartnerNameEn;

                var surcharges = new List<CsShipmentSurchargeDetailsModel>();
                foreach (var housebill in listHousebill)
                {
                    var surcharge = surchargeService.GetByHB(housebill.Id);
                    surcharges.AddRange(surcharge);
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
                        decimal saleProfit = 0;
                        string partnerName = string.Empty;
                        if (surcharge.Type == Constants.CHARGE_OBH_TYPE)
                        {
                            isOBH = true;
                            partnerName = surcharge.PayerName;
                        }
                        if (surcharge.Type == Constants.CHARGE_BUY_TYPE)
                        {
                            cost = surcharge.Total;
                        }
                        if (surcharge.Type == Constants.CHARGE_SELL_TYPE)
                        {
                            revenue = surcharge.Total;
                        }
                        saleProfit = cost + revenue;

                        //Check ExchangeDate # null: nếu bằng null thì gán ngày hiện tại.
                        var exchargeDateSurcharge = surcharge.ExchangeDate == null ? DateTime.Now : surcharge.ExchangeDate;
                        //Exchange Rate theo Currency truyền vào
                        var exchangeRate = currencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == exchargeDateSurcharge.Value.Date && x.CurrencyFromId == surcharge.CurrencyId && x.CurrencyToId == Constants.CURRENCY_USD && x.Active == true)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                        decimal _exchangeRateUSD;
                        if ((exchangeRate != null && exchangeRate.Rate != 0))
                        {
                            _exchangeRateUSD = exchangeRate.Rate;
                        }
                        else
                        {
                            //Exchange Rate ngược
                            var exchangeRateReverse = currencyExchangeRepository.Get(x => (x.DatetimeCreated.Value.Date == exchargeDateSurcharge.Value.Date && x.CurrencyFromId == "USD" && x.CurrencyToId == surcharge.CurrencyId && x.Active == true)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                            _exchangeRateUSD = (exchangeRateReverse != null && exchangeRateReverse.Rate != 0) ? 1 / exchangeRateReverse.Rate : 1;
                        }

                        var charge = new FormPLsheetReport();
                        charge.COSTING = "COSTING";
                        charge.TransID = shipment.JobNo; //JobNo of shipment
                        charge.TransDate = _transDate;
                        charge.HWBNO = surcharge.Hwbno;
                        charge.MAWB = shipment.Mawb; //MasterBill of shipment
                        charge.PartnerName = string.Empty; //NOT USE
                        charge.ContactName = userSaleman?.Username; //Saleman đầu tiên của list housebill
                        charge.ShipmentType = _shipmentType;
                        charge.NominationParty = string.Empty;
                        charge.Nominated = true; //Gán cứng
                        charge.POL = _polFull;
                        charge.POD = _podFull;
                        charge.Commodity = shipment.Commodity;
                        charge.Volumne = string.Empty; //Gán rỗng
                        charge.Carrier = supplier?.PartnerNameEn;
                        charge.Agent = agent?.PartnerNameEn;
                        charge.ATTN = shipper; //Shipper đầu tiên của list housebill
                        charge.Consignee = consignee; //Consignee đầu tiên của list housebill
                        charge.ContainerNo = _containerNoList; //Danh sách container của Shipment (Format: contNo/SealNo)
                        charge.OceanVessel = shipment.FlightVesselName; //Tên chuyến bay
                        charge.LocalVessel = shipment.FlightVesselName; //Tên chuyến bay
                        charge.FlightNo = shipment.VoyNo; //Mã chuyến bay
                        charge.SeaImpVoy = string.Empty;//Gán rỗng
                        charge.LoadingDate = _etdDate; //ETD
                        charge.ArrivalDate = _etaDate; //ETA
                        charge.FreightCustomer = string.Empty; //NOT USE
                        charge.FreightColoader = 0; //NOT USE
                        charge.PayableAccount = surcharge.PartnerName;//Partner name of charge
                        charge.Description = surcharge.ChargeNameEn; //Charge name of charge
                        charge.Curr = surcharge.CurrencyId; //Currency of charge
                        charge.VAT = 0; //NOT USE
                        charge.VATAmount = 0; //NOT USE
                        charge.Cost = cost; //Phí chi của charge
                        charge.Revenue = revenue; //Phí thu của charge
                        charge.Exchange = currency == Constants.CURRENCY_USD ? _exchangeRateUSD * saleProfit : 0; //Exchange phí của charge về USD
                        charge.VNDExchange = surcharge.ExchangeRate != null ? surcharge.ExchangeRate.Value : 0;
                        charge.Paid = (revenue > 0 || cost < 0) && isOBH == false ? false : true;
                        charge.DatePaid = DateTime.Now; //NOT USE
                        charge.Docs = surcharge.InvoiceNo; //InvoiceNo of charge
                        charge.Notes = surcharge.Notes;
                        charge.InputData = string.Empty; //Gán rỗng
                        charge.SalesProfit = saleProfit;
                        charge.Quantity = surcharge.Quantity;
                        charge.UnitPrice = surcharge.UnitPrice != null ? surcharge.UnitPrice.Value : 0;
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
                        charge.ShipmentSource = shipment.ShipmentType;
                        charge.RealCost = true;

                        listCharge.Add(charge);
                    }
                }
                else
                {
                    var charge = new FormPLsheetReport();
                    charge.COSTING = "COSTING";
                    charge.TransID = shipment.JobNo; //JobNo of shipment
                    charge.TransDate = _transDate;
                    charge.MAWB = shipment.Mawb; //MasterBill of shipment
                    charge.ContactName = userSaleman?.Username; //Saleman đầu tiên của list housebill
                    charge.ShipmentType = _shipmentType;
                    charge.Nominated = true;
                    charge.POL = _polFull;
                    charge.POD = _podFull;
                    charge.Commodity = shipment.Commodity;
                    charge.Carrier = supplier?.PartnerNameEn;
                    charge.Agent = agent?.PartnerNameEn;
                    charge.ATTN = shipper; //Shipper đầu tiên của list housebill
                    charge.Consignee = consignee; //Consignee đầu tiên của list housebill
                    charge.ContainerNo = _containerNoList; //Danh sách container của Shipment (Format: contNo/SealNo)
                    charge.OceanVessel = shipment.FlightVesselName; //Tên chuyến bay
                    charge.LocalVessel = shipment.FlightVesselName; //Tên chuyến bay
                    charge.FlightNo = shipment.VoyNo; //Mã chuyến bay
                    charge.SeaImpVoy = string.Empty;
                    charge.LoadingDate = _etdDate; //ETD
                    charge.ArrivalDate = _etaDate; //ETA
                    charge.LastRevised = _dateNow;
                    charge.SalesCurr = currency;
                    charge.GW = _grossWeight;//Đang lấy GrossWeight của Shipment
                    charge.MCW = _netWeight;//Đang lấy NetWeight của Shipment
                    charge.HCW = _chargeWeight;//Đang lấy ChargeWeight của Shipment
                    charge.ShipmentSource = shipment.ShipmentType;
                    charge.RealCost = true;
                    listCharge.Add(charge);
                }
            }
            else
            {
                var charge = new FormPLsheetReport();
                charge.COSTING = "COSTING";
                charge.TransID = shipment.JobNo; //JobNo of shipment
                charge.TransDate = _transDate;
                charge.MAWB = shipment.Mawb; //MasterBill of shipment
                charge.ShipmentType = _shipmentType;
                charge.Nominated = true;
                charge.POL = _polFull;
                charge.POD = _podFull;
                charge.Commodity = shipment.Commodity;
                charge.Carrier = supplier?.PartnerNameEn;
                charge.Agent = agent?.PartnerNameEn;
                charge.ContainerNo = _containerNoList; //Danh sách container của Shipment (Format: contNo/SealNo)
                charge.OceanVessel = shipment.FlightVesselName; //Tên chuyến bay
                charge.LocalVessel = shipment.FlightVesselName; //Tên chuyến bay
                charge.FlightNo = shipment.VoyNo; //Mã chuyến bay
                charge.SeaImpVoy = string.Empty;
                charge.LoadingDate = _etdDate; //ETD
                charge.ArrivalDate = _etaDate; //ETA
                charge.LastRevised = _dateNow;
                charge.SalesCurr = currency;
                charge.GW = _grossWeight;//Đang lấy GrossWeight của Shipment
                charge.MCW = _netWeight;//Đang lấy NetWeight của Shipment
                charge.HCW = _chargeWeight;//Đang lấy ChargeWeight của Shipment
                charge.ShipmentSource = shipment.ShipmentType;
                charge.RealCost = true;
                listCharge.Add(charge);
            }

            var parameter = new FormPLsheetReportParameter();
            parameter.Contact = _currentUser;//Get user login
            parameter.CompanyName = Constants.COMPANY_NAME;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress1 = Constants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎";
            parameter.Website = Constants.COMPANY_WEBSITE;
            parameter.CurrDecimalNo = 2;
            parameter.DecimalNo = 2;
            parameter.HBLList = _hblNoList;

            result = new Crystal
            {
                ReportName = "FormPLsheet.rpt",
                AllowPrint = true,
                AllowExport = true
            };
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
    }


}


