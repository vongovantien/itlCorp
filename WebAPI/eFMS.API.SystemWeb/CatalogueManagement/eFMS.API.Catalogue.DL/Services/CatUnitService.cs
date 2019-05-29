using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatUnitService:RepositoryBase<CatUnit,CatUnitModel>,ICatUnitService
    {
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;
        public CatUnitService(IContextBase<CatUnit> repository,IMapper mapper, IDistributedCache distributedCache, ICurrentUser user) : base(repository, mapper)
        {
            cache = distributedCache;
            currentUser = user;
            SetChildren<CatCharge>("Id", "UnitId"); 
            SetChildren<CsMawbcontainer>("Id", "ContainerTypeId");
            SetChildren<CsMawbcontainer>("Id", "UnitOfMeasureId");
            SetChildren<CsShipmentSurcharge>("Id", "UnitId");
        }

        public override HandleState Add(CatUnitModel model)
        {
            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
            model.Inactive = false;
            model.UserCreated = model.UserModified = currentUser.UserID;
            var entitty = mapper.Map<CatUnit>(model);
            var hs = DataContext.Add(entitty);
            if (hs.Success)
            {
                RedisCacheHelper.SetObject(cache, Templates.CatUnit.NameCaching.ListName, DataContext.Get());
            }
            return hs;
        }
        public HandleState Update(CatUnitModel model)
        {
            var entity = mapper.Map<CatUnit>(model);
            entity.DatetimeModified = DateTime.Now;
            entity.UserModified = currentUser.UserID;
            if(entity.Inactive == true)
            {
                entity.InactiveOn = DateTime.Now;
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                Func<CatUnit, bool> predicate = x => x.Id == model.Id;
                RedisCacheHelper.ChangeItemInList(cache, Templates.CatUnit.NameCaching.ListName, entity, predicate);
            }
            return hs;
        }
        public HandleState Delete(short id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                Func<CatCountry, bool> predicate = x => x.Id == id;
                RedisCacheHelper.RemoveItemInList(cache, Templates.CatCountry.NameCaching.ListName, predicate);
            }
            return hs;
        }
        public List<UnitType> GetUnitTypes()
        {
            return DataEnums.UnitTypes;
        }

        public List<CatUnit> Paging(CatUnitCriteria criteria, int pageNumber, int pageSize, out int rowsCount)
        {
            List<CatUnit> returnList = null;
            var data = Query(criteria);
            rowsCount = data.Count();
            if (rowsCount == 0) return returnList;
            else data = data.OrderByDescending(x => x.DatetimeModified);
            if (pageSize > 1)
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }
                returnList = data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            return returnList;
        }

        public IQueryable<CatUnit> Query(CatUnitCriteria criteria)
        {
            IQueryable<CatUnit> data = RedisCacheHelper.Get<CatUnit>(cache, Templates.CatUnit.NameCaching.ListName);
            IQueryable<CatUnit> list = null;
            if (criteria.All == null)
            {
                //list = data.Where(x => x.Inactive == criteria.Inactive || criteria.Inactive == null);
                Expression<Func<CatUnit, bool>> andQuery = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitNameVn ?? "").IndexOf(criteria.UnitNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitNameEn ?? "").IndexOf(criteria.UnitNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitType ?? "").IndexOf(criteria.UnitType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
                list = Query(data, andQuery);
                //list = list.Where(x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                //&& (x.UnitNameVn??"").IndexOf(criteria.UnitNameVn??"", StringComparison.OrdinalIgnoreCase) > -1
                //&& (x.UnitNameEn??"").IndexOf(criteria.UnitNameEn??"", StringComparison.OrdinalIgnoreCase) > -1
                //&& (x.UnitType ?? "").IndexOf(criteria.UnitType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                //);
            }
            else
            {
                Expression<Func<CatUnit, bool>> orQuery = x => ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.UnitNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.UnitNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.UnitType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                                && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
                list = Query(data, orQuery);
                //list = data.Where(x => x.Inactive == criteria.Inactive || criteria.Inactive == null);
                //list = list.Where(x => (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                //|| (x.UnitNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                //|| (x.UnitNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                //|| (x.UnitType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                //);
            }
            return list;
        }
        private IQueryable<CatUnit> Query(IQueryable<CatUnit> dataFromCache, Expression<Func<CatUnit, bool>> query)
        {
            if (dataFromCache == null)
            {
                RedisCacheHelper.SetObject(cache, Templates.CatUnit.NameCaching.ListName, DataContext.Get());
                return DataContext.Get(query);
            }
            else
            {
                return dataFromCache.Where(query);
            }
        }
        private IQueryable<CatUnit> GetAll()
        {
            IQueryable<CatUnit> data = RedisCacheHelper.Get<CatUnit>(cache, Templates.CatUnit.NameCaching.ListName);
            if (data == null)
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.CatUnit.NameCaching.ListName, data);
            }
            return data;
        }
    }
}
