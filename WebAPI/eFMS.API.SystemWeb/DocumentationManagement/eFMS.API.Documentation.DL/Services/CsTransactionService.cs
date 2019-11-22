﻿using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Connection;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Common;
using System.Data.SqlClient;
using eFMS.API.Documentation.Service.Contexts;
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
        readonly ICsMawbcontainerService containerService;
        private readonly IStringLocalizer stringLocalizer;

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
            ICsMawbcontainerService contService) : base(repository, mapper)
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
                    shipment = "AAA";
                    break;
                case TransactionTypeEnum.AirExport:
                    shipment = "AAA";
                    break;
                case TransactionTypeEnum.AirImport:
                    shipment = "AAA";
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
            try
            {
                var transaction = mapper.Map<CsTransaction>(model);
                transaction.Id = Guid.NewGuid();
                if (model.CsMawbcontainers.Count > 0)
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
                    var branchOfUser = sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault().WorkPlaceId;
                    if (branchOfUser != null)
                    {
                        transaction.BranchId = branchOfUser;
                    }
                }

                var hsTrans = DataContext.Add(transaction);
                if (hsTrans.Success)
                {
                    var containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);
                    if (containers != null && containers.Count > 0)
                    {
                        foreach (var container in containers)
                        {
                            container.Id = Guid.NewGuid();
                            container.Mblid = transaction.Id;
                            container.UserModified = transaction.UserCreated;
                            container.DatetimeModified = DateTime.Now;
                            var hsContMBL = csMawbcontainerRepo.Add(container);
                        }
                    }
                }
                var result = hsTrans;
                return new { model = transaction, result };
            }
            catch (Exception ex)
            {
                var result = new HandleState(ex.Message);
                return new { model = new object { }, result };
            }
        }

        public HandleState UpdateCSTransaction(CsTransactionEditModel model)
        {
            try
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
                    var branchOfUser = sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault().WorkPlaceId;
                    if (branchOfUser != null)
                    {
                        transaction.BranchId = branchOfUser;
                    }
                }
                var hsTrans = DataContext.Update(transaction, x => x.Id == transaction.Id);
                if (hsTrans.Success)
                {
                    if (model.CsMawbcontainers != null && model.CsMawbcontainers.Count > 0)
                    {
                        var containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);

                        var listIdOfCont = containers.Where(x => x.Id != Guid.Empty).Select(s => s.Id);
                        var idContainersNeedRemove = csMawbcontainerRepo.Get(x => x.Mblid == transaction.Id && !listIdOfCont.Contains(x.Id)).Select(s => s.Id);
                        //Delete item of List Container MBL
                        if (idContainersNeedRemove != null && idContainersNeedRemove.Count() > 0)
                        {
                            var hsDelContHBL = csMawbcontainerRepo.Delete(x => idContainersNeedRemove.Contains(x.Id));
                        }

                        foreach (var container in containers)
                        {
                            //Insert & Update List Container MBL
                            if (container.Id == Guid.Empty)
                            {
                                container.Id = Guid.NewGuid();
                                container.Mblid = transaction.Id;
                                container.UserModified = transaction.UserModified;
                                container.DatetimeModified = DateTime.Now;
                                var hsAddContMBL = csMawbcontainerRepo.Add(container);
                            }
                            else
                            {
                                container.Mblid = transaction.Id;
                                container.UserModified = transaction.UserModified;
                                container.DatetimeModified = DateTime.Now;
                                var hsUpdateContMBL = csMawbcontainerRepo.Update(container, x => x.Id == container.Id);
                            }
                        }
                    }
                }
                return hsTrans;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
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
            var hs = new HandleState();
            try
            {
                var containers = csMawbcontainerRepo.Get(x => x.Mblid == jobId);
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
                hs = DataContext.Delete(x => x.Id == jobId);
            }
            catch (Exception ex)
            {
                hs = new HandleState(ex.Message);
            }
            return hs;
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
        private IQueryable<CsTransactionModel> GetTransaction(string transactionType, bool isSearch)
        {
            var masterBills = DataContext.Get(x => x.TransactionType == transactionType && x.CurrentStatus != TermData.Canceled);
            
            var coloaders = catPartnerRepo.Get();
            var agents = catPartnerRepo.Get();
            var pols = catPlaceRepo.Get();
            var pods = catPlaceRepo.Get();
            var creators = sysUserRepo.Get();
            IQueryable<CsTransactionModel> query = null;
            if (isSearch)
            {
                var houseBills = csTransactionDetailRepo.Get();
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
                            HblId = houseBill.Id == Guid.Empty || houseBill.Id == null ? Guid.Empty : houseBill.Id,
                        };
            }
            else
            {
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
                            PODName = pod.NameEn,
                            POLName = pol.NameEn,
                            CreatorName = creator.Username,
                        };
            }
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
            var listSearch = GetTransaction(transactionType, true);
            var listData = GetTransaction(transactionType, false);
            if (listSearch == null || listSearch.Any() == false) return null;

            IQueryable<CsTransactionModel> results = null;

            switch (criteria.TransactionType)
            {
                case TransactionTypeEnum.InlandTrucking:
                    //results = QueryIT(criteria, listSearch, listData);
                    break;
                case TransactionTypeEnum.AirExport:
                    //results = QueryAE(criteria, listSearch, listData);
                    break;
                case TransactionTypeEnum.AirImport:
                    //results = QueryAI(criteria, listSearch, listData);
                    break;
                case TransactionTypeEnum.SeaConsolExport:
                    //results = QuerySEC(criteria, listSearch, listData);
                    break;
                case TransactionTypeEnum.SeaConsolImport:
                    //results = QuerySIC(criteria, listSearch, listData);
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    results = QuerySEF(criteria, listSearch, listData);
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    results = QuerySIF(criteria, listSearch, listData);
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    //results = QuerySEL(criteria, listSearch, listData);
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    //results = QuerySIL(criteria, listSearch, listData);
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
        private IQueryable<CsTransactionModel> QueryIT(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch, IQueryable<CsTransactionModel> listData)
        {
            return null;
        }

        /// <summary>
        /// Query Air Export
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QueryAE(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch, IQueryable<CsTransactionModel> listData)
        {
            return null;
        }

        /// <summary>
        /// Query Air Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QueryAI(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch, IQueryable<CsTransactionModel> listData)
        {
            return null;
        }

        /// <summary>
        /// Query Sea Consol Export
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySEC(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch, IQueryable<CsTransactionModel> listData)
        {
            return null;
        }

        /// <summary>
        /// Query Sea Consol Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySIC(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch, IQueryable<CsTransactionModel> listData)
        {
            return null;
        }

        /// <summary>
        /// Query Sea FCL Export
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySEF(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch, IQueryable<CsTransactionModel> listData)
        {
            var containers = csMawbcontainerRepo.Get();
            var query = (from transaction in listSearch
                         join container in containers on transaction.Id equals container.Mblid into containerTrans
                         from cont in containerTrans.DefaultIfEmpty()
                         select new
                         {
                             transaction,
                             cont.ContainerNo,
                             cont.SealNo
                         });
            if (criteria.All == null)
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.Mawb ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.AgentName ?? "").IndexOf(criteria.AgentName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.transaction.CustomerId ?? "") == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                    && ((x.transaction.NotifyPartyId ?? "") == criteria.NotifyPartyId || string.IsNullOrEmpty(criteria.NotifyPartyId))
                    && ((x.transaction.SaleManId ?? "") == criteria.SaleManId || string.IsNullOrEmpty(criteria.SaleManId))
                    && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null))
                    && ((x.transaction.Etd ?? null) <= (criteria.ToDate ?? null))
                    );
                //query = query.OrderByDescending(x => x.transaction.ModifiedDate);//.ThenByDescending(x => x.transaction.CreatedDate);
            }
            else
            {
                query = query.Where(x => ((x.transaction.JobNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.Mawb ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.HWBNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.SupplierName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.AgentName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || ((x.transaction.CustomerId ?? "") == criteria.All || string.IsNullOrEmpty(criteria.All))
                             || ((x.transaction.NotifyPartyId ?? "") == criteria.All || string.IsNullOrEmpty(criteria.All))
                             || ((x.transaction.SaleManId ?? "") == criteria.All || string.IsNullOrEmpty(criteria.All))
                             || (x.SealNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.ContainerNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                             && ((x.transaction.Etd ?? null) >= (criteria.FromDate ?? null) && (x.transaction.Etd ?? null) <= (criteria.ToDate ?? null))
                    );
                //query = query.OrderByDescending(x => x.transaction.ModifiedDate);//.ThenByDescending(x => x.transaction.CreatedDate);
            }
            //return query.Select(x => x.transaction).Distinct();
            var jobNos = query.Select(s => s.transaction.JobNo).Distinct();
            var result = listData.Where(x => jobNos.Contains(x.JobNo)).OrderByDescending(x => x.DatetimeModified);
            return result;
        }

        /// <summary>
        /// Query Sea FCL Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySIF(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch, IQueryable<CsTransactionModel> listData)
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
                //query = query.OrderByDescending(x => x.transaction.ModifiedDate);
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
                //query = query.OrderByDescending(x => x.transaction.ModifiedDate);
            }
            var jobNos = query.Select(s => s.transaction.JobNo).Distinct();
            var result = listData.Where(x => jobNos.Contains(x.JobNo)).OrderByDescending(x => x.DatetimeModified);
            return result;
        }

        /// <summary>
        /// Query Sea LCL Export
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySEL(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch, IQueryable<CsTransactionModel> listData)
        {
            return null;
        }

        /// <summary>
        /// Query Sea LCL Import
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IQueryable<CsTransactionModel> QuerySIL(CsTransactionCriteria criteria, IQueryable<CsTransactionModel> listSearch, IQueryable<CsTransactionModel> listData)
        {
            return null;
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
                List<CsMawbcontainer> containers = null;
                if (model.CsMawbcontainers.Count > 0)
                {
                    containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);

                    foreach (var container in containers)
                    {
                        container.Id = Guid.NewGuid();
                        container.Mblid = transaction.Id;
                        container.UserModified = transaction.UserCreated;
                        container.DatetimeModified = DateTime.Now;
                        csMawbcontainerRepo.Add(container, false);
                    }
                }
                else
                {
                    containers = csMawbcontainerRepo.Get(x => x.Mblid == model.Id).ToList();
                    foreach (var container in containers)
                    {
                        container.ContainerNo = string.Empty;
                        container.SealNo = string.Empty;
                        container.MarkNo = string.Empty;
                        container.Id = Guid.NewGuid();
                        container.Mblid = transaction.Id;
                        container.UserModified = transaction.UserCreated;
                        container.DatetimeModified = DateTime.Now;
                        csMawbcontainerRepo.Add(container, false);
                    }
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
                                x.Mblid = Guid.Empty;
                                x.ContainerNo = string.Empty;
                                x.SealNo = string.Empty;
                                x.MarkNo = string.Empty;
                                x.UserModified = transaction.UserCreated;
                                x.DatetimeModified = DateTime.Now;
                                csMawbcontainerRepo.Add(x, false);
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
                                charge.SoaadjustmentRequestor = null;
                                charge.SoaadjustmentRequestedDate = null;
                                charge.SoaadjustmentReason = null;
                                charge.UnlockedSoadirector = null;
                                charge.UnlockedSoadirectorDate = null;
                                charge.UnlockedSoadirectorStatus = null;
                                charge.UnlockedSoasaleMan = null;
                                csShipmentSurchargeRepo.Add(charge, false);
                            }
                        }
                    }
                }
                transactionRepository.SubmitChanges();
                csTransactionDetailRepo.SubmitChanges();
                csMawbcontainerRepo.SubmitChanges();
                csShipmentSurchargeRepo.SubmitChanges();
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
            var listCharge = new List<SIFFormPLsheetReport>();
            var charge = new SIFFormPLsheetReport();
            charge.COSTING = "COSTING";
            charge.TransID = "TransID";
            charge.TransDate = DateTime.Now;
            charge.HWBNO = "HWBNO";
            charge.MAWB = "MAWB";
            charge.PartnerName = "PartnerName";
            charge.ContactName = "ContactName";
            charge.ShipmentType = "ShipmentType";
            charge.NominationParty = "NominationParty";
            charge.Nominated = true;
            charge.POL = "POL";
            charge.POD = "POD";
            charge.Commodity = "Commodity";
            charge.Volumne = "Volumne";
            charge.Carrier = "Carrier";
            charge.Agent = "Agent";
            charge.ATTN = "ATTN";
            charge.Consignee = "Consignee";
            charge.ContainerNo = "ContainerNo";
            charge.OceanVessel = "OceanVessel";
            charge.LocalVessel = "LocalVessel";
            charge.FlightNo = "FlightNo";
            charge.SeaImpVoy = "SeaImpVoy";
            charge.LoadingDate = "LoadingDate";
            charge.ArrivalDate = "ArrivalDate";
            charge.FreightCustomer = 0;
            charge.FreightColoader = 0;
            charge.PayableAccount = "PayableAccount";
            charge.Description = "Description";
            charge.Curr = "Curr";
            charge.VAT = 0;
            charge.VATAmount = 0;
            charge.Cost = 0;
            charge.Revenue = 0;
            charge.Exchange = 0;
            charge.VNDExchange = 0;
            charge.Paid = true;
            charge.DatePaid = DateTime.Now;
            charge.Docs = "Docs";
            charge.Notes = "Notes";
            charge.InputData = "InputData";
            charge.SalesProfit = 0;
            charge.Quantity = 0;
            charge.UnitPrice = 0;
            charge.Unit = "Unit";
            charge.LastRevised = "LastRevised";
            charge.OBH = true;
            charge.ExtRateVND = 0;
            charge.KBck = true;
            charge.NoInv = true;
            charge.Approvedby = "Approvedby";
            charge.ApprovedDate = DateTime.Now;
            charge.SalesCurr = "SalesCurr";
            charge.GW = 0;
            charge.MCW = 0;
            charge.HCW = 0;
            charge.PaymentTerm = "PaymentTerm";
            charge.DetailNotes = "DetailNotes";
            charge.ExpressNotes = "ExpressNotes";
            charge.InvoiceNo = "InvoiceNo";
            charge.CodeVender = "CodeVender";
            charge.CodeCus = "CodeCus";
            charge.Freight = true;
            charge.Collect = true;
            charge.FreightPayableAt = "FreightPayableAt";
            charge.PaymentTime = 0;
            charge.PaymentTimeCus = 0;
            charge.Noofpieces = 0;
            charge.UnitPieces = "UnitPieces";
            charge.TypeofService = "TypeofService";
            charge.ShipmentSource = "ShipmentSource";
            charge.RealCost = true;

            listCharge.Add(charge);

            var parameter = new SIFFormPLsheetReportParams();
            parameter.Contact = _currentUser;//Get user login
            parameter.CompanyName = Constants.COMPANY_NAME;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress1 = Constants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎";
            parameter.Website = Constants.COMPANY_WEBSITE;
            parameter.CurrDecimalNo = 2;
            parameter.DecimalNo = 2;
            parameter.HBLList = "HBList";

            result = new Crystal
            {
                ReportName = "SIFFormPLsheet.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listCharge);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
        #endregion -- PREVIEW --
    }
}
