using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Helpers;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatAreaService : RepositoryBase<CatArea, CatAreaModel>, ICatAreaService
    {
        private readonly IDistributedCache cache;
        public CatAreaService(IContextBase<CatArea> repository, IMapper mapper, IDistributedCache distributedCache) : base(repository, mapper)
        {
            cache = distributedCache;
        }

        public List<CatAreaViewModel> GetByLanguage()
        {
            var data = GetAll();
            if (data == null) return null;
            else
            {
                data = data.OrderBy(x => x.NameEn);
                return GetDataByLanguage(data);
            }
        }

        private IQueryable<CatArea> GetAll()
        {
            var lstCatArea = RedisCacheHelper.GetObject<List<CatArea>>(cache, Templates.CatArea.NameCaching.ListName);
            IQueryable<CatArea> data = null;
            if (lstCatArea != null)
            {
                data = lstCatArea.AsQueryable();
            }
            else
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.CatArea.NameCaching.ListName, data.ToList());
            }
            return data;
            //var data = RedisCacheHelper.GetAll(cache, Templates.CatArea.NameCaching.ListName, DataContext.Get());
            //return data;
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
                        Active = item.Active,
                        InActiveOn = item.InActiveOn
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
                        Active = item.Active,
                        InActiveOn = item.InActiveOn
                    };
                    results.Add(area);
                }
            }
            return results;
        }
    }
}
