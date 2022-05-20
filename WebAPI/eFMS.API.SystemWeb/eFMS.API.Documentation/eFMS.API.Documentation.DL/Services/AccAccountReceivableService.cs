using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.ForPartner.DL.Models.Receivable;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;


namespace eFMS.API.Documentation.DL.Services
{
    public class AccAccountReceivableService : RepositoryBase<AccAccountReceivable, AccAccountReceivableModel>, IAccAccountReceivableService
    {
        private readonly ICsShipmentSurchargeService csShipmentSurchargeService;
        private readonly IContextBase<CsTransactionDetail> csTranDetailRepository;
        private readonly IContextBase<OpsTransaction> opsTranRepository;

        public AccAccountReceivableService(
            IContextBase<AccAccountReceivable> repository,
            ICsShipmentSurchargeService surchargeService,
            IContextBase<CsTransactionDetail> csTranDetailRepo,
            IContextBase<OpsTransaction> opsTranRepo,
            IMapper mapper
            ) : base(repository, mapper)
        {
            csShipmentSurchargeService = surchargeService;
            csTranDetailRepository = csTranDetailRepo;
            opsTranRepository = opsTranRepo;
        }

        public List<ObjectReceivableModel> GetListObjectReceivableBySurchargeIds(List<Guid> Ids)
        {
            var surcharges = csShipmentSurchargeService.Get(x => Ids.Any(a => a == x.Id));

            return GetListObjectReceivable(surcharges);
        }

        public List<ObjectReceivableModel> GetListObjectReceivableBySurcharges(IQueryable<CsShipmentSurcharge> surcharges)
        {
            return GetListObjectReceivable(surcharges);
        }

        private List<ObjectReceivableModel> GetListObjectReceivable(IQueryable<CsShipmentSurcharge> surcharges)
        {
            if (surcharges.Count() == 0)
            {
                return new List<ObjectReceivableModel>();
            }
            var objPO = Enumerable.Empty<ObjectReceivableModel>().AsQueryable();
            var objPR = Enumerable.Empty<ObjectReceivableModel>().AsQueryable();
            var surchargeOps = surcharges.Where(x => x.TransactionType == DocumentConstants.LG_SHIPMENT);
            var surchargeCs = surcharges.Where(x => x.TransactionType != DocumentConstants.LG_SHIPMENT);
            if (surchargeOps.Count() > 0)
            {
                var hblids = surchargeOps.Select(x => x.Hblid).ToList();
                var houseBills = opsTranRepository.Get(x => hblids.Contains(x.Hblid));
                objPO = from surcharge in surchargeOps
                        join csd in houseBills on surcharge.Hblid equals csd.Hblid
                        where !string.IsNullOrEmpty(surcharge.PaymentObjectId)
                        select new ObjectReceivableModel
                        {
                            PartnerId = surcharge.PaymentObjectId,
                            Office = surcharge.OfficeId,
                            Service = surcharge.TransactionType,
                            SalesmanId = csd.SalemanId
                        };
            }
            if (surchargeCs.Count() > 0)
            {
                var hblids = surchargeCs.Select(x => x.Hblid).ToList();
                var houseBills = csTranDetailRepository.Get(x => hblids.Contains(x.Id));

                objPR = from surcharge in surchargeCs
                        join csd in houseBills on surcharge.Hblid equals csd.Id
                        where !string.IsNullOrEmpty(surcharge.PaymentObjectId)
                        select new ObjectReceivableModel
                        {
                            PartnerId = surcharge.PaymentObjectId,
                            Office = surcharge.OfficeId,
                            Service = surcharge.TransactionType,
                            SalesmanId = csd.SaleManId
                        };
            }
            var objMerge = objPO.Union(objPR).ToList();

            var objectReceivables = objMerge.GroupBy(g => new { g.Service, g.PartnerId, g.Office, g.SalesmanId })
                .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Service = s.Key.Service, Office = s.Key.Office, SalesmanId = s.Key.SalesmanId });
            return objectReceivables.ToList();
        }
    }
}
