using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using ITL.NetCore.Common;
using Microsoft.Extensions.Localization;
using eFMS.API.Catalogue.DL.Common;
using Microsoft.Extensions.Caching.Distributed;
using ITL.NetCore.Connection.NoSql;
using eFMS.API.Catalogue.Service.Contexts;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCommodityService : RepositoryBase<CatCommodity, CatCommodityModel>, ICatCommodityService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatCommodityGroup> catCommonityGroupRepo;
        private readonly IDistributedCache cache;
        public CatCommodityService(IContextBase<CatCommodity> repository, IMapper mapper, IContextBase<CatCommodityGroup> catCommonityGroup, IStringLocalizer<LanguageSub> localizer, IDistributedCache distributedCache) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            cache = distributedCache;
            SetChildren<CsMawbcontainer>("Id", "CommodityId");
            catCommonityGroupRepo = catCommonityGroup;
        }

        public override HandleState Add(CatCommodityModel model)
        {
            var commonity = mapper.Map<CatCommodity>(model);
            commonity.DatetimeCreated = commonity.DatetimeModified = DateTime.Now;
            commonity.Inactive = false;
            var result = DataContext.Add(commonity);
            if (result.Success)
            {
                RedisCacheHelper.SetObject(cache, Templates.CatCommodity.NameCaching.ListName, DataContext.Get());
            }
            return result;
        }
        public List<CommodityImportModel> CheckValidImport(List<CommodityImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var commodities = dc.CatCommodity.ToList();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[LanguageSub.MSG_COMMOIDITY_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    if(commodities.Any(x => x.Code.ToLower() == item.Code.ToLower()))
                    {
                        item.Code = stringLocalizer[LanguageSub.MSG_COMMOIDITY_CODE_EMPTY];
                        item.IsValid = false;
                    }
                    if(list.Count(x => x.Code.ToLower() == item.Code.ToLower()) > 1)
                    {
                        item.Code = stringLocalizer[LanguageSub.MSG_COMMOIDITY_CODE_DUPLICATED];
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.CommodityNameEn))
                {
                    item.CommodityNameEn = stringLocalizer[LanguageSub.MSG_COMMOIDITY_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CommodityNameVn))
                {
                    item.CommodityNameVn = stringLocalizer[LanguageSub.MSG_COMMOIDITY_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (item.CommodityGroupId == null)
                {
                    item.CommodityGroupId = -1;
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Status))
                {
                    item.Status = stringLocalizer[LanguageSub.MSG_COMMOIDITY_STATUS_EMPTY];
                    item.IsValid = false;
                }
            });
            return list;
        }

        public HandleState Delete(int id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                Func<CatCommodity, bool> predicate = x => x.Id == id;
                RedisCacheHelper.RemoveItemInList(cache, Templates.CatCommodity.NameCaching.ListName, predicate);
            }
            return hs;
        }

        public HandleState Import(List<CommodityImportModel> data)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var lstCommodity = RedisCacheHelper.GetObject<List<CatCommodity>>(cache, Templates.CatCommodity.NameCaching.ListName);
                var newList = new List<CatCommodity>();
                foreach (var item in data)
                {
                    var commodity = new CatCommodity
                    {
                        CommodityNameEn = item.CommodityNameEn,
                        CommodityNameVn = item.CommodityNameVn,
                        CommodityGroupId = item.CommodityGroupId,
                        Code = item.Code,
                        Inactive = item.Status.ToString().ToLower()=="active"?false:true,
                        DatetimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        UserCreated = ChangeTrackerHelper.currentUser
                    };
                    dc.CatCommodity.Add(commodity);
                    newList.Add(commodity);
                }
                dc.SaveChanges();
                if (lstCommodity == null)
                {
                    lstCommodity = dc.CatCommodity.ToList();
                }
                else
                {
                    lstCommodity.AddRange(newList);
                }
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public List<CatCommodityModel> Paging(CatCommodityCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CatCommodityModel> results = null;
            var data = Query(criteria);
            if(data == null)
            {
                rowsCount = 0;
                return results;
            }
            rowsCount = data.Count();
            data = data.OrderByDescending(x => x.DatetimeModified);
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = data.Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }
        private IQueryable<CatCommodity> GetCommodities(CatCommodityCriteria criteria)
        {
            var commonitiesCaching = RedisCacheHelper.GetObject<List<CatCommodity>>(cache, Templates.CatCommodity.NameCaching.ListName);
            IQueryable<CatCommodity> commodities = null;
            if (commonitiesCaching == null)
            {
                commodities = DataContext.Get(x => x.Inactive == criteria.Inactive || criteria.Inactive == null);
            }
            else
            {
                commodities = commonitiesCaching.Where(x => x.Inactive == criteria.Inactive || criteria.Inactive == null).AsQueryable();
            }
            return commodities;
        }
        public IQueryable<CatCommodityModel> Query(CatCommodityCriteria criteria)
        {
            IQueryable<CatCommodityModel> results = null;
            var commodities = GetCommodities(criteria);
            var catCommonityGroups = ((eFMSDataContext)DataContext.DC).CatCommodityGroup;
            if (criteria.All == null)
            {
                results = commodities.Join(catCommonityGroups, com => com.CommodityGroupId, group => group.Id,
                                        (com, group) => new { com, group })
                                     .Where(x => (x.com.CommodityNameVn ?? "").IndexOf(criteria.CommodityNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.com.CommodityNameEn ?? "").IndexOf(criteria.CommodityNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.group.GroupNameEn ?? "").IndexOf(criteria.CommodityGroupNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.group.GroupNameVn ?? "").IndexOf(criteria.CommodityGroupNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.com.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                     ).Select(x => new CatCommodityModel {
                                         Id = x.com.Id,
                                         Code = x.com.Code,
                                         CommodityNameVn = x.com.CommodityNameVn,
                                         CommodityNameEn = x.com.CommodityNameEn,
                                         CommodityGroupId = x.com.CommodityGroupId,
                                         Note = x.com.Note,
                                         UserCreated = x.com.UserCreated,
                                         DatetimeCreated = x.com.DatetimeCreated,
                                         UserModified = x.com.UserModified,
                                         DatetimeModified = x.com.DatetimeModified,
                                         Inactive = x.com.Inactive,
                                         InactiveOn = x.com.InactiveOn,
                                         CommodityGroupNameEn = x.group.GroupNameEn,
                                         CommodityGroupNameVn = x.group.GroupNameVn
                                    });
            }
            else
            {
                results = commodities.Join(catCommonityGroups, com => com.CommodityGroupId, group => group.Id,
                                        (com, group) => new { com, group })
                                     .Where(x => (x.com.CommodityNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.com.CommodityNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.group.GroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.group.GroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.com.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                     ).Select(x => new CatCommodityModel
                                     {
                                         Id = x.com.Id,
                                         Code = x.com.Code,
                                         CommodityNameVn = x.com.CommodityNameVn,
                                         CommodityNameEn = x.com.CommodityNameEn,
                                         CommodityGroupId = x.com.CommodityGroupId,
                                         Note = x.com.Note,
                                         UserCreated = x.com.UserCreated,
                                         DatetimeCreated = x.com.DatetimeCreated,
                                         UserModified = x.com.UserModified,
                                         DatetimeModified = x.com.DatetimeModified,
                                         Inactive = x.com.Inactive,
                                         InactiveOn = x.com.InactiveOn,
                                         CommodityGroupNameEn = x.group.GroupNameEn,
                                         CommodityGroupNameVn = x.group.GroupNameVn
                                     });
            }
            return results;
        }

        public HandleState Update(CatCommodityModel model)
        {
            var entity = mapper.Map<CatCommodity>(model);
            entity.DatetimeModified = DateTime.Now;
            if (entity.Inactive == true)
            {
                entity.InactiveOn = DateTime.Now;
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                Func<CatCommodity, bool> predicate = x => x.Id == model.Id;
                RedisCacheHelper.ChangeItemInList(cache, Templates.CatCommodity.NameCaching.ListName, entity, predicate);
            }
            return hs;
        }
    }
}
