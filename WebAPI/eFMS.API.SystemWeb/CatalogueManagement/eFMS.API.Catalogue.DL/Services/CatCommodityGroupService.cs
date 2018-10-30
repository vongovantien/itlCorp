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

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCommodityGroupService : RepositoryBase<CatCommodityGroup, CatCommodityGroupModel>, ICatCommodityGroupService
    {
        public CatCommodityGroupService(IContextBase<CatCommodityGroup> repository, IMapper mapper) : base(repository, mapper)
        {
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
                    ).ToList();
            }
            else
            {
                results = Get(x => ((x.GroupNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                        || ((x.GroupNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
            }
            return results;
        }
    }
}
