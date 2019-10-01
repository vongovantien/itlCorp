using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
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
        public ShipmentService(IContextBase<CsTransaction> cs, IContextBase<OpsTransaction> ops, IMapper iMapper, IContextBase<CsTransactionDetail> detail, IContextBase<CsShipmentSurcharge> surcharge, IContextBase<CatPartner> catPartner)
        {
            csRepository = cs;
            opsRepository = ops;
            mapper = iMapper;
            detailRepository = detail;
            surCharge = surcharge;
            catPartnerRepo = catPartner;
        }

        public IQueryable<Shipments> GetShipmentNotLocked()
        {
            var shipmentsOperation = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != "Canceled" && x.IsLocked == false)
                                    .Select(x => new Shipments
                                    {
                                        Id = x.Id,
                                        JobId = x.JobNo,
                                        HBL = x.Hwbno,
                                        MBL = x.Mblno,
                                        CustomerId = x.CustomerId,
                                        HBLID = x.Hblid
                                    });
            var transactions = csRepository.Get(x => x.IsLocked == false);
            var shipmentsDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.x.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.y.Mawb,
                CustomerId = x.y.CustomerId,
                HBLID = x.y.Id
            });
            var shipments = shipmentsOperation.Union(shipmentsDocumention);
            return shipments;
        }

        public IQueryable<Shipments> GetShipmentsCreditPayer(string partner, List<string> services)
        {
            //Chỉ lấy ra những phí Credit(BUY) & Payer
            var surcharge = surCharge.Get(x =>
                    (x.Type == "BUY" || (x.PayerId != null && x.CreditNo != null))
                && (x.PayerId == partner || x.PaymentObjectId == partner)
            );

            var transactions = csRepository.Get(x => services.Contains(x.TransactionType));
            var shipmentDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.y.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.y.Mawb,
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
                var shipmentOperation = opsRepository.Get(x => x.CurrentStatus != "Canceled");
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
            var dataList = new List<ShipmentsCopy>();

            if (string.IsNullOrEmpty(searchOption) || keywords == null || keywords.Count == 0 || keywords.Any(x => x == null)) return dataList;

            var surcharge = surCharge.Get();
            var cstran = csRepository.Get();
            var cstrandel = detailRepository.Get();
            var opstran = opsRepository.Get(x => x.CurrentStatus != "Canceled");
            var query = from sur in surcharge
                        join cstd in cstrandel on sur.Hblid equals cstd.Id into cstd2
                        from cstd in cstd2.DefaultIfEmpty()
                        join cst in cstran on cstd.JobId equals cst.Id into cst2
                        from cst in cst2.DefaultIfEmpty()
                        join ops in opstran on sur.Hblid equals ops.Hblid into ops2
                        from ops in ops2.DefaultIfEmpty()
                        join cus in catPartnerRepo.Get() on ((ops.CustomerId == null) ? cstd.CustomerId : ops.CustomerId) equals cus.Id into cus2
                        from cus in cus2.DefaultIfEmpty()
                        where
                                searchOption.Equals("JOBNO") ? keywords.Contains((ops.JobNo == null ? cst.JobNo : ops.JobNo)) : 1 == 1
                            &&
                                searchOption.Equals("HBL") ? keywords.Contains((ops.Hwbno == null ? cstd.Hwbno : ops.Hwbno)) : 1 == 1
                            &&
                                searchOption.Equals("MBL") ? keywords.Contains((ops.Mblno == null ? cstd.Mawb : ops.Mblno)) : 1 == 1
                            &&
                                searchOption.Equals("CUSTOMNO") ? keywords.Contains(sur.ClearanceNo) : 1 == 1
                        select new ShipmentsCopy {
                            JobId = (ops.JobNo == null ? cst.JobNo : ops.JobNo),
                            Customer = cus.ShortName,
                            HBL = (ops.Hwbno == null ? cstd.Hwbno : ops.Hwbno),
                            MBL = (ops.Mblno == null ? cstd.Mawb : ops.Mblno),
                            CustomNo = sur.ClearanceNo,
                            Service = (ops.JobNo == null ? cst.TransactionType : "CL"),
                        };

            var listShipment = query.Where(x => x.JobId != null && x.HBL != null && x.MBL != null)
                            .GroupBy(x => new { x.JobId, x.Customer, x.MBL, x.HBL, x.CustomNo, x.Service })
                            .Select(s => new ShipmentsCopy
                            {
                                JobId = s.Key.JobId,
                                Customer = s.Key.Customer,
                                MBL = s.Key.MBL,
                                HBL = s.Key.HBL,
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
                CustomNo = x.CustomNo,
                Service = CustomData.Services.Where(s => s.Value == x.Service).FirstOrDefault()?.DisplayName
            }).ToList();
            return dataList;
        }
    }
}
