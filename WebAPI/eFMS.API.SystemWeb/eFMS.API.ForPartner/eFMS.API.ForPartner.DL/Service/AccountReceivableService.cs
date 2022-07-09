using AutoMapper;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Receivable;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.ForPartner.DL.Service
{
    public class AccountReceivableService : RepositoryBase<AccAccountReceivable, AccAccountReceivableModel>, IAccountReceivableService
    {
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        private readonly IContextBase<SysOffice> officeRepo;
        readonly Guid? HM = Guid.Empty;
        readonly Guid? BH = Guid.Empty;
        public AccountReceivableService(IContextBase<AccAccountReceivable> repository, 
            IMapper mapper, IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<SysOffice> office
            ) : base(repository, mapper)
        {
            surchargeRepository = surchargeRepo;
            officeRepo = office;

            HM = officeRepo.Get(x => x.Code == ForPartnerConstants.OFFICE_HM)?.FirstOrDefault()?.Id;
            BH = officeRepo.Get(x => x.Code == ForPartnerConstants.OFFICE_BH)?.FirstOrDefault()?.Id;
        }

        public List<ObjectReceivableModel> GetObjectReceivableBySurchargeId(List<Guid> surchargeIds)
        {
            var surcharges = surchargeRepository.Get(x => surchargeIds.Any(a => a == x.Id) && x.OfficeId != HM && x.OfficeId != BH);

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
