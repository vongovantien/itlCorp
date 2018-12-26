using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Helpers;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
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

        public List<CatCountryImportModel> CheckValidImport(List<CatCountryImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var countries = dc.CatCountry.ToList();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.NameEn))
                {
                    item.NameEn = string.Format("Name En is not allow empty!|wrong");
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVn = string.Format("Name Vn is not allow empty!|wrong");
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = string.Format("Code is not allow empty!|wrong");
                    item.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(x => x.Code.ToLower()==item.Code.ToLower());
                    if(country != null)
                    {
                        item.Code = string.Format("Code {0} has been existed!|wrong", item.Code);
                        item.IsValid = false;
                    }
                    if(list.Count(x => (x.Code??"").IndexOf(item.Code ??"", StringComparison.OrdinalIgnoreCase) >=0) > 1)
                    {
                        item.Code = string.Format("Code {0} has been duplicated!|wrong", item.Code);
                        item.IsValid = false;
                    }
                }
            });
            return list;
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

        public HandleState Import(List<CatCountryImportModel> data)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach (var item in data)
                {
                    DateTime? inactive = null;
                    var country = new CatCountry
                    {
                        Code = item.Code,
                        NameEn = item.NameEn,
                        NameVn = item.NameVn,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = ChangeTrackerHelper.currentUser,
                        Inactive = (item.Status ?? "").Contains("active"),
                        InactiveOn = item.Status != null? DateTime.Now: inactive
                    };
                    dc.CatCountry.Add(country);
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
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
