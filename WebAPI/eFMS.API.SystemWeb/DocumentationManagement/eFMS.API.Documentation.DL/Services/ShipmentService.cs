﻿using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace eFMS.API.Documentation.DL.Services
{
    public class ShipmentService : RepositoryBase<CsTransaction, CsTransactionModel>, IShipmentService
    {
        readonly IContextBase<OpsTransaction> opsRepository;
        readonly IContextBase<CsTransactionDetail> detailRepository;
        readonly IContextBase<CsShipmentSurcharge> surCharge;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<OpsStageAssigned> opsStageAssignedRepo;
        private readonly ICurrentUser currentUser;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<CatCharge> catChargeRepo;
        readonly IContextBase<CatChargeGroup> catChargeGroupRepo;
        readonly IContextBase<SysOffice> sysOfficeRepo;
        readonly IContextBase<SysUserLevel> sysUserLevelRepo;
        private readonly ICurrencyExchangeService currencyExchangeService;

        public ShipmentService(IContextBase<CsTransaction> repository, IMapper mapper,
            IContextBase<OpsTransaction> ops,
            IContextBase<CsTransactionDetail> detail,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<CatPartner> catPartner,
            IContextBase<OpsStageAssigned> opsStageAssigned,
            ICurrentUser user,
            IContextBase<SysUser> sysUser,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            IContextBase<CatPlace> catPlace,
            IContextBase<CatCharge> catCharge,
            IContextBase<CatChargeGroup> catChargeGroup,
            IContextBase<SysOffice> sysOffice,
            IContextBase<SysUserLevel> sysUserLevel,
            ICurrencyExchangeService currencyExchange) : base(repository, mapper)
        {
            opsRepository = ops;
            detailRepository = detail;
            surCharge = surcharge;
            catPartnerRepo = catPartner;
            opsStageAssignedRepo = opsStageAssigned;
            currentUser = user;
            sysEmployeeRepo = sysEmployee;
            sysUserRepo = sysUser;
            catCurrencyExchangeRepo = catCurrencyExchange;
            catPlaceRepo = catPlace;
            catChargeRepo = catCharge;
            catChargeGroupRepo = catChargeGroup;
            sysOfficeRepo = sysOffice;
            sysUserLevelRepo = sysUserLevel;
            currencyExchangeService = currencyExchange;
        }

        public IQueryable<Shipments> GetShipmentNotLocked()
        {
            var userCurrent = currentUser.UserID;
            //Start change request Modified 14/10/2019 by Andy.Hoa
            //Get list shipment operation theo user current
            var shipmentsOperation = from ops in opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false)
                                     join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId
                                     where osa.MainPersonInCharge == userCurrent
                                     select new Shipments
                                     {
                                         Id = ops.Id,
                                         JobId = ops.JobNo,
                                         HBL = ops.Hwbno,
                                         MBL = ops.Mblno,
                                         CustomerId = ops.CustomerId,
                                         AgentId = ops.AgentId,
                                         CarrierId = ops.SupplierId,
                                         HBLID = ops.Hblid
                                     };
            shipmentsOperation = shipmentsOperation.GroupBy(x => new { x.Id, x.JobId, x.HBL, x.MBL, x.CustomerId, x.AgentId, x.CarrierId, x.HBLID }).Select(s => new Shipments
            {
                Id = s.Key.Id,
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL,
                CustomerId = s.Key.CustomerId,
                AgentId = s.Key.AgentId,
                CarrierId = s.Key.CarrierId,
                HBLID = s.Key.HBLID,
                Service = "CL"
            });
            shipmentsOperation = shipmentsOperation.Distinct();

            //End change request
            var transactions = from cst in DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false)
                               join osa in opsStageAssignedRepo.Get() on cst.Id equals osa.JobId
                               where osa.MainPersonInCharge == userCurrent
                               select cst;
            var shipmentsDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.x.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.x.Mawb,
                CustomerId = x.y.CustomerId,
                AgentId = x.x.AgentId,
                CarrierId = x.x.ColoaderId,
                HBLID = x.y.Id,
                Service = x.x.TransactionType
            });
            shipmentsDocumention = shipmentsDocumention.Distinct();

            var shipments = shipmentsOperation.Union(shipmentsDocumention);
            return shipments;
        }

        public IQueryable<Shipments> GetShipmentsCreditPayer(string partner, List<string> services)
        {
            //Chỉ lấy ra những phí Credit(BUY) & Payer (chưa bị lock)
            var surcharge = surCharge.Get(x =>
                    (x.Type == DocumentConstants.CHARGE_BUY_TYPE || (x.PayerId != null && x.CreditNo != null))
                && (x.PayerId == partner || x.PaymentObjectId == partner)
            );

            var transactions = DataContext.Get(x => x.IsLocked == false && services.Contains(x.TransactionType));
            var shipmentDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.y.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.x.Mawb,
            });
            var shipmentsDocumention = surcharge.Join(shipmentDocumention, x => x.Hblid, y => y.Id, (x, y) => new { x, y })
                .Select(x => new Shipments
                {
                    Id = x.x.Id,
                    JobId = x.y.JobId,
                    HBL = x.y.HBL,
                    MBL = x.y.MBL,
                });

            IQueryable<Shipments> shipments = shipmentsDocumention
                .Where(x => x.JobId != null && x.HBL != null && x.MBL != null)
                .Select(s => new Shipments { JobId = s.JobId, HBL = s.HBL, MBL = s.MBL });
            //Nếu có chứa Service Custom Logistic
            if (services.Contains("CL"))
            {
                var shipmentOperation = opsRepository.Get(x => x.IsLocked == false && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);
                var shipmentsOperation = surcharge.Join(shipmentOperation, x => x.Hblid, y => y.Hblid, (x, y) => new { x, y }).Select(x => new Shipments
                {
                    JobId = x.y.JobNo,
                    HBL = x.y.Hwbno,
                    MBL = x.y.Mblno,
                });
                shipments = shipmentsDocumention.Union(shipmentsOperation).Where(x => x.JobId != null && x.HBL != null && x.MBL != null).Select(s => new Shipments { JobId = s.JobId, HBL = s.HBL, MBL = s.MBL });
            }

            var shipmentsResult = shipments.GroupBy(x => new { x.JobId, x.HBL, x.MBL }).Select(s => new Shipments
            {
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL
            });
            return shipmentsResult;
        }

        public List<ShipmentsCopy> GetListShipmentBySearchOptions(string searchOption, List<string> keywords)
        {
            var userCurrent = currentUser.UserID;

            var dataList = new List<ShipmentsCopy>();

            if (string.IsNullOrEmpty(searchOption) || keywords == null || keywords.Count == 0 || keywords.Any(x => x == null)) return dataList;

            var surcharge = surCharge.Get();
            var cstran = from cstd in DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED)
                         join osa in opsStageAssignedRepo.Get() on cstd.Id equals osa.JobId //So sánh bằng
                         where osa.MainPersonInCharge == userCurrent
                         select cstd;

            var cstrandel = detailRepository.Get();
            //var opstran = opsRepository.Get(x => x.CurrentStatus != Constants.CURRENT_STATUS_CANCELED);

            //Start change request Modified 14/10/2019 by Andy.Hoa
            //Get list shipment operation theo user current
            var opstran = from ops in opsRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED)
                          join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId //So sánh bằng
                          where osa.MainPersonInCharge == userCurrent
                          select ops;
            var shipmentOperation = from ops in opstran
                                    join sur in surcharge on ops.Hblid equals sur.Hblid into sur2
                                    from sur in sur2.DefaultIfEmpty()
                                    join cus in catPartnerRepo.Get() on ops.CustomerId equals cus.Id into cus2
                                    from cus in cus2.DefaultIfEmpty()
                                    where
                                        searchOption.Equals("JobNo") ? keywords.Contains(ops.JobNo) : true
                                    &&
                                        searchOption.Equals("Hwbno") ? keywords.Contains(ops.Hwbno) : true
                                    &&
                                        searchOption.Equals("Mawb") ? keywords.Contains(ops.Mblno) : true
                                    &&
                                        searchOption.Equals("ClearanceNo") ? keywords.Contains(sur.ClearanceNo) : true
                                    select new ShipmentsCopy
                                    {
                                        JobId = ops.JobNo,
                                        Customer = cus.ShortName,
                                        MBL = ops.Mblno,
                                        HBL = ops.Hwbno,
                                        HBLID = ops.Hblid,
                                        CustomNo = sur.ClearanceNo,
                                        Service = "CL"
                                    };
            shipmentOperation = shipmentOperation.Distinct();
            //End change request

            var shipmentDoc = from cstd in cstrandel
                              join cst in cstran on cstd.JobId equals cst.Id into cst2
                              from cst in cst2.DefaultIfEmpty()
                              join sur in surcharge on cstd.Id equals sur.Hblid into sur2
                              from sur in sur2.DefaultIfEmpty()
                              join cus in catPartnerRepo.Get() on cstd.CustomerId equals cus.Id into cus2
                              from cus in cus2.DefaultIfEmpty()
                              where
                                    searchOption.Equals("JobNo") ? keywords.Contains(cst.JobNo) : true
                                &&
                                    searchOption.Equals("Hwbno") ? keywords.Contains(cstd.Hwbno) : true
                                &&
                                    searchOption.Equals("Mawb") ? keywords.Contains(cstd.Mawb) : true
                                &&
                                    searchOption.Equals("ClearanceNo") ? keywords.Contains(sur.ClearanceNo) : true
                              select new ShipmentsCopy
                              {
                                  JobId = cst.JobNo,
                                  Customer = cus.ShortName,
                                  MBL = cst.Mawb,
                                  HBL = cstd.Hwbno,
                                  HBLID = cstd.Id,
                                  CustomNo = sur.ClearanceNo,
                                  Service = cst.TransactionType
                              };
            shipmentDoc = shipmentDoc.Distinct();

            var query = shipmentOperation.Union(shipmentDoc);
            var listShipment = query.Where(x => x.JobId != null && x.HBL != null && x.MBL != null)
                            .GroupBy(x => new { x.JobId, x.Customer, x.MBL, x.HBL, x.HBLID, x.CustomNo, x.Service })
                            .Select(s => new ShipmentsCopy
                            {
                                JobId = s.Key.JobId,
                                Customer = s.Key.Customer,
                                MBL = s.Key.MBL,
                                HBL = s.Key.HBL,
                                HBLID = s.Key.HBLID,
                                CustomNo = s.Key.CustomNo,
                                Service = s.Key.Service
                            });

            dataList = listShipment.AsEnumerable().Select((x, index) => new ShipmentsCopy
            {
                No = index + 1,
                JobId = x.JobId,
                Customer = x.Customer,
                MBL = x.MBL,
                HBL = x.HBL,
                HBLID = x.HBLID,
                CustomNo = x.CustomNo,
                Service = Common.CustomData.Services.FirstOrDefault(s => s.Value == x.Service)?.DisplayName
            }).ToList();
            return dataList;
        }
        private IQueryable<CsTransaction> GetShipmentServicesByTime(IQueryable<CsTransaction> csTransactions, ShipmentCriteria criteria)
        {
            switch (criteria.TransactionType)
            {
                case TransactionTypeEnum.AirExport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                                                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.AirImport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.SeaFCLImport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    csTransactions = csTransactions.Where(x => (((x.Etd ?? null) >= (criteria.FromDate ?? null)) && ((x.Etd ?? null) <= (criteria.ToDate ?? null)))
                        || (criteria.FromDate == null && criteria.ToDate == null));
                    break;
            }
            return csTransactions;
        }
        public LockedLogResultModel GetShipmentToUnLock(ShipmentCriteria criteria)
        {
            LockedLogResultModel result = null;
            IQueryable<LockedLogModel> opShipments = null;
            string transactionType = string.Empty;
            opShipments = opsRepository.Get(x => ((criteria.ShipmentPropertySearch == ShipmentPropertySearch.JOBID ? criteria.Keywords.Contains(x.JobNo) : true
                                                    && criteria.ShipmentPropertySearch == ShipmentPropertySearch.MBL ? criteria.Keywords.Contains(x.Mblno) : true)
                                                    || criteria.Keywords == null)
                                                    &&
                                                    (
                                                            (((x.ServiceDate ?? null) >= (criteria.FromDate ?? null)) && ((x.ServiceDate ?? null) <= (criteria.ToDate ?? null)))
                                                        || (criteria.FromDate == null && criteria.ToDate == null)
                                                    )
                                                    )
                .Select(x => new LockedLogModel
                {
                    Id = x.Id,
                    OPSShipmentNo = x.JobNo,
                    LockedLog = x.LockedLog,
                    IsLocked = x.IsLocked
                });
            if (criteria.TransactionType == TransactionTypeEnum.CustomLogistic)
            {
                result = GetLogHistory(opShipments);
                return result;
            }
            if (criteria.TransactionType > 0)
            {

                transactionType = DataTypeEx.GetType(criteria.TransactionType);
            }
            var csTransactions = DataContext.Get(x => ((criteria.ShipmentPropertySearch == ShipmentPropertySearch.JOBID ? criteria.Keywords.Contains(x.JobNo) : true
                                                 && criteria.ShipmentPropertySearch == ShipmentPropertySearch.MBL ? criteria.Keywords.Contains(x.Mawb) : true)
                                                    || criteria.Keywords == null)
                                                 && (x.TransactionType == transactionType || string.IsNullOrEmpty(transactionType))
                                              );
            csTransactions = GetShipmentServicesByTime(csTransactions, criteria);
            if (criteria.ShipmentPropertySearch == ShipmentPropertySearch.HBL)
            {
                var shipmentDetails = detailRepository.Get(x => criteria.Keywords.Contains(x.Hwbno));
                csTransactions = csTransactions.Join(shipmentDetails, x => x.Id, y => y.JobId, (x, y) => x);
            }
            var csShipments = csTransactions
                .Select(x => new LockedLogModel
                {
                    Id = x.Id,
                    CSShipmentNo = x.JobNo,
                    LockedLog = x.LockedLog,
                    IsLocked = x.IsLocked
                });

            IQueryable<LockedLogModel> shipments = null;

            if (opShipments != null && csShipments != null)
            {
                shipments = opShipments.Union(csShipments);
            }
            else if (csShipments == null && opShipments != null)
            {
                shipments = opShipments;
            }
            else if (opShipments == null && csShipments != null)
            {
                shipments = csShipments;
            }
            result = GetLogHistory(shipments);
            return result;
        }

        private LockedLogResultModel GetLogHistory(IQueryable<LockedLogModel> shipments)
        {
            var result = new LockedLogResultModel();
            if (shipments == null) return result;
            result.LockedLogs = shipments.Where(x => x.IsLocked == true);
            result.Logs = new List<string>();
            foreach (var item in shipments)
            {
                var logs = item.LockedLog != null ? item.LockedLog.Split(';').Where(x => x.Length > 0).ToList() : new List<string>();
                if (logs.Count > 0)
                {
                    result.Logs.AddRange(logs);
                }
            }
            return result;
        }

        public HandleState UnLockShipment(List<LockedLogModel> shipments)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in shipments)
                    {
                        if (item.OPSShipmentNo != null)
                        {
                            var opsShipment = opsRepository.Get(x => x.Id == item.Id)?.FirstOrDefault();
                            if (opsShipment != null)
                            {
                                if (opsShipment.IsLocked == true)
                                {
                                    opsShipment.IsLocked = false;
                                    opsShipment.DatetimeModified = DateTime.Now;
                                    opsShipment.UserModified = currentUser.UserID;
                                    string log = opsShipment.JobNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + currentUser.UserName + ";";
                                    opsShipment.LockedLog = opsShipment.LockedLog + log;
                                    var isSuccessLockOps = opsRepository.Update(opsShipment, x => x.Id == opsShipment.Id);
                                }
                            }
                        }
                        else
                        {
                            var csShipment = DataContext.Get(x => x.Id == item.Id)?.FirstOrDefault();
                            if (csShipment != null)
                            {
                                if (csShipment.IsLocked == true)
                                {
                                    csShipment.IsLocked = false;
                                    csShipment.DatetimeModified = DateTime.Now;
                                    csShipment.UserModified = currentUser.UserID;
                                    string log = csShipment.JobNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + currentUser.UserName + ";";
                                    csShipment.LockedLog = csShipment.LockedLog + log;
                                    var isSuccessLockCs = DataContext.Update(csShipment, x => x.Id == csShipment.Id);
                                }
                            }
                        }
                    }
                    trans.Commit();
                    return new HandleState();
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

        public IQueryable<Shipments> GetShipmentNotDelete()
        {

            //Get list shipment operation: Current Status != 'Canceled'
            var shipmentsOperation = from ops in opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED)
                                     select new Shipments
                                     {
                                         Id = ops.Id,
                                         JobId = ops.JobNo,
                                         HBL = ops.Hwbno,
                                         MBL = ops.Mblno,
                                         CustomerId = ops.CustomerId,
                                         AgentId = ops.AgentId,
                                         CarrierId = ops.SupplierId,
                                         HBLID = ops.Hblid
                                     };
            shipmentsOperation = shipmentsOperation.GroupBy(x => new { x.Id, x.JobId, x.HBL, x.MBL, x.CustomerId, x.AgentId, x.CarrierId, x.HBLID }).Select(s => new Shipments
            {
                Id = s.Key.Id,
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL,
                CustomerId = s.Key.CustomerId,
                AgentId = s.Key.AgentId,
                CarrierId = s.Key.CarrierId,
                HBLID = s.Key.HBLID,
                Service = "CL"
            });
            //Get list shipment document: Current Status != 'Canceled'
            var transactions = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);
            var shipmentsDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.x.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.x.Mawb,
                CustomerId = x.y.CustomerId,
                AgentId = x.x.AgentId,
                CarrierId = x.x.ColoaderId,
                HBLID = x.y.Id,
                Service = x.x.TransactionType
            });
            var shipments = shipmentsOperation.Union(shipmentsDocumention);
            return shipments;
        }
        #region -- EXPORT SHIPMENT OVERVIEW
        public IQueryable<GeneralExportShipmentOverviewResult> GetDataGeneralExportShipmentOverview(GeneralReportCriteria criteria)
        {
            IQueryable<GeneralExportShipmentOverviewResult> list;
            var dataShipment = GeneralExportShipmentOverview(criteria);
            var dataOperation = GeneralExportOperationOverview(criteria);
            list = dataShipment.Union(dataOperation);
            return list;
        }


        public IQueryable<GeneralExportShipmentOverviewResult> GeneralExportShipmentOverview(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewResult> lstShipment = new List<GeneralExportShipmentOverviewResult>();
            var dataShipment = QueryDataShipmentOverview(criteria);
            foreach (var item in dataShipment)
            {
                GeneralExportShipmentOverviewResult data = new GeneralExportShipmentOverviewResult();
                if (item.ServiceName == TermData.InlandTrucking)
                {
                    data.ServiceName = "Inland Trucking ";
                }
                if (item.ServiceName == TermData.AirExport)
                {
                    data.ServiceName = "Export (Air) ";
                }
                if (item.ServiceName == TermData.AirImport)
                {
                    data.ServiceName = "Import (Air) ";
                }
                if (item.ServiceName == TermData.SeaConsolExport)
                {
                    data.ServiceName = "Export (Sea Consol) ";
                }
                if (item.ServiceName == TermData.SeaConsolImport)
                {
                    data.ServiceName = "Import (Sea Consol) ";
                }
                if (item.ServiceName == TermData.SeaFCLExport)
                {
                    data.ServiceName = "Export (Sea FCL) ";
                }
                if (item.ServiceName == TermData.SeaFCLImport)
                {
                    data.ServiceName = "Import (Sea FCL) ";
                }
                if (item.ServiceName == TermData.SeaLCLExport)
                {
                    data.ServiceName = "Export (Sea LCL) ";
                }
                if (item.ServiceName == TermData.SeaLCLImport)
                {
                    data.ServiceName = "Import (Sea LCL) ";
                }
                data.JobNo = item.JobNo;
                data.etd = item.etd;
                data.eta = item.eta;
                data.FlightNo = item.FlightNo;
                data.MblMawb = item.MblMawb;
                data.HblHawb = item.HblHawb;
                data.PolPod = catPlaceRepo.Get(x => x.Id == item.Pol).Select(t => t.Code).FirstOrDefault() + "/" + catPlaceRepo.Get(x => x.Id == item.Pod).Select(t => t.Code).FirstOrDefault();
                data.Carrier = catPartnerRepo.Get(x => x.Id == item.Carrier).FirstOrDefault()?.ShortName;
                data.Agent = catPartnerRepo.Get(x => x.Id == item.Agent).FirstOrDefault()?.ShortName;
                data.Shipper = catPartnerRepo.Get(x => x.Id == item.Shipper).FirstOrDefault()?.PartnerNameEn;
                data.Consignee = catPartnerRepo.Get(x => x.Id == item.Consignee).FirstOrDefault()?.PartnerNameEn;
                data.ShipmentType = item.ShipmentType;
                data.Salesman = sysUserRepo.Get(x => x.Id == item.Salesman).FirstOrDefault()?.Username;
                data.AgentName = catPartnerRepo.Get(x => x.Id == item.Agent).FirstOrDefault()?.PartnerNameVn;
                data.GW = item.GW;
                data.CW = item.CW;
                #region -- Phí Selling trước thuế --
                decimal _totalSellAmountFreight = 0;
                decimal _totalSellAmountTrucking = 0;
                decimal _totalSellAmountHandling = 0;
                decimal _totalSellAmountOther = 0;
                var _chargeSell = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == item.HblId);
                foreach (var charge in _chargeSell)
                {

                    var chargeObj = catChargeRepo.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                    var charGroupObj = catChargeGroupRepo.Get(x => x.Id == chargeObj.ChargeGroup).FirstOrDefault();
                    //SELL
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    /*if (_rate == null)
                    {
                        var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date).ToList();
                        _rate = GetRateCurrencyExchange(currencyExchange, charge.CurrencyId, criteria.Currency);
                    }*/
                    // tinh total phi chargeGroup freight
                    if (charGroupObj?.Name == "Freight")
                    {
                        _totalSellAmountFreight += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Trucking")
                    {
                        _totalSellAmountTrucking += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Handling")
                    {
                        _totalSellAmountHandling += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Other")
                    {
                        _totalSellAmountOther += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    //END SELL

                }
                data.TotalSellFreight = _totalSellAmountFreight;
                data.TotalSellTrucking = _totalSellAmountTrucking;
                data.TotalSellHandling = _totalSellAmountHandling;
                data.TotalSellOthers = _totalSellAmountOther;
                data.TotalSell = data.TotalSellFreight + data.TotalSellTrucking + data.TotalSellHandling + data.TotalSellOthers;
                #endregion
                #region -- Phí Buying trước thuế --
                decimal _totalBuyAmountFreight = 0;
                decimal _totalBuyAmountTrucking = 0;
                decimal _totalBuyAmountHandling = 0;
                decimal _totalBuyAmountOther = 0;
                decimal _totalBuyAmountKB = 0;
                var _chargeBuy = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.Hblid == item.HblId);
                foreach (var charge in _chargeBuy)
                {
                    var chargeObj = catChargeRepo.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                    var charGroupObj = catChargeGroupRepo.Get(x => x.Id == chargeObj.ChargeGroup).FirstOrDefault();
                    //BUY
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    /*if (_rate == null)
                    {
                        var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date).ToList();
                        _rate = GetRateCurrencyExchange(currencyExchange, charge.CurrencyId, criteria.Currency);
                    }*/
                    // tinh total phi chargeGroup freight
                    if (charGroupObj?.Name == "Freight")
                    {
                        _totalBuyAmountFreight += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Trucking")
                    {
                        _totalBuyAmountTrucking += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Handling")
                    {
                        _totalBuyAmountHandling += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Other")
                    {
                        _totalBuyAmountOther += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charge.KickBack == true)
                    {
                        _totalBuyAmountKB += charge.Quantity * charge.UnitPrice * _rate ?? 0;
                    }

                    //END BUY
                }
                data.TotalBuyFreight = _totalBuyAmountFreight;
                data.TotalBuyTrucking = _totalBuyAmountTrucking;
                data.TotalBuyHandling = _totalBuyAmountHandling;
                data.TotalBuyOthers = _totalBuyAmountOther;
                data.TotalBuyKB = _totalBuyAmountKB;
                data.TotalBuy = data.TotalSellFreight + data.TotalSellTrucking + data.TotalSellHandling + data.TotalSellOthers + _totalBuyAmountKB;
                data.Profit = data.TotalSell - data.TotalBuy;
                #endregion -- Phí Buying trước thuế --

                #region -- Phí OBH sau thuế --
                decimal _obh = 0;
                var _chargeObh = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE && x.Hblid == item.HblId);
                foreach (var charge in _chargeObh)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    /*if (_rate == null)
                    {
                        var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date).ToList();
                        _rate = GetRateCurrencyExchange(currencyExchange, charge.CurrencyId, criteria.Currency);
                    }*/
                    _obh += charge.Total * _rate; // Phí OBH sau thuế
                }
                data.AmountOBH = _obh;
                #endregion -- Phí OBH sau thuế --
                data.Destination = catPlaceRepo.Get(x => x.Id == item.Pod).Select(t => t.NameVn).FirstOrDefault();
                data.CustomerId = item.CustomerId;
                data.CustomerName = catPartnerRepo.Get(x => x.Id == item.CustomerId).Select(t => t.ShortName).FirstOrDefault();
                data.RalatedHblHawb = string.Empty;// tạm thời để trống
                data.RalatedJobNo = string.Empty;// tạm thời để trống
                data.HandleOffice = sysOfficeRepo.Get(x => x.Id == item.OfficeId).Select(t => t.Code).FirstOrDefault();
                var OfficeSaleman = sysUserLevelRepo.Get(x => x.UserId == item.Salesman).Select(t => t.OfficeId).FirstOrDefault();
                data.SalesOffice = sysOfficeRepo.Get(x => x.Id == OfficeSaleman).Select(t => t.Code).FirstOrDefault();
                data.Creator = sysUserRepo.Get(x => x.Id == item.Creator).Select(t => t.Username).FirstOrDefault();
                data.POINV = item.POINV;
                data.BKRefNo = item.JobNo;
                data.Commodity = item.Commodity;
                data.ServiceMode = item.ServiceMode;//chua co thong tin
                data.PMTerm = item.PMTerm;
                data.ShipmentNotes = item.ShipmentNotes;
                data.Created = item.Created;
                lstShipment.Add(data);
            }
            return lstShipment.AsQueryable();
        }

        public IQueryable<GeneralExportShipmentOverviewResult> GeneralExportOperationOverview(GeneralReportCriteria criteria)
        {
            List<GeneralExportShipmentOverviewResult> lstShipment = new List<GeneralExportShipmentOverviewResult>();
            var dataOpertation = QueryDataOperation(criteria);
            foreach (var item in dataOpertation)
            {
                GeneralExportShipmentOverviewResult data = new GeneralExportShipmentOverviewResult();
                data.ServiceName = item.ProductService;
                data.JobNo = item.JobNo;
                data.PolPod = catPlaceRepo.Get(x => x.Id == data.Pol).Select(t => t.Code).FirstOrDefault() + "/" + catPlaceRepo.Get(x => x.Id == data.Pod).Select(t => t.Code).FirstOrDefault();
                data.Shipper = catPartnerRepo.Get(x => x.Id == item.Shipper).FirstOrDefault()?.PartnerNameEn;
                data.Consignee = catPartnerRepo.Get(x => x.Id == item.Consignee).FirstOrDefault()?.PartnerNameEn;
                #region -- Phí Selling trước thuế --
                decimal _totalSellAmountFreight = 0;
                decimal _totalSellAmountTrucking = 0;
                decimal _totalSellAmountHandling = 0;
                decimal _totalSellAmountOther = 0;
                var _chargeSell = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == item.Hblid);
                foreach (var charge in _chargeSell)
                {

                    var chargeObj = catChargeRepo.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                    var charGroupObj = catChargeGroupRepo.Get(x => x.Id == chargeObj.ChargeGroup).FirstOrDefault();
                    //SELL
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    /*if (_rate == null)
                    {
                        var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date).ToList();
                        _rate = GetRateCurrencyExchange(currencyExchange, charge.CurrencyId, criteria.Currency);
                    }*/
                    // tinh total phi chargeGroup freight
                    if (charGroupObj?.Name == "Freight")
                    {
                        _totalSellAmountFreight += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Trucking")
                    {
                        _totalSellAmountTrucking += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Handling")
                    {
                        _totalSellAmountHandling += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Other")
                    {
                        _totalSellAmountOther += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    //END SELL

                }
                data.TotalSellFreight = _totalSellAmountFreight;
                data.TotalSellTrucking = _totalSellAmountTrucking;
                data.TotalSellHandling = _totalSellAmountHandling;
                data.TotalSellOthers = _totalSellAmountOther;
                data.TotalSell = data.TotalSellFreight + data.TotalSellTrucking + data.TotalSellHandling + data.TotalSellOthers;
                #endregion
                #region -- Phí Buying trước thuế --
                decimal _totalBuyAmountFreight = 0;
                decimal _totalBuyAmountTrucking = 0;
                decimal _totalBuyAmountHandling = 0;
                decimal _totalBuyAmountOther = 0;
                decimal _totalBuyAmountKB = 0;
                var _chargeBuy = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.Hblid == item.Hblid);
                foreach (var charge in _chargeBuy)
                {
                    var chargeObj = catChargeRepo.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                    var charGroupObj = catChargeGroupRepo.Get(x => x.Id == chargeObj.ChargeGroup).FirstOrDefault();
                    //BUY
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    /*if (_rate == null)
                    {
                        var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date).ToList();
                        _rate = GetRateCurrencyExchange(currencyExchange, charge.CurrencyId, criteria.Currency);
                    }*/
                    // tinh total phi chargeGroup freight
                    if (charGroupObj?.Name == "Freight")
                    {
                        _totalBuyAmountFreight += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Trucking")
                    {
                        _totalBuyAmountTrucking += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Handling")
                    {
                        _totalBuyAmountHandling += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charGroupObj?.Name == "Other")
                    {
                        _totalBuyAmountOther += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                    }
                    if (charge.KickBack == true)
                    {
                        _totalBuyAmountKB += charge.Quantity * charge.UnitPrice * _rate ?? 0;
                    }

                    //END BUY
                }
                data.TotalBuyFreight = _totalBuyAmountFreight;
                data.TotalBuyTrucking = _totalBuyAmountTrucking;
                data.TotalBuyHandling = _totalBuyAmountHandling;
                data.TotalBuyOthers = _totalBuyAmountOther;
                data.TotalBuyKB = _totalBuyAmountKB;
                data.TotalBuy = data.TotalSellFreight + data.TotalSellTrucking + data.TotalSellHandling + data.TotalSellOthers + _totalBuyAmountKB;
                data.Profit = data.TotalSell - data.TotalBuy;
                #endregion -- Phí Buying trước thuế --

                #region -- Phí OBH sau thuế --
                decimal _obh = 0;
                var _chargeObh = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE && x.Hblid == item.Hblid);
                foreach (var charge in _chargeObh)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    /*if (_rate == null)
                    {
                        var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == charge.ExchangeDate.Value.Date).ToList();
                        _rate = GetRateCurrencyExchange(currencyExchange, charge.CurrencyId, criteria.Currency);
                    }*/
                    _obh += charge.Total * _rate; // Phí OBH sau thuế
                }
                data.AmountOBH = _obh;
                #endregion -- Phí OBH sau thuế --
                data.Destination = catPlaceRepo.Get(x => x.Id == item.Pod).Select(t => t.NameVn).FirstOrDefault();
                data.CustomerId = item.CustomerId;
                data.CustomerName = catPartnerRepo.Get(x => x.Id == item.CustomerId).Select(t => t.ShortName).FirstOrDefault();
                data.RalatedHblHawb = string.Empty;// tạm thời để trống
                data.RalatedJobNo = string.Empty;// tạm thời để trống
                data.HandleOffice = sysOfficeRepo.Get(x => x.Id == item.OfficeId).Select(t => t.Code).FirstOrDefault();
                var OfficeSaleman = sysUserLevelRepo.Get(x => x.UserId == item.SalemanId).Select(t => t.OfficeId).FirstOrDefault();
                data.SalesOffice = sysOfficeRepo.Get(x => x.Id == OfficeSaleman).Select(t => t.Code).FirstOrDefault();
                data.BKRefNo = item.JobNo;
                data.ServiceMode = item.ServiceMode;//chua co thong tin
                lstShipment.Add(data);
            }
            return lstShipment.AsQueryable();
        }



        private IQueryable<GeneralExportShipmentOverviewResult> QueryDataShipmentOverview(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans;
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = null;
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                queryTrans = q =>
                    q.TransactionType.Contains("E") ?
                    (q.Etd.HasValue ? q.Etd.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Etd.Value.Date <= criteria.ServiceDateTo.Value.Date : false)
                    :
                    (q.Eta.HasValue ? q.Eta.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Eta.Value.Date <= criteria.ServiceDateTo.Value.Date : false);
            }
            else
            {
                queryTrans = q =>
                    q.DatetimeCreated.HasValue ? q.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && q.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date : false;
            }

            if (!string.IsNullOrEmpty(criteria.CustomerId))
            {
                queryTranDetail = q => q.CustomerId == criteria.CustomerId;
            }

            if (!string.IsNullOrEmpty(criteria.Service))
            {
                queryTrans = queryTrans.And(q => criteria.Service.Contains(q.TransactionType));
            }
            else
            {
                queryTrans = queryTrans.And(q => q.TransactionType == criteria.Service);
            }

            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                queryTrans = queryTrans.And(q => q.JobNo == criteria.JobId);
            }

            if (!string.IsNullOrEmpty(criteria.Mawb))
            {
                queryTrans = queryTrans.And(q => q.Mawb == criteria.Mawb);
            }

            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryTranDetail = queryTranDetail == null ?
                    (q => q.Hwbno == criteria.Hawb)
                    :
                    queryTranDetail.And(q => q.Hwbno == criteria.Hawb);
            }

            if (!string.IsNullOrEmpty(criteria.OfficeId))
            {
                queryTrans = queryTrans.And(q => criteria.OfficeId.Contains(q.OfficeId.ToString()));
            }
            else
            {
                queryTrans = queryTrans.And(q => q.OfficeId == Guid.Empty);
            }

            if (!string.IsNullOrEmpty(criteria.DepartmentId))
            {
                queryTrans = queryTrans.And(q => criteria.DepartmentId.Contains(q.DepartmentId.ToString()));
            }
            else
            {
                queryTrans = queryTrans.And(q => q.DepartmentId == null);
            }

            if (!string.IsNullOrEmpty(criteria.GroupId))
            {
                queryTrans = queryTrans.And(q => criteria.GroupId.Contains(q.GroupId.ToString()));
            }
            else
            {
                queryTrans = queryTrans.And(q => q.GroupId == null);
            }

            if (!string.IsNullOrEmpty(criteria.PersonInCharge))
            {
                queryTrans = queryTrans.And(q => criteria.PersonInCharge.Contains(q.PersonIncharge));
            }

            if (!string.IsNullOrEmpty(criteria.SalesMan))
            {
                queryTranDetail = (queryTranDetail == null) ?
                    (q => criteria.SalesMan.Contains(q.SaleManId))
                    :
                    queryTranDetail.And(q => criteria.SalesMan.Contains(q.SaleManId));
            }

            if (!string.IsNullOrEmpty(criteria.Creator))
            {
                queryTrans = queryTrans.And(q => criteria.Creator.Contains(q.UserCreated));
            }

            if (!string.IsNullOrEmpty(criteria.CarrierId))
            {
                queryTrans = queryTrans.And(q => q.ColoaderId == criteria.CarrierId);
            }

            if (!string.IsNullOrEmpty(criteria.AgentId))
            {
                queryTrans = queryTrans.And(q => q.AgentId == criteria.AgentId);
            }

            if (criteria.Pol != null && criteria.Pol != Guid.Empty)
            {
                queryTrans = queryTrans.And(q => q.Pol == criteria.Pol);
            }

            if (criteria.Pod != null && criteria.Pod != Guid.Empty)
            {
                queryTrans = queryTrans.And(q => q.Pod == criteria.Pod);
            }

            var masterBills = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false).Where(queryTrans);

            if (queryTranDetail == null)
            {
                var houseBills = detailRepository.Get();
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId into housebill
                                    from house in housebill.DefaultIfEmpty()
                                    select new GeneralExportShipmentOverviewResult
                                    {
                                        ServiceName = master.TransactionType,
                                        JobNo = master.JobNo,
                                        etd = master.Etd,
                                        eta = master.Eta,
                                        FlightNo = master.FlightVesselName,
                                        MblMawb = master.Mawb,
                                        HblHawb = house.Hwbno,
                                        Pol = master.Pol,
                                        Pod = master.Pod,
                                        ShipmentType = master.ShipmentType,
                                        Salesman = house.SaleManId,
                                        Carrier = master.ColoaderId,
                                        Agent = master.AgentId,
                                        Shipper = house.ShipperId,
                                        Consignee = house.ConsigneeId,
                                        PackageType = house.PackageType,
                                        QTy = house.PackageQty,
                                        Cont20 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "xCont20").Count : 0,
                                        Cont40 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "xCont40").Count : 0,
                                        Cont40HC = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "xCont40HC").Count : 0,
                                        Cont45 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "xCont45").Count : 0,
                                        GW = master.GrossWeight,
                                        CW = master.ChargeWeight,
                                        CBM = house.Cbm.HasValue ? house.Cbm : master.Cbm,
                                        HblId = house.Id,
                                        CustomerId = house.CustomerId,
                                        OfficeId = master.OfficeId,
                                        Creator = master.UserCreated,
                                        POINV = master.Pono,
                                        Commodity = master.Commodity,
                                        PMTerm = master.PaymentTerm,
                                        ShipmentNotes = master.Notes,
                                        Created = master.DatetimeCreated

                                        
                                    };
                return queryShipment;
            }
            else
            {
                var houseBills = detailRepository.Get().Where(queryTranDetail);
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new GeneralExportShipmentOverviewResult
                                    {
                                        ServiceName = master.TransactionType,
                                        JobNo = master.JobNo,
                                        etd = master.Etd,
                                        eta = master.Eta,
                                        FlightNo = master.FlightVesselName,
                                        MblMawb = master.Mawb,
                                        HblHawb = house.Hwbno,
                                        Pol = master.Pol,
                                        Pod = master.Pod,
                                        ShipmentType = master.ShipmentType,
                                        Salesman = house.SaleManId,
                                        Carrier = master.ColoaderId,
                                        Agent = master.AgentId,
                                        Shipper = house.ShipperId,
                                        Consignee = house.ConsigneeId,
                                        PackageType = house.PackageType,
                                        QTy = house.PackageQty,
                                        Cont20 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "xCont20").Count : 0,
                                        Cont40 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "xCont40").Count : 0,
                                        Cont40HC = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "xCont40HC").Count : 0,
                                        Cont45 = !string.IsNullOrEmpty(house.PackageContainer) ? Regex.Matches(house.PackageContainer, "xCont45").Count : 0,
                                        GW = master.GrossWeight,
                                        CW = master.ChargeWeight,
                                        CBM = house.Cbm.HasValue ? house.Cbm : master.Cbm,
                                        HblId = house.Id,
                                        CustomerId = house.CustomerId,
                                        OfficeId = master.OfficeId,
                                        Creator = master.UserCreated,
                                        POINV = master.Pono,
                                        Commodity = master.Commodity,
                                        PMTerm = master.PaymentTerm,
                                        ShipmentNotes = master.Notes,
                                        Created = master.DatetimeCreated
                                    };
                return queryShipment;
            }
        }

        #endregion

        #region -- GENERAL REPORT --

        public List<GeneralReportResult> GetDataGeneralReport(GeneralReportCriteria criteria, int page, int size, out int rowsCount)
        {
            var dataDocumentation = GeneralReportDocumentation(criteria);
            IQueryable<GeneralReportResult> list;
            if (criteria.Service.Contains("CL"))
            {
                var dataOperation = GeneralReportOperation(criteria);
                list = dataDocumentation.Union(dataOperation);
            }
            else
            {
                list = dataDocumentation;
            }

            var results = new List<GeneralReportResult>();
            if (list == null)
            {
                rowsCount = 0;
                return results;
            }
            var tempList = list.ToList();
            int no = 1;
            tempList.ForEach(fe =>
            {
                fe.No = no;
                no++;
            });
            rowsCount = tempList.Select(s => s.No).Count();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = tempList.Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }

        private IQueryable<OpsTransaction> QueryDataOperation(GeneralReportCriteria criteria)
        {
            var shipments = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false);
            Expression<Func<OpsTransaction, bool>> query = q => true;
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                query = q =>
                    q.ServiceDate.HasValue ? q.ServiceDate.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date : false;
            }
            else
            {
                query = q =>
                    q.DatetimeCreated.HasValue ? q.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && q.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date : false;
            }

            if (!string.IsNullOrEmpty(criteria.CustomerId))
            {
                query = query.And(q => q.CustomerId == criteria.CustomerId);
            }

            query = query.And(q => criteria.Service.Contains("CL"));

            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                query = query.And(q => q.JobNo == criteria.JobId);
            }

            if (!string.IsNullOrEmpty(criteria.Mawb))
            {
                query = query.And(q => q.Mblno == criteria.Mawb);
            }

            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                query = query.And(q => q.Hwbno == criteria.Hawb);
            }

            if (!string.IsNullOrEmpty(criteria.OfficeId))
            {
                query = query.And(q => criteria.OfficeId.Contains(q.OfficeId.ToString()));
            }
            else
            {
                query = query.And(q => q.OfficeId == Guid.Empty);
            }

            if (!string.IsNullOrEmpty(criteria.DepartmentId))
            {
                query = query.And(q => criteria.DepartmentId.Contains(q.DepartmentId.ToString()));
            }
            else
            {
                query = query.And(q => q.DepartmentId == null);
            }

            if (!string.IsNullOrEmpty(criteria.GroupId))
            {
                query = query.And(q => criteria.GroupId.Contains(q.GroupId.ToString()));
            }
            else
            {
                query = query.And(q => q.GroupId == null);
            }

            if (!string.IsNullOrEmpty(criteria.PersonInCharge))
            {
                query = query.And(q => criteria.PersonInCharge.Contains(q.BillingOpsId));
            }

            if (!string.IsNullOrEmpty(criteria.SalesMan))
            {
                query = query.And(q => criteria.SalesMan.Contains(q.SalemanId));
            }

            if (!string.IsNullOrEmpty(criteria.Creator))
            {
                query = query.And(q => criteria.Creator.Contains(q.UserCreated));
            }

            if (!string.IsNullOrEmpty(criteria.CarrierId))
            {
                query = query.And(q => q.SupplierId == criteria.CarrierId);
            }

            if (!string.IsNullOrEmpty(criteria.AgentId))
            {
                query = query.And(q => q.AgentId == criteria.AgentId);
            }

            if (criteria.Pol != null && criteria.Pol != Guid.Empty)
            {
                query = query.And(q => q.Pol == criteria.Pol);
            }

            if (criteria.Pod != null && criteria.Pod != Guid.Empty)
            {
                query = query.And(q => q.Pod == criteria.Pod);
            }
            var queryShipment = shipments.Where(query);
            return queryShipment;
        }

        private IQueryable<GeneralReportResult> GeneralReportOperation(GeneralReportCriteria criteria)
        {
            List<GeneralReportResult> dataList = new List<GeneralReportResult>();
            var dataShipment = QueryDataOperation(criteria);
            foreach (var item in dataShipment)
            {
                GeneralReportResult data = new GeneralReportResult();
                data.JobId = item.JobNo;
                data.Mawb = item.Mblno;
                data.Hawb = item.Hwbno;
                data.CustomerName = catPartnerRepo.Get(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn;
                data.CarrierName = catPartnerRepo.Get(x => x.Id == item.SupplierId).FirstOrDefault()?.PartnerNameEn;
                data.AgentName = catPartnerRepo.Get(x => x.Id == item.AgentId).FirstOrDefault()?.PartnerNameEn;
                data.ServiceDate = item.ServiceDate;

                var _polCode = catPlaceRepo.Get(x => x.Id == item.Pol).FirstOrDefault()?.Code;
                var _podCode = catPlaceRepo.Get(x => x.Id == item.Pod).FirstOrDefault()?.Code;
                data.Route = _polCode + "/" + _podCode;

                data.Qty = item.SumPackages ?? 0;

                #region -- Phí Selling trước thuế --
                decimal _revenue = 0;
                var _chargeSell = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == item.Hblid);
                foreach (var charge in _chargeSell)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _revenue += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                data.Revenue = _revenue;
                #endregion -- Phí Selling trước thuế --

                #region -- Phí Buying trước thuế --
                decimal _cost = 0;
                var _chargeBuy = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.Hblid == item.Hblid);
                foreach (var charge in _chargeBuy)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _cost += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                data.Cost = _cost;
                #endregion -- Phí Buying trước thuế --

                data.Profit = data.Revenue - data.Cost;

                #region -- Phí OBH sau thuế --
                decimal _obh = 0;
                var _chargeObh = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE && x.Hblid == item.Hblid);
                foreach (var charge in _chargeObh)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _obh += charge.Total * _rate; // Phí OBH sau thuế
                }
                data.Obh = _obh;
                #endregion -- Phí OBH sau thuế --

                var _empPic = sysUserRepo.Get(j => j.Id == item.BillingOpsId).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empPic))
                {
                    data.PersonInCharge = sysEmployeeRepo.Get(x => x.Id == _empPic).FirstOrDefault()?.EmployeeNameEn;
                }

                var _empSale = sysUserRepo.Get(j => j.Id == item.SalemanId).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empSale))
                {

                    data.Salesman = sysEmployeeRepo.Get(x => x.Id == _empSale).FirstOrDefault()?.EmployeeNameEn;
                }

                data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == "CL").FirstOrDefault()?.DisplayName;

                dataList.Add(data);
            }
            return dataList.AsQueryable();
        }

        private IQueryable<GeneralReportResult> QueryDataDocumentation(GeneralReportCriteria criteria)
        {
            Expression<Func<CsTransaction, bool>> queryTrans;
            Expression<Func<CsTransactionDetail, bool>> queryTranDetail = null;
            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                queryTrans = q =>
                    q.TransactionType.Contains("E") ?
                    (q.Etd.HasValue ? q.Etd.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Etd.Value.Date <= criteria.ServiceDateTo.Value.Date : false)
                    :
                    (q.Eta.HasValue ? q.Eta.Value.Date >= criteria.ServiceDateFrom.Value.Date && q.Eta.Value.Date <= criteria.ServiceDateTo.Value.Date : false);
            }
            else
            {
                queryTrans = q =>
                    q.DatetimeCreated.HasValue ? q.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && q.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date : false;
            }

            if (!string.IsNullOrEmpty(criteria.CustomerId))
            {
                queryTranDetail = q => q.CustomerId == criteria.CustomerId;
            }

            if (!string.IsNullOrEmpty(criteria.Service))
            {
                queryTrans = queryTrans.And(q => criteria.Service.Contains(q.TransactionType));
            }
            else
            {
                queryTrans = queryTrans.And(q => q.TransactionType == criteria.Service);
            }

            if (!string.IsNullOrEmpty(criteria.JobId))
            {
                queryTrans = queryTrans.And(q => q.JobNo == criteria.JobId);
            }

            if (!string.IsNullOrEmpty(criteria.Mawb))
            {
                queryTrans = queryTrans.And(q => q.Mawb == criteria.Mawb);
            }

            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                queryTranDetail = queryTranDetail == null ?
                    (q => q.Hwbno == criteria.Hawb)
                    :
                    queryTranDetail.And(q => q.Hwbno == criteria.Hawb);
            }

            if (!string.IsNullOrEmpty(criteria.OfficeId))
            {
                queryTrans = queryTrans.And(q => criteria.OfficeId.Contains(q.OfficeId.ToString()));
            }
            else
            {
                queryTrans = queryTrans.And(q => q.OfficeId == Guid.Empty);
            }

            if (!string.IsNullOrEmpty(criteria.DepartmentId))
            {
                queryTrans = queryTrans.And(q => criteria.DepartmentId.Contains(q.DepartmentId.ToString()));
            }
            else
            {
                queryTrans = queryTrans.And(q => q.DepartmentId == null);
            }

            if (!string.IsNullOrEmpty(criteria.GroupId))
            {
                queryTrans = queryTrans.And(q => criteria.GroupId.Contains(q.GroupId.ToString()));
            }
            else
            {
                queryTrans = queryTrans.And(q => q.GroupId == null);
            }

            if (!string.IsNullOrEmpty(criteria.PersonInCharge))
            {
                queryTrans = queryTrans.And(q => criteria.PersonInCharge.Contains(q.PersonIncharge));
            }

            if (!string.IsNullOrEmpty(criteria.SalesMan))
            {
                queryTranDetail = (queryTranDetail == null) ?
                    (q => criteria.SalesMan.Contains(q.SaleManId))
                    :
                    queryTranDetail.And(q => criteria.SalesMan.Contains(q.SaleManId));
            }

            if (!string.IsNullOrEmpty(criteria.Creator))
            {
                queryTrans = queryTrans.And(q => criteria.Creator.Contains(q.UserCreated));
            }

            if (!string.IsNullOrEmpty(criteria.CarrierId))
            {
                queryTrans = queryTrans.And(q => q.ColoaderId == criteria.CarrierId);
            }

            if (!string.IsNullOrEmpty(criteria.AgentId))
            {
                queryTrans = queryTrans.And(q => q.AgentId == criteria.AgentId);
            }

            if (criteria.Pol != null && criteria.Pol != Guid.Empty)
            {
                queryTrans = queryTrans.And(q => q.Pol == criteria.Pol);
            }

            if (criteria.Pod != null && criteria.Pod != Guid.Empty)
            {
                queryTrans = queryTrans.And(q => q.Pod == criteria.Pod);
            }

            var masterBills = DataContext.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false).Where(queryTrans);
            if (queryTranDetail == null)
            {
                var houseBills = detailRepository.Get();
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId into housebill
                                    from house in housebill.DefaultIfEmpty()
                                    select new GeneralReportResult
                                    {
                                        JobId = master.JobNo,
                                        Mawb = master.Mawb,
                                        HblId = house.Id,
                                        Hawb = house.Hwbno,
                                        CustomerId = house.CustomerId,
                                        CarrierId = master.ColoaderId,
                                        AgentId = master.AgentId,
                                        Qty = master.PackageQty ?? 0,
                                        Pol = master.Pol,
                                        Pod = master.Pod,
                                        ServiceDate = master.ServiceDate,
                                        PicId = master.PersonIncharge,
                                        SalesmanId = house.SaleManId,
                                        Service = master.TransactionType
                                    };
                return queryShipment;
            }
            else
            {
                var houseBills = detailRepository.Get().Where(queryTranDetail);
                var queryShipment = from master in masterBills
                                    join house in houseBills on master.Id equals house.JobId
                                    select new GeneralReportResult
                                    {
                                        JobId = master.JobNo,
                                        Mawb = master.Mawb,
                                        HblId = house.Id,
                                        Hawb = house.Hwbno,
                                        CustomerId = house.CustomerId,
                                        CarrierId = master.ColoaderId,
                                        AgentId = master.AgentId,
                                        Qty = master.PackageQty ?? 0,
                                        Pol = master.Pol,
                                        Pod = master.Pod,
                                        ServiceDate = master.ServiceDate,
                                        PicId = master.PersonIncharge,
                                        SalesmanId = house.SaleManId,
                                        Service = master.TransactionType
                                    };
                return queryShipment;
            }

        }

        private IQueryable<GeneralReportResult> GeneralReportDocumentation(GeneralReportCriteria criteria)
        {
            var dataShipment = QueryDataDocumentation(criteria);
            List<GeneralReportResult> dataList = new List<GeneralReportResult>();
            foreach (var item in dataShipment)
            {
                GeneralReportResult data = new GeneralReportResult();
                data.JobId = item.JobId;
                data.Mawb = item.Mawb;
                data.Hawb = item.Hawb;
                data.CustomerName = catPartnerRepo.Get(x => x.Id == item.CustomerId).FirstOrDefault()?.PartnerNameEn;
                data.CarrierName = catPartnerRepo.Get(x => x.Id == item.CarrierId).FirstOrDefault()?.PartnerNameEn;
                data.AgentName = catPartnerRepo.Get(x => x.Id == item.AgentId).FirstOrDefault()?.PartnerNameEn;
                data.ServiceDate = item.ServiceDate;

                var _polCode = catPlaceRepo.Get(x => x.Id == item.Pol).FirstOrDefault()?.Code;
                var _podCode = catPlaceRepo.Get(x => x.Id == item.Pod).FirstOrDefault()?.Code;
                data.Route = _polCode + "/" + _podCode;

                data.Qty = item.Qty;

                #region -- Phí Selling trước thuế --
                decimal _revenue = 0;
                var _chargeSell = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.Hblid == item.HblId);
                foreach (var charge in _chargeSell)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _revenue += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                data.Revenue = _revenue;
                #endregion -- Phí Selling trước thuế --

                #region -- Phí Buying trước thuế --
                decimal _cost = 0;
                var _chargeBuy = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.Hblid == item.HblId);
                foreach (var charge in _chargeBuy)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _cost += charge.Quantity * charge.UnitPrice * _rate ?? 0; // Phí Selling trước thuế
                }
                data.Cost = _cost;
                #endregion -- Phí Buying trước thuế --

                data.Profit = data.Revenue - data.Cost;

                #region -- Phí OBH sau thuế --
                decimal _obh = 0;
                var _chargeObh = surCharge.Get(x => x.Type == DocumentConstants.CHARGE_OBH_TYPE && x.Hblid == item.HblId);
                foreach (var charge in _chargeObh)
                {
                    //Tỉ giá quy đổi theo ngày FinalExchangeRate, nếu FinalExchangeRate là null thì quy đổi theo ngày ExchangeDate
                    var _rate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, criteria.Currency);
                    _obh += charge.Total * _rate; // Phí OBH sau thuế
                }
                data.Obh = _obh;
                #endregion -- Phí OBH sau thuế --

                var _empPic = sysUserRepo.Get(j => j.Id == item.PicId).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empPic))
                {
                    data.PersonInCharge = sysEmployeeRepo.Get(x => x.Id == _empPic).FirstOrDefault()?.EmployeeNameEn;
                }

                var _empSale = sysUserRepo.Get(j => j.Id == item.SalesmanId).FirstOrDefault()?.EmployeeId;
                if (!string.IsNullOrEmpty(_empSale))
                {

                    data.Salesman = sysEmployeeRepo.Get(x => x.Id == _empSale).FirstOrDefault()?.EmployeeNameEn;
                }

                data.ServiceName = API.Common.Globals.CustomData.Services.Where(x => x.Value == item.Service).FirstOrDefault()?.DisplayName;

                dataList.Add(data);
            }
            return dataList.AsQueryable();
        }
        #endregion -- GENERAL REPORT --      
    }
}
