using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
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
    public class CatAreaService : RepositoryBase<CatArea, CatAreaModel>, ICatAreaService
    {
        public CatAreaService(IContextBase<CatArea> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<CatAreaViewModel> GetByLanguage()
        {
            var data = DataContext.Get();
            return GetDataByLanguage(data);
        }

        private List<CatAreaViewModel> GetDataByLanguage(IQueryable<CatArea> data)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var results = new List<CatAreaViewModel>();
            if (currentCulture.Name == "vi-VN")
            {
                foreach (var item in data)
                {
                    var area = new CatAreaViewModel
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
                    results.Add(area);
                }
            }
            else
            {
                foreach (var item in data)
                {
                    var area = new CatAreaViewModel
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
                    results.Add(area);
                }
            }
            return results;
        }
    }
}
