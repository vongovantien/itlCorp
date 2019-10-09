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
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common.Helpers;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCommodityService : RepositoryBase<CatCommodity, CatCommodityModel>, ICatCommodityService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatCommodityGroup> catCommonityGroupRepo;
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;

        public CatCommodityService(IContextBase<CatCommodity> repository, 
            IMapper mapper, 
            IContextBase<CatCommodityGroup> catCommonityGroup, 
            IStringLocalizer<LanguageSub> localizer, 
            IDistributedCache distributedCache,
            ICurrentUser user) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            cache = distributedCache;
            SetChildren<CsMawbcontainer>("Id", "CommodityId");
            catCommonityGroupRepo = catCommonityGroup;
            currentUser = user;
        }

        /// <summary>
        /// get all
        /// </summary>
        /// <returns></returns>
        public IQueryable<CatCommodityModel> GetAll()
        {
            IQueryable<CatCommodity> data = RedisCacheHelper.Get<CatCommodity>(cache, Templates.CatCommodity.NameCaching.ListName);
            if (data == null)
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.CatCommodity.NameCaching.ListName, data);
            }
            if (data == null) return null;
            var results = data.Select(x => mapper.Map<CatCommodityModel>(x));
            return results;
        }

        public List<CatCommodityModel> Paging(CatCommodityCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CatCommodityModel> results = null;
            var data = Query(criteria);
            if (data == null)
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
        public IQueryable<CatCommodityModel> Query(CatCommodityCriteria criteria)
        {
            IQueryable<CatCommodityModel> results = null;
            var commodities = GetCommodities(criteria);
            var catCommonityGroups = catCommonityGroupRepo.Get();
            var query = from com in commodities
                        join grCom in catCommonityGroups on com.CommodityGroupId equals grCom.Id into grpComs
                        from grp in grpComs.DefaultIfEmpty()
                        select new CatCommodityModel {
                            Id = com.Id,
                            Code = com.Code,
                            CommodityNameVn = com.CommodityNameVn,
                            CommodityNameEn = com.CommodityNameEn,
                            CommodityGroupId = com.CommodityGroupId,
                            Note = com.Note,
                            UserCreated = com.UserCreated,
                            DatetimeCreated = com.DatetimeCreated,
                            UserModified = com.UserModified,
                            DatetimeModified = com.DatetimeModified,
                            Active = com.Active,
                            InActiveOn = com.InActiveOn,
                            CommodityGroupNameEn = grp != null? grp.GroupNameEn: string.Empty,
                            CommodityGroupNameVn = grp != null? grp.GroupNameVn: string.Empty
                        };
            if (criteria.All == null)
            {
                results = query.Where(x => (x.CommodityNameVn ?? "").IndexOf(criteria.CommodityNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.CommodityNameEn ?? "").IndexOf(criteria.CommodityNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.CommodityGroupNameEn ?? "").IndexOf(criteria.CommodityGroupNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.CommodityGroupNameVn ?? "").IndexOf(criteria.CommodityGroupNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              );
            }
            else
            {
                results = query.Where(x => (x.CommodityNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.CommodityNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.CommodityGroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.CommodityGroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              );
            }
            return results;
        }
        
        #region CRUD
        public override HandleState Add(CatCommodityModel entity)
        {
            var commonity = mapper.Map<CatCommodity>(entity);
            commonity.UserCreated = commonity.UserModified = currentUser.UserID;
            commonity.DatetimeCreated = commonity.DatetimeModified = DateTime.Now;
            commonity.Active = true;
            var result = DataContext.Add(commonity);
            if (result.Success)
            {
                cache.Remove(Templates.CatCommodity.NameCaching.ListName);
            }
            return result;
        }
        public HandleState Update(CatCommodityModel model)
        {
            var entity = mapper.Map<CatCommodity>(model);
            entity.UserModified = currentUser.UserID;
            entity.DatetimeModified = DateTime.Now;
            if (entity.Active == true)
            {
                entity.InActiveOn = DateTime.Now;
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                cache.Remove(Templates.CatCommodity.NameCaching.ListName);
            }
            return hs;
        }
        public HandleState Delete(int id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                cache.Remove(Templates.CatCommodity.NameCaching.ListName);
            }
            return hs;
        }
        #endregion

        #region Import
        public HandleState Import(List<CommodityImportModel> data)
        {
            try
            {
                //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach (var item in data)
                {
                    var commodity = new CatCommodity
                    {
                        CommodityNameEn = item.CommodityNameEn,
                        CommodityNameVn = item.CommodityNameVn,
                        CommodityGroupId = item.CommodityGroupId,
                        Code = item.Code,
                        Active = item.Status.ToLower() != "active",
                        DatetimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        UserCreated = currentUser.UserID
                    };
                    DataContext.Add(commodity);
                    //dc.CatCommodity.Add(commodity);
                }
                //dc.SaveChanges();
                DataContext.SubmitChanges();
                cache.Remove(Templates.CatCommodity.NameCaching.ListName);
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        public List<CommodityImportModel> CheckValidImport(List<CommodityImportModel> list)
        {
            //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var commodities = DataContext.Get().ToList();
            var commodityGroups = catCommonityGroupRepo.Get().ToList();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[LanguageSub.MSG_COMMOIDITY_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    if (commodities.Any(x => x.Code.ToLower() == item.Code.ToLower()))
                    {
                        item.Code = stringLocalizer[LanguageSub.MSG_COMMOIDITY_CODE_EXISTED, item.Code];
                        item.IsValid = false;
                    }
                    else if (list.Count(x => x.Code.ToLower() == item.Code.ToLower()) > 1)
                    {
                        item.Code = stringLocalizer[LanguageSub.MSG_COMMOIDITY_CODE_DUPLICATED, item.Code];
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
                else
                {
                    if(!commodityGroups.Any(x => x.Id == item.CommodityGroupId))
                    {
                        item.CommodityGroupId = -2;
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.Status))
                {
                    item.Status = stringLocalizer[LanguageSub.MSG_COMMOIDITY_STATUS_EMPTY];
                    item.IsValid = false;
                }
            });
            return list;
        }
        #endregion

        private IQueryable<CatCommodity> GetCommodities(CatCommodityCriteria criteria)
        {
            var commonitiesCaching = RedisCacheHelper.GetObject<List<CatCommodity>>(cache, Templates.CatCommodity.NameCaching.ListName);
            IQueryable<CatCommodity> commodities = null;
            if (commonitiesCaching == null)
            {
                RedisCacheHelper.SetObject(cache, Templates.CatCommodity.NameCaching.ListName, DataContext.Get());
                commodities = DataContext.Get(x => x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                commodities = commonitiesCaching.Where(x => x.Active == criteria.Active || criteria.Active == null).AsQueryable();
            }
            return commodities;
        }

    }
}
