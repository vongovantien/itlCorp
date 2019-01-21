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

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCommodityService : RepositoryBase<CatCommodity, CatCommodityModel>, ICatCommodityService
    {
        private readonly IContextBase<CatCommodityGroup> catCommonityGroupRepo;
        public CatCommodityService(IContextBase<CatCommodity> repository, IMapper mapper, IContextBase<CatCommodityGroup> catCommonityGroup) : base(repository, mapper)
        {
            catCommonityGroupRepo = catCommonityGroup;
        }

        public List<CommodityImportModel> CheckValidImport(List<CommodityImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var commodities = dc.CatCommodity.ToList();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.CommodityNameEn))
                {
                    item.CommodityNameEn = string.Format("Name En is not allow empty!|wrong");
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CommodityNameVn))
                {
                    item.CommodityNameVn = string.Format("Name Local is not allow empty!|wrong");
                    item.IsValid = false;
                }
                if (item.CommodityGroupId == null)
                {
                    item.CommodityGroupId = -1;
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Status))
                {
                    item.Status = string.Format("Status is not allow empty!|wrong");
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
                                     .Where(x => ((x.com.CommodityNameVn ?? "").IndexOf(criteria.CommodityNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                              && ((x.com.CommodityNameEn ?? "").IndexOf(criteria.CommodityNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                              && ((x.group.GroupNameEn ?? "").IndexOf(criteria.CommodityGroupNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                              && ((x.group.GroupNameVn ?? "").IndexOf(criteria.CommodityGroupNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                     ).Select(x => new CatCommodityViewModel {
                                         Id = x.com.Id,
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
                                         CommonityGroupNameEn = x.group.GroupNameEn,
                                         CommonityGroupNameVn = x.group.GroupNameVn
                                    }).ToList();
            }
            else
            {
                results = commonities.Join(catCommonityGroups, com => com.CommodityGroupId, group => group.Id,
                                        (com, group) => new { com, group })
                                     .Where(x => ((x.com.CommodityNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                              || ((x.com.CommodityNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                              || ((x.group.GroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                              || ((x.group.GroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                     ).Select(x => new CatCommodityViewModel
                                     {
                                         Id = x.com.Id,
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
                                         CommonityGroupNameEn = x.group.GroupNameEn,
                                         CommonityGroupNameVn = x.group.GroupNameVn
                                     }).ToList();
            }
            return results;
        }
    }
}
