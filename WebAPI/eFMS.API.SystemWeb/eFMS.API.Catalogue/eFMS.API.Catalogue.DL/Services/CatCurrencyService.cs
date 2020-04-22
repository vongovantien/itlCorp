using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;
using System.Linq.Expressions;
using ITL.NetCore.Connection.Caching;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCurrencyService : RepositoryBaseCache<CatCurrency, CatCurrencyModel>, ICatCurrencyService
    {
        private readonly ICurrentUser currentUser;

        public CatCurrencyService(IContextBase<CatCurrency> repository, 
            ICacheServiceBase<CatCurrency> cacheService, 
            IMapper mapper,
            ICurrentUser currUser) : base(repository, cacheService, mapper)
        {
            currentUser = currUser;
            SetChildren<CatCharge>("Id", "CurrencyId");
            SetChildren<CatCurrencyExchange>("Id", "CurrencyFromId");
            SetChildren<CatCurrencyExchange>("Id", "CurrencyToId");
            //SetChildren<AcctSoa>("Id", "CurrencyId");
            SetChildren<CatCharge>("Id", "CurrencyId");
            SetChildren<CsShipmentSurcharge>("Id", "CurrencyId");
            //SetChildren<AcctCdnote>("Id", "CurrencyId");
            //SetChildren<AcctSoa>("Id", "Currency");
            SetChildren<CatCharge>("Id", "Currency");
        }

        #region CRUD
        public override HandleState Add(CatCurrencyModel entity)
        {
            entity.DatetimeCreated = entity.DatetimeModified = DateTime.Now;
            entity.Active = true;
            entity.UserCreated = currentUser.UserID;
            var result = DataContext.Add(entity);
            if (result.Success)
            {
                ClearCache();
                Get();
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
                    DataContext.SubmitChanges();
                    ClearCache();
                    Get();
                }
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
            if(hs.Success == true)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion

        public IQueryable<CatCurrencyModel> Paging(CatCurrrencyCriteria criteria, int pageNumber, int pageSize, out int rowsCount)
        {
            Expression<Func<CatCurrencyModel, bool>> query;
            if (criteria.All == null)
            {
                query = (x => (x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                       && (x.CurrencyName ?? "").IndexOf(criteria.CurrencyName ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }
            else
            {
                query = (x => (x.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    || (x.CurrencyName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }
            var data = Paging(query, pageNumber, pageSize, x => x.DatetimeModified, false, out rowsCount);
            return data;
        }
        public IQueryable<CatCurrencyModel> Query(CatCurrrencyCriteria criteria)
        {
            return GetBy(criteria);
        }

        private IQueryable<CatCurrencyModel> GetBy(CatCurrrencyCriteria criteria)
        {
            Expression<Func<CatCurrencyModel, bool>> query;
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
            return Get(query);
        }

        public IQueryable<CatCurrencyModel> GetAll()
        {
            return Get();
        }
    }
}
