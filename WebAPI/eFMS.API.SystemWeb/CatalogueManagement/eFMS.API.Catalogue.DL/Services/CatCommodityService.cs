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
using ITL.NetCore.Connection.Caching;
using System.Linq.Expressions;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCommodityService : RepositoryBaseCache<CatCommodity, CatCommodityModel>, ICatCommodityService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCommodityGroupService catCommodityGroupService;
        private readonly ICurrentUser currentUser;
        public CatCommodityService(IContextBase<CatCommodity> repository, 
            ICacheServiceBase<CatCommodity> cacheService, 
            IMapper mapper,
            IStringLocalizer<CatalogueLanguageSub> localizer,
            ICatCommodityGroupService commodityGroupService,
            ICurrentUser user) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            SetChildren<CsMawbcontainer>("Id", "CommodityId");
            catCommodityGroupService = commodityGroupService;
            currentUser = user;
        }

        /// <summary>
        /// get all
        /// </summary>
        /// <returns></returns>
        public IQueryable<CatCommodityModel> GetAll()
        {
            var data = Get();
            return data;
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
            Expression<Func<CatCommodityModel, bool>> query = null;

            var commodities = Get();
            var catCommonityGroups = catCommodityGroupService.Get();
            IQueryable<CatCommodityModel> results = from com in commodities
                      join grCom in catCommonityGroups on com.CommodityGroupId equals grCom.Id into grpComs
                      from grp in grpComs.DefaultIfEmpty()
                      select new CatCommodityModel
                      {
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
                          InactiveOn = com.InactiveOn,
                          CommodityGroupNameEn = grp != null ? grp.GroupNameEn : string.Empty,
                          CommodityGroupNameVn = grp != null ? grp.GroupNameVn : string.Empty
                      };
            if (criteria.All == null)
            {
                query = x => (x.CommodityNameVn ?? "").IndexOf(criteria.CommodityNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.CommodityNameEn ?? "").IndexOf(criteria.CommodityNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.CommodityGroupNameEn ?? "").IndexOf(criteria.CommodityGroupNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.CommodityGroupNameVn ?? "").IndexOf(criteria.CommodityGroupNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.CommodityNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.CommodityNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.CommodityGroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.CommodityGroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                            && (x.Active == criteria.Active || criteria.Active == null);
            }
            if (results == null) return results;
            results = results.Where(query);
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
                ClearCache();
                Get();
            }
            return result;
        }
        public HandleState Update(CatCommodityModel model)
        {
            var entity = mapper.Map<CatCommodity>(model);
            entity.UserModified = currentUser.UserID;
            entity.DatetimeModified = DateTime.Now;
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
        public HandleState Delete(int id)
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

        #region Import
        public HandleState Import(List<CommodityImportModel> data)
        {
            try
            {
                foreach (var item in data)
                {
                    bool active = !string.IsNullOrEmpty(item.Status) && (item.Status.ToLower() == "active");
                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var commodity = new CatCommodity
                    {
                        CommodityNameEn = item.CommodityNameEn,
                        CommodityNameVn = item.CommodityNameVn,
                        CommodityGroupId = item.CommodityGroupId,
                        Code = item.Code,
                        Active = active,
                        InactiveOn = inactiveDate,
                        DatetimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        UserCreated = currentUser.UserID
                    };
                    DataContext.Add(commodity, false);
                }
                DataContext.SubmitChanges();
                ClearCache();
                Get();
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        public List<CommodityImportModel> CheckValidImport(List<CommodityImportModel> list)
        {
            var commodities = Get();
            var commodityGroups = catCommodityGroupService.Get();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[CatalogueLanguageSub.MSG_COMMOIDITY_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    if (commodities.Any(x => x.Code != null && x.Code.ToLower() == item.Code.ToLower()))
                    {
                        item.Code = stringLocalizer[CatalogueLanguageSub.MSG_COMMOIDITY_CODE_EXISTED, item.Code];
                        item.IsValid = false;
                    }
                    else if (list.Count(x => x.Code != null && x.Code.ToLower() == item.Code.ToLower()) > 1)
                    {
                        item.Code = stringLocalizer[CatalogueLanguageSub.MSG_COMMOIDITY_CODE_DUPLICATED, item.Code];
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.CommodityNameEn))
                {
                    item.CommodityNameEn = stringLocalizer[CatalogueLanguageSub.MSG_COMMOIDITY_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CommodityNameVn))
                {
                    item.CommodityNameVn = stringLocalizer[CatalogueLanguageSub.MSG_COMMOIDITY_NAME_LOCAL_EMPTY];
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
                    item.Status = stringLocalizer[CatalogueLanguageSub.MSG_COMMOIDITY_STATUS_EMPTY];
                    item.IsValid = false;
                }
            });
            return list;
        }
        #endregion
    }
}
