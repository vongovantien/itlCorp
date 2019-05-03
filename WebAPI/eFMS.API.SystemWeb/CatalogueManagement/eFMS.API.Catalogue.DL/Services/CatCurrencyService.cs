using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCurrencyService : RepositoryBase<CatCurrency, CatCurrencyModel>, ICatCurrencyService
    {
        private readonly IDistributedCache cache;
        public CatCurrencyService(IContextBase<CatCurrency> repository, IMapper mapper, IDistributedCache distributedCache) : base(repository, mapper)
        {
            cache = distributedCache;
            SetChildren<CatCharge>("Id", "CurrencyId");
            SetChildren<CatCurrencyExchange>("Id", "CurrencyFromId");
            SetChildren<CatCurrencyExchange>("Id", "CurrencyToId");
            SetChildren<AcctSoa>("Id", "CurrencyId");
            SetChildren<CatCharge>("Id", "CurrencyId");
            SetChildren<CsShipmentSurcharge>("Id", "CurrencyId");
        }

        public override HandleState Add(CatCurrencyModel model)
        {
            var entity = mapper.Map<CatCurrency>(model);
            entity.DatetimeCreated = entity.DatetimeModified = DateTime.Now;
            entity.Inactive = false;
            var result = DataContext.Add(entity, true);
            if (result.Success)
            {
                RedisCacheHelper.SetObject(cache, Templates.CatCurrency.NameCaching.ListName, DataContext.Get());
            }
            return result;
        }
        public HandleState Delete(string id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                Func<CatCurrency, bool> predicate = x => x.Id == id;
                RedisCacheHelper.RemoveItemInList(cache, Templates.CatCurrency.NameCaching.ListName, predicate);
            }
            return hs;
        }

        public List<CatCurrency> Paging(CatCurrrencyCriteria criteria, int pageNumber, int pageSize, out int rowsCount, out int totalPages)
        {
            var data = Query(criteria);
            List<CatCurrency> results = new List<CatCurrency>();
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
                    results = data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            return results;
        }


        public IQueryable<CatCurrency> Query(CatCurrrencyCriteria criteria)
        {
            var list = DataContext.Where(x => x.Inactive == criteria.Inactive || criteria.Inactive == null);
            if (criteria.All == null)
            {
                list = list.Where(x => (x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.CurrencyName ?? "").IndexOf(criteria.CurrencyName ?? "", StringComparison.OrdinalIgnoreCase) > -1);              
            }
            else
            {

                list = list.Where(x => (x.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >-1
                                    || (x.CurrencyName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }
            return list;
        }

        public HandleState Update(CatCurrencyModel model)
        {
            HandleState result = new HandleState();
            try
            {
                var entity = mapper.Map<CatCurrency>(model);
                entity.DatetimeModified = DateTime.Now;
                if (entity.Inactive == true)
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
                            DataContext.DC.Update(item);
                        }
                    }
                }
                ((eFMSDataContext)DataContext.DC).SaveChanges();
                RedisCacheHelper.SetObject(cache, Templates.CatCurrency.NameCaching.ListName, DataContext.Get());
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }
    }
}
