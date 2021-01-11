using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
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
        public HandleState Update(CatUnitModel model)
        {
            var entity = DataContext.First(x => x.Id == model.Id);
            if (entity == null) return new HandleState(404, "Not found");
            entity.UnitNameEn = model.UnitNameEn;
            entity.UnitNameVn = model.UnitNameVn;
            entity.UnitType = model.UnitType;
            entity.DescriptionEn = model.DescriptionEn;
            entity.DescriptionVn = model.DescriptionVn;
            entity.DatetimeModified = DateTime.Now;
            entity.UserModified = currentUser.UserID;
            entity.Active = model.Active;
            if (entity.Active == false)
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
            var data = GetBy(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return data;
            }
            IQueryable<CatUnitModel> returnList = null;
            rowsCount = data.Select(x => x.Id).Count();
            if (criteria.All == null)
            {
                data = data.OrderByDescending(x => x.DatetimeModified);
            }
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
            ClearCache();
            Expression<Func<CatUnitModel, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitNameVn ?? "").IndexOf(criteria.UnitNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitNameEn ?? "").IndexOf(criteria.UnitNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.UnitType ?? "").IndexOf(criteria.UnitType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.Active == criteria.Active || criteria.Active == null);
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
            var result = data.Select(x => new CatUnitModel
            {
                Id = x.Id,
                Code = x.Code,
                UnitNameVn = x.UnitNameVn,
                UnitNameEn = x.UnitNameEn,
                UnitType = x.UnitType,
                UnitTypeName = GetUnitTypes().Where(u => u.Value == x.UnitType).Select(u => u.DisplayName).FirstOrDefault(),
                DescriptionEn = x.DescriptionEn,
                DescriptionVn = x.DescriptionVn,
                UserCreated = x.UserCreated,
                DatetimeCreated = x.DatetimeCreated,
                UserModified = x.UserModified,
                DatetimeModified = x.DatetimeModified,
                Active = x.Active,
                InActiveOn = x.InActiveOn
            });
            if (criteria.All != null && result.Where(x => x.Code == criteria.All).Any())
            {
                result = result.OrderByDescending(x => x.Code == criteria.All);
            }
            return result;
        }
    }
}
