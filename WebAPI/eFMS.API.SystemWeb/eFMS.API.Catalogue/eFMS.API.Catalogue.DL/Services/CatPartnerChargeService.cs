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
        private readonly IContextBase<CatCharge> catChargeRepository;

        public CatPartnerChargeService(IContextBase<CatPartnerCharge> repository,
            IMapper mapper,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<CatCharge> catChargeRepo,
            ICurrentUser currtUser) : base(repository, mapper)
        {
            currentUser = currtUser;
            catPartnerRepository = catPartnerRepo;
            catChargeRepository = catChargeRepo;
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
            if (data == null) return null;

            IQueryable<CatPartnerChargeModel> results = from d in data
                                                        join charge in catChargeRepository.Get() on d.ChargeId equals charge.Id into chargeGr
                                                        from itemCharge in chargeGr.DefaultIfEmpty()
                                                        select new CatPartnerChargeModel
                                                        {
                                                            chargeNameEn = itemCharge.ChargeNameEn,
                                                            partnerName = partner.PartnerNameEn,
                                                            partnerShortName = partner.ShortName,
                                                            PartnerId = d.PartnerId,
                                                            Id = d.Id,
                                                            UnitId = d.UnitId,
                                                            ChargeId = d.ChargeId,
                                                            Quantity = d.Quantity,
                                                            QuantityType = d.QuantityType,
                                                            UnitPrice = d.UnitPrice,
                                                            CurrencyId = d.CurrencyId,
                                                            Vatrate = d.Vatrate,
                                                            UserModified = d.UserModified,
                                                            DatetimeModified = d.DatetimeModified,
                                                        };
            return results;
        }
    }
}
