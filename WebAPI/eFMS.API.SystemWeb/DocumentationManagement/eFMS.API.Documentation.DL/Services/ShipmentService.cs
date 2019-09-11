using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class ShipmentService: IShipmentService
    {
        readonly IContextBase<CsTransaction> csRepository;
        readonly IContextBase<OpsTransaction> opsRepository;
        readonly IContextBase<CsTransactionDetail> detailRepository;
        readonly IContextBase<CsShipmentSurcharge> surCharge;
        readonly IMapper mapper;
        public ShipmentService(IContextBase<CsTransaction> cs, IContextBase<OpsTransaction> ops, IMapper iMapper, IContextBase<CsTransactionDetail> detail, IContextBase<CsShipmentSurcharge> surcharge) 
        {
            csRepository = cs;
            opsRepository = ops;
            mapper = iMapper;
            detailRepository = detail;
            surCharge = surcharge;
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

        public IQueryable<Shipments> GetShipmentsCreditPayer()
        {
            //Chỉ lấy ra những phí Credit(BUY) & Payer
            var surcharge = surCharge.Get();

            var shipmentOperation = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != "Canceled");
            var shipmentsOperation = surcharge.Join(shipmentOperation, x => x.Hblid, y => y.Hblid, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.x.Id,
                JobId = x.y.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.y.Mblno,
            });

            var transactions = csRepository.Get();
            var shipmentDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.y.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.y.Mawb,
            });
            var shipmentsDocumention = surcharge.Join(shipmentDocumention, x => x.Hblid, y => y.Id, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.x.Id,
                JobId = x.y.JobId,
                HBL = x.y.HBL,
                MBL = x.y.MBL,
            });

            var shipments = shipmentsOperation.Union(shipmentsDocumention).Select(s => new Shipments { JobId = s.JobId, HBL = s.HBL, MBL = s.MBL });
            var shipmentsResult = shipments.GroupBy(x => new { x.JobId, x.HBL, x.MBL }).Select(s => new Shipments {
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL
            });
            return shipmentsResult;
        }
    }
}
