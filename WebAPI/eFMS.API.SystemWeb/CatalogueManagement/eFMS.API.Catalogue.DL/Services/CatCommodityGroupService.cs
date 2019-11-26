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
using eFMS.API.Catalogue.DL.ViewModels;
using System.Globalization;
using System.Threading;
using ITL.NetCore.Common;
using Microsoft.Extensions.Localization;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.Caching;
using System.Linq.Expressions;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCommodityGroupService : RepositoryBaseCache<CatCommodityGroup, CatCommodityGroupModel>, ICatCommodityGroupService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        public CatCommodityGroupService(IContextBase<CatCommodityGroup> repository, 
            ICacheServiceBase<CatCommodityGroup> cacheService, 
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            SetChildren<CatCommodity>("Id", "CommodityGroupId");
        }

        public List<CatCommodityGroupViewModel> GetByLanguage()
        {
            var data = DataContext.Get();
            return GetDataByLanguage(data);
        }

        private List<CatCommodityGroupViewModel> GetDataByLanguage(IQueryable<CatCommodityGroup> data)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var results = new List<CatCommodityGroupViewModel>();
            if (currentCulture.Name == "vi-VN")
            {
                foreach (var item in data)
                {
                    var group = new CatCommodityGroupViewModel
                    {
                        Id = item.Id,
                        GroupName = item.GroupNameVn,
                        UserCreated = item.UserCreated,
                        DatetimeCreated = item.DatetimeCreated,
                        UserModified = item.UserModified,
                        DatetimeModified = item.DatetimeModified,
                        Active = item.Active,
                        InActiveOn = item.InactiveOn
                    };
                    results.Add(group);
                }
            }
            else
            {
                foreach (var item in data)
                {
                    var group = new CatCommodityGroupViewModel
                    {
                        Id = item.Id,
                        GroupName = item.GroupNameEn,
                        UserCreated = item.UserCreated,
                        DatetimeCreated = item.DatetimeCreated,
                        UserModified = item.UserModified,
                        DatetimeModified = item.DatetimeModified,
                        Active = item.Active,
                        InActiveOn = item.InactiveOn
                    };
                    results.Add(group);
                }
            }
            return results;
        }

        public IQueryable<CatCommodityGroupModel> Paging(CatCommodityGroupCriteria criteria, int page, int size, out int rowsCount)
        {
            Expression<Func<CatCommodityGroupModel, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.GroupNameEn ?? "").IndexOf(criteria.GroupNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.GroupNameVn ?? "").IndexOf(criteria.GroupNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.GroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                        || (x.GroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                        && (x.Active == criteria.Active || criteria.Active == null);
            }
            var data = Paging(query, page, size, out rowsCount);
            return data;
        }

        public IQueryable<CatCommodityGroupModel> Query(CatCommodityGroupCriteria criteria)
        {
            Expression<Func<CatCommodityGroupModel, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.GroupNameEn ?? "").IndexOf(criteria.GroupNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.GroupNameVn ?? "").IndexOf(criteria.GroupNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.GroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                        || (x.GroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                        && (x.Active == criteria.Active || criteria.Active == null);
            }
            var results = Get(query);
            return results;
        }

        public List<CommodityGroupImportModel> CheckValidImport(List<CommodityGroupImportModel> list)
        {
            var commodityGroups = Get();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.GroupNameEn))
                {
                    item.GroupNameEn = stringLocalizer[LanguageSub.MSG_COMMOIDITY_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var commodityGr = commodityGroups.FirstOrDefault(x => x.GroupNameEn.ToLower() == item.GroupNameEn.ToLower());
                    if (commodityGr != null)
                    {
                        item.GroupNameEn = string.Format(stringLocalizer[LanguageSub.MSG_STAGE_EXISTED], item.GroupNameEn);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.GroupNameEn.ToLower() == item.GroupNameEn.ToLower()) > 1){
                        item.GroupNameEn = string.Format(stringLocalizer[LanguageSub.MSG_STAGE_CODE_DUPLICATE], item.GroupNameEn);
                        item.IsValid = false;
                    }
                }


                if (string.IsNullOrEmpty(item.GroupNameVn))
                {
                    item.GroupNameVn = stringLocalizer[LanguageSub.MSG_COMMOIDITY_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var commodityGr = commodityGroups.FirstOrDefault(x => x.GroupNameVn.ToLower() == item.GroupNameVn.ToLower());
                    if (commodityGr != null)
                    {
                        item.GroupNameVn = string.Format(stringLocalizer[LanguageSub.MSG_STAGE_EXISTED], item.GroupNameVn);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.GroupNameVn.ToLower() == item.GroupNameVn.ToLower()) > 1)
                    {
                        item.GroupNameVn = string.Format(stringLocalizer[LanguageSub.MSG_STAGE_CODE_DUPLICATE], item.GroupNameVn);
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

        public HandleState Import(List<CommodityGroupImportModel> data)
        {
            try
            {
                foreach(var item in data)
                {
                    var commodityGroup = new CatCommodityGroup
                    {
                        GroupNameEn = item.GroupNameEn,
                        GroupNameVn = item.GroupNameVn,
                        Active = item.Status.ToLower() != "active",
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID
                    };
                    DataContext.Add(commodityGroup, false);
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
    }
}
