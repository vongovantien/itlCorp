using AutoMapper;
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
        public AccAccountReceivableService(
            IContextBase<AccAccountReceivable> repository,
            ICsShipmentSurchargeService surchargeService,
            IMapper mapper
            ) : base(repository, mapper)
        {
            csShipmentSurchargeService = surchargeService;
        }

        public List<ObjectReceivableModel> GetListObjectReceivableBySurchargeIds(List<Guid> Ids)
        {
            var surcharges = csShipmentSurchargeService.Get(x => Ids.Any(a => a == x.Id));
            if (surcharges.Count() == 0)
            {
                return new List<ObjectReceivableModel>();
            }
            var objPO = from surcharge in surcharges
                        where !string.IsNullOrEmpty(surcharge.PaymentObjectId)
                        select new ObjectReceivableModel { PartnerId = surcharge.PaymentObjectId, Office = surcharge.OfficeId, Service = surcharge.TransactionType };
            var objPR = from surcharge in surcharges
                        where !string.IsNullOrEmpty(surcharge.PayerId)
                        select new ObjectReceivableModel { PartnerId = surcharge.PayerId, Office = surcharge.OfficeId, Service = surcharge.TransactionType };

            var objMerge = objPO.Union(objPR).ToList();
            var objectReceivables = objMerge.GroupBy(g => new { g.Service, g.PartnerId, g.Office })
                .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Service = s.Key.Service, Office = s.Key.Office });
            return objectReceivables.ToList();
        }
    }
}
