using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatStandardChargeService : RepositoryBaseCache<CatStandardCharge, CatStandardChargeModel>, ICatStandardChargeService
    {
        private readonly ICurrentUser currentUser;
        private readonly ICatChargeService catChargeService;
        private readonly IContextBase<CatCharge> catChargeReposity;
        public CatStandardChargeService(IContextBase<CatStandardCharge> repository,
            ICacheServiceBase<CatStandardCharge> cacheService,
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user,
            IContextBase<CatCharge> catCharge,
            ICatChargeService charService) : base(repository, cacheService, mapper)
        {
            currentUser = user;
            catChargeService = charService;
            catChargeReposity = catCharge;

        }        
        public IQueryable<CatStandardChargeModel> GetBy(string type, string transactionType)
        {
            IQueryable<CatStandardCharge> standCharge = DataContext.Get(x => (x.Type == type && x.TransactionType == transactionType));
            if (standCharge == null) return null;
            IQueryable<CatStandardChargeModel> result = (
               from stcharge in standCharge
               join charge in catChargeReposity.Get() on stcharge.ChargeId equals charge.Id
               select new CatStandardChargeModel
               {
                   ChargeId = stcharge.ChargeId,
                   Code = charge.Code,
                   Quantity = stcharge.Quantity,
                   UnitPrice = stcharge.UnitPrice,
                   CurrencyId = stcharge.CurrencyId,
                   Vatrate = stcharge.Vatrate,
                   Type = stcharge.Type,
                   TransactionType = stcharge.TransactionType,
                   Service = stcharge.Service,
                   ServiceType = stcharge.ServiceType,
                   Office = stcharge.Office,
                   Notes = stcharge.Notes
               });
            return result;
        }
        public HandleState Import(List<CatStandardChargeImportModel> data)
        {
            try
            {
                foreach (var item in data)
                {
                    var standardCharge = new CatStandardCharge
                    {
                        Id = Guid.NewGuid(),
                        ChargeId = catChargeReposity.Get(x => x.Code == item.Code).FirstOrDefault()?.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        CurrencyId = item.CurrencyId,
                        Vatrate = item.Vatrate,
                        Type = item.Type,
                        TransactionType = item.TransactionType,
                        Service = item.Service,
                        ServiceType = item.ServiceType,
                        Office = item.Office,
                        Notes = item.Notes,
                        UserCreated = currentUser.UserID,
                        DatetimeCreated = DateTime.Now,
                        UserModified = currentUser.UserID,
                        DatetimeModified = DateTime.Now
                    };
                    DataContext.Add(standardCharge, false);
                }
                DataContext.SubmitChanges();
                ClearCache();
                Get();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        public HandleState DeleteStandard(Guid chargeId)
        {
                var hs = DataContext.Delete(x => (x.ChargeId == chargeId || x.ChargeId == null));
                if (hs.Success == true)
                {
                    ClearCache();
                    Get();
                }
                return hs;
        }
    }
}
