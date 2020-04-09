using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class CatUnitService: RepositoryBaseCache<CatUnit,CatUnitModel>, ICatUnitService
    {
        private readonly ICurrentUser currentUser;

        public CatUnitService(IContextBase<CatUnit> repository, 
            ICacheServiceBase<CatUnit> cacheService, 
            IMapper mapper,
            ICurrentUser user) : base(repository, cacheService, mapper)
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
            entity.Active = true;
            entity.UserCreated = entity.UserModified = currentUser.UserID;
            var unit = mapper.Map<CatUnit>(entity);
            var hs = DataContext.Add(unit);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        public HandleState Update(CatUnitModel model)
        {
            var entity = mapper.Map<CatUnit>(model);
            entity.DatetimeModified = DateTime.Now;
            entity.UserModified = currentUser.UserID;
            if(entity.Active == false)
            {
                entity.InactiveOn = DateTime.Now;
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        public HandleState Delete(short id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion

        public List<UnitType> GetUnitTypes()
        {
            return DataEnums.UnitTypes;
        }

        public IQueryable<CatUnitModel> Paging(CatUnitCriteria criteria, int pageNumber, int pageSize, out int rowsCount)
        {
            IQueryable<CatUnitModel> returnList = null;
            var data = GetBy(criteria);
            if(data == null)
            {
                rowsCount = 0;
                return data;
            }
            rowsCount = data.Count();
            data = data.OrderByDescending(x => x.DatetimeModified);
            if (pageSize > 1)
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }
                returnList = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            return returnList;
        }

        public IQueryable<CatUnitModel> Query(CatUnitCriteria criteria)
        {
            var data = GetBy(criteria);
            return data;
        }
        private IQueryable<CatUnitModel> GetBy(CatUnitCriteria criteria)
        {
            Expression<Func<CatUnitModel, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitNameVn ?? "").IndexOf(criteria.UnitNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitNameEn ?? "").IndexOf(criteria.UnitNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitType ?? "").IndexOf(criteria.UnitType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.Active == criteria.Active || criteria.Active == null);
                //);
            }
            else
            {
                query = x => ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.UnitNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.UnitNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.UnitType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                                && (x.Active == criteria.Active || criteria.Active == null);
            }
            var data = Get().Where(query);
            return data;
        }
        public IQueryable<CatUnitModel> GetAll()
        {
            return Get();
        }

        public CatUnitModel GetDetail(short id)
        {
            var data = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (data == null) return null;
            return mapper.Map<CatUnitModel>(data);
        }
    }
}
