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
        readonly IMapper mapper;
        public ShipmentService(IContextBase<CsTransaction> cs, IContextBase<OpsTransaction> ops, IMapper iMapper, IContextBase<CsTransactionDetail> detail) 
        {
            csRepository = cs;
            opsRepository = ops;
            mapper = iMapper;
            detailRepository = detail;
        }

        public IQueryable<Shipments> GetShipmentNotLocked()
        {
            var shipmentsOperation = opsRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != "Canceled" && x.IsLocked == false)
                                    .Select(x => new Shipments
                                    {
                                        Id = x.Id,
                                        JobId = x.JobNo,
                                        HBL = x.Hwbno,
                                        MBL = x.Mblno
                                    });
            var transactions = csRepository.Get(x => x.IsLocked == false);
            var shipmentsDocumention = transactions.Join(detailRepository.Get(), x => x.Id, y => y.JobId, (x, y) => new { x, y }).Select(x => new Shipments
            {
                Id = x.x.Id,
                JobId = x.x.JobNo,
                HBL = x.y.Hwbno,
                MBL = x.y.Mawb,
            });
            var shipments = shipmentsOperation.Union(shipmentsDocumention);
            return shipments;
        }
    }
}
