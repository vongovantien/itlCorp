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
            var data = DataContext.Get(x =>(x.Type == type && x.TransactionType == transactionType));
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
                        ChargeId = catChargeReposity.Get(x => x.Code == item.Code).FirstOrDefault()?.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Currency = item.Currency,
                        VatRate = item.VatRate,
                        Type = item.Type,
                        TransactionType = item.TransactionType,
                        Service = item.Service,
                        ServiceType = item.ServiceType,
                        Office = item.Office,
                        Note = item.Note,
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
    }
}
