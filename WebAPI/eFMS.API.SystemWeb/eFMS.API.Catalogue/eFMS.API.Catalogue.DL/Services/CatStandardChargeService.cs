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
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly ICatChargeService catChargeService;
        public CatStandardChargeService(IContextBase<CatStandardCharge> repository,
            ICacheServiceBase<CatStandardCharge> cacheService,
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user,
            ICatChargeService charService) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            catChargeService = charService;

        }        
        public IQueryable<CatStandardChargeModel> GetBy(string type)
        {
            var data = DataContext.Get(x => x.Type == type);
            if (data == null) return null;
            return data.ProjectTo<CatStandardChargeModel>(mapper.ConfigurationProvider);
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
                        ChargeId = item.ChargeId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Currency = item.Currency,
                        Type = item.Type,
                        TransactionType = item.TransactionType,
                        Service = item.Service,
                        ServiceType = item.ServiceType,
                        Note = item.Note

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
    }
}
