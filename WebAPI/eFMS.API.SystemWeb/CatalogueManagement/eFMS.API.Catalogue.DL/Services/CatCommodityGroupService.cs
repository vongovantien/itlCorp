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
using System.Globalization;
using System.Threading;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCommodityGroupService : RepositoryBase<CatCommodityGroup, CatCommodityGroupModel>, ICatCommodityGroupService
    {
        public CatCommodityGroupService(IContextBase<CatCommodityGroup> repository, IMapper mapper) : base(repository, mapper)
        {
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

        public List<CatCommodityGroupModel> Query(CatCommodityGroupCriteria criteria)
        {
            List<CatCommodityGroupModel> results = null;
            if (criteria.All == null)
            {
                results = Get(x =>((x.GroupNameEn ?? "").IndexOf(criteria.GroupNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                        && ((x.GroupNameVn ?? "").IndexOf(criteria.GroupNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                    ).OrderBy(x => x.GroupNameEn).OrderBy(x => x.GroupNameVn).ToList();
            }
            else
            {
                results = Get(x => ((x.GroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                        || ((x.GroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                    ).OrderBy(x => x.GroupNameEn).OrderBy(x => x.GroupNameVn).ToList();
            }
            return results;
        }
    }
}
