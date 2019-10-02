using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatUnitService: RepositoryBaseCache<CatUnit,CatUnitModel>,ICatUnitService
    {
        private readonly ICurrentUser currentUser;
        public CatUnitService(IContextBase<CatUnit> repository, ICacheServiceBase<List<CatUnit>> cacheService, IMapper mapper, ICurrentUser user) : base(repository, cacheService,mapper)
        {
            currentUser = user;
            SetChildren<CatCharge>("Id", "UnitId"); 
            SetChildren<CsMawbcontainer>("Id", "ContainerTypeId");
            SetChildren<CsMawbcontainer>("Id", "UnitOfMeasureId");
            SetChildren<CsShipmentSurcharge>("Id", "UnitId");
        }

        #region CRUD
        public override HandleState Add(CatUnitModel entity)
        {
            entity.DatetimeCreated = entity.DatetimeModified = DateTime.Now;
            entity.Inactive = false;
            entity.UserCreated = entity.UserModified = currentUser.UserID;
            var unit = mapper.Map<CatUnit>(entity);
            var hs = DataContext.Add(unit);
            //if (hs.Success)
            //{
            //    cache.Remove(Templates.CatUnit.NameCaching.ListName);
            //}
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
            //if (hs.Success)
            //{
            //    cache.Remove(Templates.CatUnit.NameCaching.ListName);
            //}
            return hs;
        }
        public HandleState Delete(short id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            //if (hs.Success)
            //{
            //    cache.Remove(Templates.CatUnit.NameCaching.ListName);
            //}
            return hs;
        }
        #endregion

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
            //IQueryable<CatUnit> data = RedisCacheHelper.Get<CatUnit>(cache, Templates.CatUnit.NameCaching.ListName);
            //IQueryable<CatUnit> list = null;
            //Expression<Func<CatUnit, bool>> query = null;
            //if (criteria.All == null)
            //{
            //    query = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
            //                            && (x.UnitNameVn ?? "").IndexOf(criteria.UnitNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
            //                            && (x.UnitNameEn ?? "").IndexOf(criteria.UnitNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
            //                            && (x.UnitType ?? "").IndexOf(criteria.UnitType ?? "", StringComparison.OrdinalIgnoreCase) > -1
            //                            && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
            //    //);
            //}
            //else
            //{
            //    query = x => ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
            //                                                    || (x.UnitNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
            //                                                    || (x.UnitNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
            //                                                    || (x.UnitType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
            //                                                    && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
            //}
            //list = Query(data, query);
            //return list;
            Expression<Func<CatUnitModel, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitNameVn ?? "").IndexOf(criteria.UnitNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitNameEn ?? "").IndexOf(criteria.UnitNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitType ?? "").IndexOf(criteria.UnitType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
                //);
            }
            else
            {
                query = x => ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.UnitNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.UnitNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.UnitType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                                && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
            }
            var data = Query(query);
            return data;
        }
        private IQueryable<CatUnitModel> Query(Expression<Func<CatUnitModel, bool>> query)
        {
            //if (dataFromCache == null)
            //{
            //    RedisCacheHelper.SetObject(cache, Templates.CatUnit.NameCaching.ListName, DataContext.Get());
            //    return DataContext.Get(query);
            //}
            //else
            //{
            //    return dataFromCache.Where(query);
            //}
            var data = GetAll().Where(query);
            return data;
        }
        public IQueryable<CatUnitModel> GetAll()
        {
            //IQueryable<CatUnit> data = RedisCacheHelper.Get<CatUnit>(cache, Templates.CatUnit.NameCaching.ListName);
            //if (data == null)
            //{
            //    data = DataContext.Get();
            //    RedisCacheHelper.SetObject(cache, Templates.CatUnit.NameCaching.ListName, data);
            //}
            //return data;
            var data = Get();
            return data;
        }
    }
}
