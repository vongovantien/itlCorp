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
    public class CatUnitService:RepositoryBase<CatUnit,CatUnitModel>,ICatUnitService
    {
        private readonly IDistributedCache cache;
        public CatUnitService(IContextBase<CatUnit> repository,IMapper mapper, IDistributedCache distributedCache) : base(repository, mapper)
        {
            cache = distributedCache;
            SetChildren<CatCharge>("Id", "UnitId"); 
            SetChildren<CsMawbcontainer>("Id", "ContainerTypeId");
            SetChildren<CsMawbcontainer>("Id", "UnitOfMeasureId");
            SetChildren<CsShipmentSurcharge>("Id", "UnitId");
        }

        public override HandleState Add(CatUnitModel model)
        {
            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
            model.Inactive = false;
            var hs = DataContext.Add(model);
            if (hs.Success)
            {
                RedisCacheHelper.SetObject(cache, Templates.CatUnit.NameCaching.ListName, DataContext.Get().ToList());
            }
            return hs;
        }
        public HandleState Update(CatUnitModel model)
        {
            var entity = mapper.Map<CatUnit>(model);
            entity.DatetimeModified = DateTime.Now;
            if(entity.Inactive == true)
            {
                entity.InactiveOn = DateTime.Now;
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                var listUnit = RedisCacheHelper.GetObject<List<CatUnit>>(cache, Templates.CatUnit.NameCaching.ListName);
                var index = listUnit.FindIndex(x => x.Id == entity.Id);
                if (index > -1)
                {
                    listUnit[index] = entity;
                    RedisCacheHelper.SetObject(cache, Templates.CatUnit.NameCaching.ListName, listUnit);
                }
            }
            return hs;
        }
        public HandleState Delete(short id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                var listUnit = RedisCacheHelper.GetObject<List<CatUnit>>(cache, Templates.CatUnit.NameCaching.ListName);
                var index = listUnit.FindIndex(x => x.Id == id);
                if (index > -1)
                {
                    listUnit.RemoveAt(index);
                    RedisCacheHelper.SetObject(cache, Templates.CatUnit.NameCaching.ListName, listUnit);
                }
            }
            return hs;
        }
        public List<UnitType> GetUnitTypes()
        {
            return DataEnums.UnitTypes;
        }

        public List<CatUnit> Paging(CatUnitCriteria criteria, int pageNumber, int pageSize, out int rowsCount)
        {
            var list = Query(criteria);
            rowsCount = list.Count;
            if (pageSize > 1)
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }
                list = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            return list;
        }

        public List<CatUnit> Query(CatUnitCriteria criteria)
        {
            var data = GetAll();
            if (data == null) return null;
            var list = data.Where(x => x.Inactive == criteria.Inactive || criteria.Inactive == null);
            if (criteria.All == null)
            {
                list = list.Where(x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                && (x.UnitNameVn??"").IndexOf(criteria.UnitNameVn??"", StringComparison.OrdinalIgnoreCase) > -1
                && (x.UnitNameEn??"").IndexOf(criteria.UnitNameEn??"", StringComparison.OrdinalIgnoreCase) > -1
                && (x.UnitType ?? "").IndexOf(criteria.UnitType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                ).OrderByDescending(x => x.DatetimeModified);
            }
            else
            {
                list = list.Where(x => (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                || (x.UnitNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                || (x.UnitNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                || (x.UnitType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                ).OrderByDescending(x => x.DatetimeModified);
            }
            return list.ToList();
        }

        private IQueryable<CatUnit> GetAll()
        {
            IQueryable<CatUnit> data = RedisCacheHelper.Get<CatUnit>(cache, Templates.CatUnit.NameCaching.ListName);
            cache.Remove(Templates.CatUnit.NameCaching.ListName);
            if (data == null)
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.CatUnit.NameCaching.ListName, data);
            }
            return data;
        }
    }
}
