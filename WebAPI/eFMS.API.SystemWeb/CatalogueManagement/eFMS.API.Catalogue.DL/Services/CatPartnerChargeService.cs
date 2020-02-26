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
        public CatPartnerChargeService(IContextBase<CatPartnerCharge> repository, IMapper mapper, ICurrentUser currtUser) : base(repository, mapper)
        {
            currentUser = currtUser;
        }

        public HandleState AddAndUpdate(string partnerId, List<CatPartnerChargeModel> charges)
        {
            DataContext.Delete(x => x.PartnerId == partnerId, false);
            charges.ForEach(x => {
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
    }
}
