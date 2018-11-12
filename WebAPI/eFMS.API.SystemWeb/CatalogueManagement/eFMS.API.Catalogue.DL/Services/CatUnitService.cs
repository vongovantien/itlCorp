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


namespace eFMS.API.Catalogue.DL.Services
{
    public class CatUnitService:RepositoryBase<CatUnit,CatUnitModel>,ICatUnitService
    {
        public CatUnitService(IContextBase<CatUnit> repository,IMapper mapper) : base(repository, mapper)
        {

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
            var list = DataContext.Get();
            if (criteria.All == null)
            {
                list = list.Where(x => ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.UnitNameVn??"").IndexOf(criteria.UnitNameVn??"",StringComparison.OrdinalIgnoreCase)>=0)
                && ((x.UnitNameEn??"").IndexOf(criteria.UnitNameEn??"",StringComparison.OrdinalIgnoreCase)>=0)).OrderBy(x=>x.Code);
            }
            else
            {
                list = list.Where(x => ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                || ((x.UnitNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                || ((x.UnitNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)).OrderBy(x=>x.Code);
            }
            return list.ToList();
        }
    }
}
