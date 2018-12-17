using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCountryService : RepositoryBase<CatCountry, CatCountryModel>, ICatCountryService
    {
        public CatCountryService(IContextBase<CatCountry> repository, IMapper mapper) : base(repository, mapper)
        {
            SetChildren<CatPlace>("Id", "CountryId");
        }

        public List<CatCountryViewModel> GetByLanguage()
        {
            var data = DataContext.Get();
            return GetDataByLanguage(data);
        }

        public List<CatCountry> GetCountries(CatCountryCriteria criteria, int page, int size, out int rowsCount)
        {
            var returnList = new List<CatCountry>();
            if(criteria.condition == SearchCondition.AND)
            {
                var s = DataContext.Get(x =>((x.Code??"").IndexOf(criteria.Code??"")>=0)
                && (x.NameEn??"").IndexOf(criteria.NameEn??"")>=0
                && (x.NameVn??"").IndexOf(criteria.NameVn??"")>=0).ToList();
                returnList = s;
            }
            else
            {
               var s = DataContext.Get(x => ((x.Code ?? "").IndexOf(criteria.Code ?? "") >= 0)
               || ((x.NameEn ?? "").IndexOf(criteria.NameEn ?? "null") >= 0)
               || ((x.NameVn ?? "").IndexOf(criteria.NameVn ?? "null") >= 0)).ToList();
                returnList = s;
            }
            rowsCount = returnList.Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                returnList = returnList.Skip((page - 1) * size).Take(size).ToList();
            }
            return returnList;        
        }

        public List<CatCountry> Query(CatCountryCriteria criteria)
        {
            var returnList = new List<CatCountry>();
            if (criteria.condition == SearchCondition.AND)
            {
                var s = DataContext.Get(x => ((x.Code ?? "").IndexOf(criteria.Code ?? "") >= 0)
                && (x.NameEn ?? "").IndexOf(criteria.NameEn ?? "") >= 0
                && (x.NameVn ?? "").IndexOf(criteria.NameVn ?? "") >= 0).ToList();
                returnList = s;
            }
            else
            {
                var s = DataContext.Get(x => ((x.Code ?? "").IndexOf(criteria.Code ?? "") >= 0)
                || ((x.NameEn ?? "").IndexOf(criteria.NameEn ?? "null") >= 0)
                || ((x.NameVn ?? "").IndexOf(criteria.NameVn ?? "null") >= 0)).ToList();
                returnList = s;
            }
           
            return returnList;
        }

        private List<CatCountryViewModel> GetDataByLanguage(IQueryable<CatCountry> data)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var results = new List<CatCountryViewModel>();
            if (currentCulture.Name == "vi-VN")
            {
                foreach (var item in data)
                {
                    var country = new CatCountryViewModel
                    {
                        Id = item.Id,
                        Name = item.NameVn,
                        UserCreated = item.UserCreated,
                        DatetimeCreated = item.DatetimeCreated,
                        UserModified = item.UserModified,
                        DatetimeModified = item.DatetimeModified,
                        Inactive = item.Inactive,
                        InactiveOn = item.InactiveOn
                    };
                    results.Add(country);
                }
            }
            else
            {
                foreach (var item in data)
                {
                    var country = new CatCountryViewModel
                    {
                        Id = item.Id,
                        Name = item.NameEn,
                        UserCreated = item.UserCreated,
                        DatetimeCreated = item.DatetimeCreated,
                        UserModified = item.UserModified,
                        DatetimeModified = item.DatetimeModified,
                        Inactive = item.Inactive,
                        InactiveOn = item.InactiveOn
                    };
                    results.Add(country);
                }
            }
            return results;
        }
    }
}
