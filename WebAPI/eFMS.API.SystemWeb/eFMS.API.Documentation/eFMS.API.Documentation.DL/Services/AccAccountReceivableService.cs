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
        private readonly IContextBase<CsShipmentSurcharge> csShipmentSurchargeRepository;
        private readonly IContextBase<CsTransactionDetail> csTranDetailRepository;
        private readonly IContextBase<OpsTransaction> opsTranRepository;
        private readonly IContextBase<SysOffice> officeRepository;
        readonly Guid? HM = Guid.Empty;
        readonly Guid? BH = Guid.Empty;

        public AccAccountReceivableService(
            IContextBase<AccAccountReceivable> repository,
            IContextBase<CsShipmentSurcharge> csShipmentSurchargeRepo,
            IContextBase<CsTransactionDetail> csTranDetailRepo,
            IContextBase<OpsTransaction> opsTranRepo,
            IContextBase<SysOffice> officeRepo,
            IMapper mapper
            ) : base(repository, mapper)
        {
            csShipmentSurchargeRepository = csShipmentSurchargeRepo;
            csTranDetailRepository = csTranDetailRepo;
            opsTranRepository = opsTranRepo;
            officeRepository = officeRepo;

            HM = officeRepository.Get(x => x.Code == DocumentConstants.OFFICE_HM)?.FirstOrDefault()?.Id;
            BH = officeRepository.Get(x => x.Code == DocumentConstants.OFFICE_BH)?.FirstOrDefault()?.Id;

        }

        public List<ObjectReceivableModel> GetListObjectReceivableBySurchargeIds(List<Guid> Ids)
        {
            var surcharges = csShipmentSurchargeRepository.Get(x => Ids.Any(a => a == x.Id));
            return GetListObjectReceivable(surcharges);
        }

        public List<ObjectReceivableModel> GetListObjectReceivableBySurcharges(IQueryable<CsShipmentSurcharge> surcharges)
        {
            return GetListObjectReceivable(surcharges);
        }

        private List<ObjectReceivableModel> GetListObjectReceivable(IQueryable<CsShipmentSurcharge> surcharges)
        {
            surcharges = surcharges.Where(x => x.Type != DocumentConstants.CHARGE_BUY_TYPE && x.OfficeId != HM && x.OfficeId != BH);
            if (surcharges.Count() == 0)
            {
                return new List<ObjectReceivableModel>();
            }
            var objectReceivables = surcharges.GroupBy(g => new { Service = g.TransactionType, PartnerId = g.PaymentObjectId, Office = g.OfficeId })
                .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Service = s.Key.Service, Office = s.Key.Office });
            return objectReceivables.ToList();
        }
    }
}
