using AutoMapper;
using eFMS.API.Common.Helpers;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Setting.DL.Services
{
    public class TariffService : RepositoryBase<SetTariff, SetTariffModel>, ITariffService
    {
        private readonly ICurrentUser currentUser;
        private readonly IDistributedCache cache;

        public TariffService(IContextBase<SetTariff> repository, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
        }


        public List<SetTariffModel> GetAllTariff()
        {
            //var clearanceCaching = RedisCacheHelper.GetObject<List<SetTariffModel>>(cache, Templates.CustomDeclaration.NameCaching.ListName);
            List<SetTariffModel> setTariffModels = null;
            //get from view data
            var list = DataContext.Get();
            setTariffModels = mapper.Map<List<SetTariffModel>>(list);
            //RedisCacheHelper.SetObject(cache, Templates.CustomDeclaration.NameCaching.ListName, customClearances);
            return setTariffModels;
        }


        public List<TariffViewModel> Query(TariffCriteria criteria)
        {
            var tariff = GetAllTariff();
            var query = from t in tariff
                        select t;
            query = query.Where(x =>
            ((x.TariffName ?? "").IndexOf(criteria.Name ?? "", StringComparison.OrdinalIgnoreCase)) >= 0
            && (x.CustomerId ?? "").IndexOf(criteria.CustomerID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            && (x.TariffType == criteria.TariffType || string.IsNullOrEmpty(criteria.TariffType))
            && (x.ServiceMode == criteria.ServiceMode || string.IsNullOrEmpty(criteria.ServiceMode))
            && (x.SupplierId == criteria.SupplierID || string.IsNullOrEmpty(criteria.SupplierID))
            && (x.OfficeId == criteria.OfficeId || criteria.OfficeId == Guid.Empty)
            && (x.Status == criteria.Status || criteria.Status == null));
            if (criteria.DateType == "CreateDate")
            {
                query = query.Where(x =>
                (x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated <= criteria.ToDate));
            }
            else if (criteria.DateType == "EffectiveDate")
            {
                query = query.Where(x =>
                (x.EffectiveDate >= criteria.FromDate && x.EffectiveDate <= criteria.ToDate));
            }
            else if(criteria.DateType == "ModifiedDate")
            {
               query = query.Where(x =>
               (x.DatetimeModified >= criteria.FromDate && x.DatetimeModified <= criteria.ToDate));
            }
            else
            {
                query = query.Where(x =>
                   ((x.DatetimeCreated >= criteria.FromDate || criteria.FromDate == null) &&
                   (x.DatetimeCreated <= criteria.ToDate || criteria.ToDate == null))
                   || ((x.EffectiveDate >= criteria.FromDate || criteria.FromDate == null)
                   && (x.EffectiveDate <= criteria.ToDate || criteria.ToDate == null))
                   || ((x.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                   && (x.DatetimeModified <= criteria.ToDate || criteria.ToDate == null))
                  );
            }

            if (query.Count() == 0) return null;
            List<TariffViewModel> results = new List<TariffViewModel>();
            foreach (var item in query)
            {
                var tariffView = mapper.Map<TariffViewModel>(item);
                results.Add(tariffView);
            }
            return results;
        }

        public IQueryable<TariffViewModel> Paging(TariffCriteria criteria, int page, int size, out int rowsCount)
        {
            List<TariffViewModel> results = null;
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0;
                return null;
            }
            list = list.OrderByDescending(x => x.DatetimeCreated).ToList();
            rowsCount = list.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = list.Skip((page - 1) * size).Take(size).ToList();
            }
            return results.AsQueryable();
        }



    }
}
