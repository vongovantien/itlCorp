using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class ShipmentService : IShipmentService
    {
        readonly IContextBase<CsTransaction> csRepository;
        readonly IContextBase<OpsTransaction> opsRepository;
        readonly IContextBase<CsTransactionDetail> detailRepository;
        readonly IContextBase<CsShipmentSurcharge> surCharge;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IMapper mapper;
        readonly IContextBase<OpsStageAssigned> opsStageAssignedRepo;
        private readonly ICurrentUser currentUser;
        public ShipmentService(IContextBase<CsTransaction> cs, IContextBase<OpsTransaction> ops, IMapper iMapper, IContextBase<CsTransactionDetail> detail, IContextBase<CsShipmentSurcharge> surcharge, IContextBase<CatPartner> catPartner, IContextBase<OpsStageAssigned> opsStageAssigned, ICurrentUser user)
        {
            csRepository = cs;
            opsRepository = ops;
            mapper = iMapper;
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
            var shipmentsOperation = from ops in opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != "Canceled" && x.IsLocked == false)
                                     join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId into osa2
                                     from osa in osa2.DefaultIfEmpty()
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
            //End change request
            var transactions = csRepository.Get(x => x.CurrentStatus != "Canceled" && x.IsLocked == false);
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

        public IQueryable<Shipments> GetShipmentsCreditPayer(string partner, List<string> services)
        {
            //Chỉ lấy ra những phí Credit(BUY) & Payer (chưa bị lock)
            var surcharge = surCharge.Get(x =>
                    (x.Type == Common.Constants.CHARGE_BUY_TYPE || (x.PayerId != null && x.CreditNo != null))
                && (x.PayerId == partner || x.PaymentObjectId == partner)
            );

            var transactions = csRepository.Get(x => x.IsLocked == false && services.Contains(x.TransactionType));
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
                var shipmentOperation = opsRepository.Get(x => x.IsLocked == false && x.CurrentStatus != "Canceled");
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
            var cstran = csRepository.Get();
            var cstrandel = detailRepository.Get();
            //var opstran = opsRepository.Get(x => x.CurrentStatus != "Canceled");

            //Start change request Modified 14/10/2019 by Andy.Hoa
            //Get list shipment operation theo user current
            var opstran = from ops in opsRepository.Get(x => x.CurrentStatus != "Canceled")
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
        private IQueryable<Shipments> GetShipmentToUnLock(ShipmentCriteria criteria) {
            IQueryable<Shipments> opShipments = null;
            string transactionType = string.Empty;
            if (criteria.TransactionType == TransactionTypeEnum.CustomLogistic || criteria.TransactionType == 0)
            {
                opShipments = opsRepository.Get(x => ((criteria.ShipmentPropertySearch == ShipmentPropertySearch.JOBID ? criteria.Keywords.Contains(x.JobNo) : true
                                                      && criteria.ShipmentPropertySearch == ShipmentPropertySearch.MBL ? criteria.Keywords.Contains(x.Mblno) : true)
                                                        || criteria.Keywords == null)
                                                      &&
                                                        (
                                                                (((x.ServiceDate ?? null) >= (criteria.FromDate ?? null)) && ((x.ServiceDate ?? null) <= (criteria.ToDate ?? null)))
                                                            || (criteria.FromDate == null && criteria.ToDate == null)
                                                        )
                                                      )
                    .Select(x => new Shipments
                    {
                        MBL = x.Mblno,
                        JobId = x.JobNo,
                        Id = x.Id,
                        Service = "OPS"
                    });
                if(criteria.TransactionType == TransactionTypeEnum.CustomLogistic)
                {
                    return opShipments;
                }
            }
            else
            {
                transactionType = DataTypeEx.GetType(criteria.TransactionType);
            }
            var csTransactions = csRepository.Get(x => ((criteria.ShipmentPropertySearch == ShipmentPropertySearch.JOBID ? criteria.Keywords.Contains(x.JobNo) : true
                                                 && criteria.ShipmentPropertySearch == ShipmentPropertySearch.MBL ? criteria.Keywords.Contains(x.Mawb) : true)
                                                    || criteria.Keywords == null)
                                                 && (x.TransactionType == transactionType || string.IsNullOrEmpty(transactionType) || criteria.TransactionType == TransactionTypeEnum.CustomLogistic)
                                              );
            csTransactions = GetShipmentServicesByTime(csTransactions, criteria);
            if (criteria.ShipmentPropertySearch == ShipmentPropertySearch.HBL)
            {
                var shipmentDetails = detailRepository.Get(x => criteria.Keywords.Contains(x.Hwbno));
                csTransactions = csTransactions.Join(shipmentDetails, x => x.Id, y => y.JobId, (x, y) => x);
            }
            var csShipments = csTransactions
                .Select(x => new Shipments
                {
                    MBL = x.Mawb,
                    JobId = x.JobNo,
                    Id = x.Id,
                    Service = "CS"
                });
            var shipments = opShipments.Union(csShipments);
            return shipments;
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

        public ResultHandle UnLockShipment(ShipmentCriteria criteria)
        {
            var shipments = GetShipmentToUnLock(criteria);
            var results = new List<string>();
            try
            {
                foreach (var item in shipments)
                {
                    if (item.Service == "OPS")
                    {
                        var opsShipment = opsRepository.Get(x => x.Id == item.Id)?.FirstOrDefault();
                        var logs = opsShipment.LockedLog!= null? opsShipment.LockedLog.Split(';').Where(x => x.Length > 0).ToList(): new List<string>();
                        if (opsShipment != null)
                        {
                            if (opsShipment.IsLocked == false)
                            {
                                opsShipment.IsLocked = true;
                                opsShipment.DatetimeModified = DateTime.Now;
                                opsShipment.UserModified = currentUser.UserID;
                                string log = opsShipment.JobNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + currentUser.UserName;
                                opsShipment.LockedLog = opsShipment.LockedLog + log + ";";
                                var isSuccessLockOps = opsRepository.Update(opsShipment, x => x.Id == opsShipment.Id);
                                if (isSuccessLockOps.Success == false)
                                {
                                    log = opsShipment.JobNo + " unlock failed " + isSuccessLockOps.Message;
                                }
                                if(log.Length > 0) logs.Add(log);
                            }
                            if(logs.Count > 0) results.AddRange(logs);
                        }
                    }
                    else
                    {
                        var csShipment = csRepository.Get(x => x.Id == item.Id)?.FirstOrDefault();
                        var logs = csShipment.LockedLog!= null? csShipment.LockedLog.Split(';').Where(x => x.Length > 0).ToList(): new List<string>();
                        if (csShipment != null)
                        {
                            if (csShipment.IsLocked == false)
                            {
                                csShipment.IsLocked = true;
                                csShipment.DatetimeModified = DateTime.Now;
                                csShipment.UserModified = currentUser.UserID;
                                string log = csShipment.JobNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + currentUser.UserName;
                                csShipment.LockedLog = csShipment.LockedLog + log + ";";
                                var isSuccessLockCs = csRepository.Update(csShipment, x => x.Id == csShipment.Id);
                                if (isSuccessLockCs.Success == false)
                                {
                                    log = csShipment.JobNo + " unlock failed " + isSuccessLockCs.Message;
                                }
                                if(log.Length > 0) logs.Add(log);
                            }
                            if(logs.Count > 0) results.AddRange(logs);
                        }
                    }
                }
                var result = new ResultHandle { Status = true, Data = results, Message = "Done" };
                return result;
            }
            catch (Exception ex)
            {
                return new ResultHandle { Status = false, Message = ex.Message };
            }
        }
    }
}
