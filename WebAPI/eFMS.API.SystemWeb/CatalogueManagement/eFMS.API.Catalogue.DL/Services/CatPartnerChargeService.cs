using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPartnerChargeService : RepositoryBase<CatPartnerCharge, CatPartnerChargeModel>, ICatPartnerChargeService
    {
        private ICurrentUser currentUser;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        public CatPartnerChargeService(IContextBase<CatPartnerCharge> repository,
            IMapper mapper,
            IContextBase<CatPartner> catPartnerRepo,
            ICurrentUser currtUser) : base(repository, mapper)
        {
            currentUser = currtUser;
            catPartnerRepository = catPartnerRepo;
        }

        public HandleState AddAndUpdate(string partnerId, List<CatPartnerChargeModel> charges)
        {
            DataContext.Delete(x => x.PartnerId == partnerId, false);
            charges.ForEach(x =>
            {
                x.PartnerId = partnerId;
                x.Id = Guid.NewGuid();
                x.DatetimeModified = DateTime.Now;
                x.UserModified = currentUser.UserID;
            });
            HandleState hs = Add(charges, false);
            if (hs.Success)
            {
                DataContext.SubmitChanges();
                SubmitChanges();
            }
            return hs;
        }

        public IQueryable<CatPartnerChargeModel> GetBy(string partnerId)
        {
            IQueryable<CatPartnerCharge> data = null;
            CatPartner partner = catPartnerRepository.Get(x => x.Id == partnerId)?.FirstOrDefault();

            data = DataContext.Get(x => x.PartnerId == partnerId);
            IQueryable<CatPartnerChargeModel> catPartnerChargeModel = data?.Select(x => mapper.Map<CatPartnerChargeModel>(x));

            IQueryable<CatPartnerChargeModel> result = catPartnerChargeModel.Select(x => new CatPartnerChargeModel
            {
                partnerName = partner.PartnerNameEn,
                partnerShortName = partner.ShortName,
                PartnerId = x.PartnerId,
                Id = x.Id,
                UnitId = x.UnitId,
                ChargeId = x.ChargeId,
                Quantity = x.Quantity,
                QuantityType = x.QuantityType,
                UnitPrice = x.UnitPrice,
                CurrencyId = x.CurrencyId,
                Vatrate = x.Vatrate,
                UserModified = x.UserModified,
                DatetimeModified = x.DatetimeModified,
            });


            return result;
        }
    }
}
