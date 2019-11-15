using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.QueryableExtensions;
using System.Linq.Expressions;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCurrencyService : RepositoryBase<CatCurrency, CatCurrencyModel>, ICatCurrencyService
    {
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;
        public CatCurrencyService(IContextBase<CatCurrency> repository, IMapper mapper, IDistributedCache distributedCache, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
            cache = distributedCache;
            SetChildren<CatCharge>("Id", "CurrencyId");
            SetChildren<CatCurrencyExchange>("Id", "CurrencyFromId");
            SetChildren<CatCurrencyExchange>("Id", "CurrencyToId");
            SetChildren<AcctSoa>("Id", "CurrencyId");
            SetChildren<CatCharge>("Id", "CurrencyId");
            SetChildren<CsShipmentSurcharge>("Id", "CurrencyId");
            SetChildren<AcctCdnote>("Id", "CurrencyId");
            SetChildren<AcctSoa>("Id", "Currency");
            SetChildren<CatCharge>("Id", "Currency");
        }

        #region CRUD
        public override HandleState Add(CatCurrencyModel entity)
        {
            var currency = mapper.Map<CatCurrency>(entity);
            currency.DatetimeCreated = entity.DatetimeModified = DateTime.Now;
            currency.Active = true;
            currency.UserCreated = currentUser.UserID;
            var result = DataContext.Add(currency, true);
            if (result.Success)
            {
                cache.Remove(Templates.CatCurrency.NameCaching.ListName);
                RedisCacheHelper.SetObject(cache, Templates.CatCurrency.NameCaching.ListName, DataContext.Get());
            }
            return result;
        }

        public HandleState Update(CatCurrencyModel model)
        {
            HandleState result = new HandleState();
            try
            {
                var entity = mapper.Map<CatCurrency>(model);
                entity.UserModified = currentUser.UserID;
                entity.DatetimeModified = DateTime.Now;
                if (entity.Active == false)
                {
                    entity.InactiveOn = DateTime.Now;
                }
                result = DataContext.Update(entity, x => x.Id == model.Id, false);
                if (result.Success)
                {
                    if (model.IsDefault)
                    {
                        var listDefaults = DataContext.Get(x => x.Id != model.Id && x.IsDefault == true);
                        foreach (var item in listDefaults)
                        {
                            item.IsDefault = false;
                            item.DatetimeModified = DateTime.Now;
                            DataContext.Update(item, x => x.Id == item.Id, false);
                        }
                    }
                }
                DataContext.SubmitChanges();
                cache.Remove(Templates.CatCurrency.NameCaching.ListName);
                RedisCacheHelper.SetObject(cache, Templates.CatCurrency.NameCaching.ListName, DataContext.Get());
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        public HandleState Delete(string id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                cache.Remove(Templates.CatCurrency.NameCaching.ListName);
                RedisCacheHelper.SetObject(cache, Templates.CatCurrency.NameCaching.ListName, DataContext.Get());
            }
            return hs;
        }
        #endregion

        public IQueryable<CatCurrencyModel> Paging(CatCurrrencyCriteria criteria, int pageNumber, int pageSize, out int rowsCount, out int totalPages)
        {
            var data = GetBy(criteria);
            IQueryable<CatCurrencyModel> results = null;
            if (data == null)
            {
                rowsCount = 0;
                totalPages = 0;
            }
            else
            {
                rowsCount = data.Count();
                data = data.OrderByDescending(x => x.DatetimeModified);
                totalPages = (rowsCount % pageSize == 0) ? rowsCount / pageSize : (rowsCount / pageSize) + 1;
                if (pageSize > 1)
                {
                    if (pageNumber < 1)
                    {
                        pageNumber = 1;
                    }
                    results = data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ProjectTo<CatCurrencyModel>(mapper.ConfigurationProvider);
                }
            }
            return results;
        }
        public IQueryable<CatCurrencyModel> Query(CatCurrrencyCriteria criteria)
        {
            IQueryable<CatCurrency> data = GetBy(criteria);
            if (data == null) return null;
            return data.ProjectTo<CatCurrencyModel>(mapper.ConfigurationProvider);
        }

        private IQueryable<CatCurrency> GetBy(CatCurrrencyCriteria criteria)
        {
            Expression<Func<CatCurrency, bool>> query;
            if (criteria.All == null)
            {
                query = (x => (x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                       && (x.CurrencyName ?? "").IndexOf(criteria.CurrencyName ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }
            else
            {
                query = (x => (x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.CurrencyName ?? "").IndexOf(criteria.CurrencyName ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }
            IQueryable<CatCurrency> data = RedisCacheHelper.Get<CatCurrency>(cache, Templates.CatCurrency.NameCaching.ListName);
            if(data == null)
            {
                data = DataContext.Get(query);
            }
            else
            {
                data = data.Where(query);
            }
            return data;
        }

        public IQueryable<CatCurrencyModel> GetAll()
        {
            IQueryable<CatCurrency> data = RedisCacheHelper.Get<CatCurrency>(cache, Templates.CatCurrency.NameCaching.ListName);
            if (data == null)
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.CatCurrency.NameCaching.ListName, DataContext.Get());
            }
            return data.ProjectTo<CatCurrencyModel>(mapper.ConfigurationProvider);
        }
    }
}
