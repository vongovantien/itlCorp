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
    public class CatCurrencyService : RepositoryBase<CatCurrency, CatCurrencyModel>, ICatCurrencyService
    {
        public CatCurrencyService(IContextBase<CatCurrency> repository, IMapper mapper) : base(repository, mapper)
        {

        }

        public List<CatCurrency> Paging(CatCurrrencyCriteria criteria, int pageNumber, int pageSize, out int rowsCount)
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


        public List<CatCurrency> Query(CatCurrrencyCriteria criteria)
        {
            var list = DataContext.Get();
            if (criteria.All == null)
            {
                list = list.Where(x => ((x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.CurrencyName ?? "").IndexOf(criteria.CurrencyName ?? "", StringComparison.OrdinalIgnoreCase) >= 0));              
            }
            else
            {

                list = list.Where(x => ((x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                || ((x.CurrencyName ?? "").IndexOf(criteria.CurrencyName ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            return list.ToList();
        }
    }
}
