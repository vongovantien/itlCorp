using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatSalemanService : RepositoryBase<CatSaleman, CatSaleManModel>, ICatSaleManService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;

        public CatSalemanService(IContextBase<CatSaleman> repository, IMapper mapper, IStringLocalizer<LanguageSub> localizer, IDistributedCache distributedCache, ICurrentUser user) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            cache = distributedCache;
            currentUser = user;
        }

        public IQueryable<CatSaleman> GetSaleMan()
        {
            var lstSaleMan = RedisCacheHelper.GetObject<List<CatSaleman>>(cache, Templates.CatSaleMan.NameCaching.ListName);
            IQueryable<CatSaleman> data = null;
            if (lstSaleMan != null)
            {
                data = lstSaleMan.AsQueryable();
            }
            else
            {
                data = DataContext.Get();
                //RedisCacheHelper.SetObject(cache, Templates.CatSaleMan.NameCaching.ListName, data);
            }
            var results = data?.Select(x => mapper.Map<CatSaleManModel>(x));
            return results;
        }

        public List<CatSaleManModel> GetBy(string partnerId)
        {
            List<CatSaleManModel> results = null;
            //var data = DataContext.Get(x => x.JobNo == jobNo);
            var data = GetSaleMan().Where(x => x.PartnerId.Trim() == partnerId);
            if (data.Count() == 0) return results;
            results = mapper.Map<List<CatSaleManModel>>(data);
            return results;
        }

        #region CRUD
        public override HandleState Add(CatSaleManModel entity)
        {
            var saleMan = mapper.Map<CatSaleman>(entity);
            saleMan.CreateDate = DateTime.Now;
            saleMan.UserCreated = "Admin";

            var hs = DataContext.Add(saleMan);
            return hs;
        }

        public HandleState Update(CatSaleManModel model)
        {
            var entity = mapper.Map<CatSaleman>(model);
            entity.UserModified = currentUser.UserID;
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            return hs;
        }
        public HandleState Delete(Guid id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            return hs;
        }
        #endregion

        public List<CatSaleManViewModel> Query(CatSalemanCriteria criteria)
        {
            var salesMan = GetSaleMan().Where(x => x.PartnerId == criteria.PartnerId);
            var query = from saleman in salesMan
                        select new { saleman };
            if (criteria.All == null)
            {
                query = query.Where(x => 
                           (x.saleman.Company ?? "").IndexOf(criteria.Company ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.saleman.Office ?? "").IndexOf(criteria.Office ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.saleman.Status == criteria.Status || criteria.Status == null)
                           );
            }
            else
            {
                query = query.Where(x =>
                             (x.saleman.Company ?? "").IndexOf(criteria.Company ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.saleman.Office ?? "").IndexOf(criteria.Office ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.saleman.Status == criteria.Status || criteria.Status == null)
                            || (x.saleman.PartnerId == criteria.PartnerId)
                            );
            }
            if (query.Count() == 0) return null;
            List<CatSaleManViewModel> results = new List<CatSaleManViewModel>();
            foreach (var item in query)
            {
                var saleMan = mapper.Map<CatSaleManViewModel>(item.saleman);
                results.Add(saleMan);
            }
            return results;
        }

        public List<CatSaleManViewModel> Paging(CatSalemanCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CatSaleManViewModel> results = null;
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0;
                return results;
            }
            rowsCount = list.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = list.Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }

    }
}
