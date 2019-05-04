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
using ITL.NetCore.Connection.NoSql;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCommodityGroupService : RepositoryBase<CatCommodityGroup, CatCommodityGroupModel>, ICatCommodityGroupService
    {
        private readonly IStringLocalizer stringLocalizer;
        public CatCommodityGroupService(IContextBase<CatCommodityGroup> repository, IMapper mapper, IStringLocalizer<LanguageSub> localizer) : base(repository, mapper)
        {
            stringLocalizer = localizer;
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
                        Inactive = item.Inactive,
                        InactiveOn = item.InactiveOn
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
                        Inactive = item.Inactive,
                        InactiveOn = item.InactiveOn
                    };
                    results.Add(group);
                }
            }
            return results;
        }

        public List<CatCommodityGroupModel> Paging(CatCommodityGroupCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CatCommodityGroupModel> results = null;
            var list = Query(criteria);
            rowsCount = list.Count();
            if (rowsCount == 0) return results;
            else list = list.OrderByDescending(x => x.DatetimeModified);
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                list = list.OrderByDescending(x => x.DatetimeModified);
                results = list.Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }

        public IQueryable<CatCommodityGroupModel> Query(CatCommodityGroupCriteria criteria)
        {
            IQueryable<CatCommodityGroupModel> results = Get(x => x.Inactive == criteria.Inactive || criteria.Inactive == null);
            if (criteria.All == null)
            {
                results = Get(x =>((x.GroupNameEn ?? "").IndexOf(criteria.GroupNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                        && ((x.GroupNameVn ?? "").IndexOf(criteria.GroupNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                    );
            }
            else
            {
                results = Get(x => ((x.GroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                        || ((x.GroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                    );
            }
            return results;
        }

        public List<CommodityGroupImportModel> CheckValidImport(List<CommodityGroupImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var commodityGroups = dc.CatCommodityGroup.ToList();
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
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach(var item in data)
                {
                    var commodityGroup = new CatCommodityGroup
                    {
                        GroupNameEn = item.GroupNameEn,
                        GroupNameVn = item.GroupNameVn,
                        Inactive = item.Status.ToString().ToLower() == "active" ? false : true,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = ChangeTrackerHelper.currentUser
                    };
                    dc.CatCommodityGroup.Add(commodityGroup);
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
    }
}
