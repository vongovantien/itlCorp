using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using eFMS.API.Catalogue.DL.ViewModels;
using ITL.NetCore.Common;
using eFMS.API.Catalogue.Service.Helpers;
using Microsoft.Extensions.Localization;
using eFMS.API.Catalogue.DL.Common;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCommodityService : RepositoryBase<CatCommodity, CatCommodityModel>, ICatCommodityService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatCommodityGroup> catCommonityGroupRepo;
        public CatCommodityService(IContextBase<CatCommodity> repository, IMapper mapper, IContextBase<CatCommodityGroup> catCommonityGroup, IStringLocalizer<LanguageSub> localizer) : base(repository, mapper)
        {
            SetChildren<CsMawbcontainer>("Id", "CommodityId");
            catCommonityGroupRepo = catCommonityGroup;
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

        public HandleState Import(List<CommodityImportModel> data)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach(var item in data)
                {
                    var commodity = new CatCommodity
                    {
                        CommodityNameEn = item.CommodityNameEn,
                        CommodityNameVn = item.CommodityNameVn,
                        CommodityGroupId = item.CommodityGroupId,
                        Inactive = item.Status.ToString().ToLower()=="active"?false:true,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = ChangeTrackerHelper.currentUser
                    };
                    dc.CatCommodity.Add(commodity);
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public List<CatCommodityViewModel> Paging(CatCommodityCriteria criteria, int page, int size, out int rowsCount)
        {
            var list = Query(criteria);
            rowsCount = list.Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                list = list.Skip((page - 1) * size).Take(size).ToList();
            }
            return list;
        }

        public List<CatCommodityViewModel> Query(CatCommodityCriteria criteria)
        {
            List<CatCommodityViewModel> results = null;

            var commonities = DataContext.Where(x => x.Inactive == criteria.Inactive || criteria.Inactive == null);
            var catCommonityGroups = ((eFMSDataContext)DataContext.DC).CatCommodityGroup;
            if (criteria.All == null)
            {
                results = commonities.Join(catCommonityGroups, com => com.CommodityGroupId, group => group.Id,
                                        (com, group) => new { com, group })
                                     .Where(x => (x.com.CommodityNameVn ?? "").IndexOf(criteria.CommodityNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.com.CommodityNameEn ?? "").IndexOf(criteria.CommodityNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.group.GroupNameEn ?? "").IndexOf(criteria.CommodityGroupNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.group.GroupNameVn ?? "").IndexOf(criteria.CommodityGroupNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              && (x.com.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                     ).Select(x => new CatCommodityViewModel {
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
                                    }).ToList();
            }
            else
            {
                //var data = commonities.Join(catCommonityGroups, com => com.CommodityGroupId, group => group.Id,
                //                        (com, group) => new { com, group }).Select(x => new { x.com, x.group}).ToList();
                //var data1 = data.Where(x => (x.com.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                //                              || (x.com.CommodityNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= -1
                //                              || (x.com.CommodityNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= -1
                //                              || (x.group.GroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= -1
                //                              || (x.group.GroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1

                //                ).ToList();
                results = commonities.Join(catCommonityGroups, com => com.CommodityGroupId, group => group.Id,
                                        (com, group) => new { com, group })
                                     .Where(x => (x.com.CommodityNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.com.CommodityNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.group.GroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.group.GroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                              || (x.com.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                     ).Select(x => new CatCommodityViewModel
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
                                     }).ToList();
            }
            return results;
        }
    }
}
