using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
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

        public ShipmentService(IContextBase<CsTransaction> repository, IMapper mapper,
            IContextBase<OpsTransaction> ops,
            IContextBase<CsTransactionDetail> detail,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<CatPartner> catPartner,
            IContextBase<OpsStageAssigned> opsStageAssigned,
            ICurrentUser user) : base(repository, mapper)
        {
            opsRepository = ops;
            detailRepository = detail;
            surCharge = surcharge;
            catPartnerRepo = catPartner;
            opsStageAssignedRepo = opsStageAssigned;
            currentUser = user;
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
            if(criteria.TransactionType > 0){ 

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
                 shipments =  opShipments.Union(csShipments);
            }
            else if( csShipments == null && opShipments != null)
            {
                 shipments = opShipments;
            }
            else if(opShipments == null && csShipments != null)
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
            result.LockedLogs = shipments.Where(x=>x.IsLocked == true);
            result.Logs = new List<string>();
            foreach(var item in shipments)
            {
                var logs = item.LockedLog != null ? item.LockedLog.Split(';').Where(x => x.Length > 0).ToList() : new List<string>();
                if(logs.Count > 0)
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
    }
}
