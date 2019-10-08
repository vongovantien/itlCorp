using AutoMapper;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using ITL.NetCore.Connection.BL;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.IService;
using System.Linq;
using eFMS.API.Common.Helpers;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.Models.Criteria;
using System;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.Common.NoSql;

namespace eFMS.API.System.DL.Services
{
    public class SysBranchService :  RepositoryBase<SysBranch, SysBranchModel>, ISysBranchService
    {
        private readonly IDistributedCache cache;
        private readonly IContextBase<SysBu> sysBuRepository;
 


        public SysBranchService(IContextBase<SysBranch> repository, IMapper mapper, IContextBase<SysBu> sysBuRepo, IDistributedCache distributedCache) : base(repository, mapper)
        {
            sysBuRepository = sysBuRepo;
            cache = distributedCache;

        }

        public HandleState AddBranch(SysBranchModel  sysBranch)
        {
            return DataContext.Add(sysBranch);
        }

        public IQueryable<SysBranch> GetBranchs()
        {
            //var lstSysBranch = RedisCacheHelper.GetObject<List<SysBranch>>(cache, Templates.SysBranch.NameCaching.ListName);
            var lstSysBranch = new List<SysBranch>();
            IQueryable<SysBranch> data = null;
            if (lstSysBranch != null)
            {
                //data = lstSysBranch.AsQueryable();
                data = DataContext.Get();

            }
            else
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.SysBranch.NameCaching.ListName, data);
            }
            var results = data?.Select(x => mapper.Map<SysBranchModel>(x));
            return results;
        }

        public IQueryable<SysBranchViewModel> Paging(SysBranchCriteria criteria, int page, int size, out int rowsCount)
        {
            List<SysBranchViewModel> results = null;
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0;
                return results.AsQueryable();
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

        public List<SysBranchViewModel> Query(SysBranchCriteria criteria)
        {
            var sysBranchs = GetBranchs();
            var sysBu = sysBuRepository.Get();
            var query = (from branch in sysBranchs
                         join bu in sysBu on branch.Buid equals bu.Id
                         select new { branch, companyName = bu.Code });

            if (criteria.All == null)
            {
                query = query.Where(x =>
                           (x.branch.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.branch.BranchNameEn ?? "").IndexOf(criteria.BranchNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.branch.BranchNameVn ?? "").IndexOf(criteria.BranchNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.branch.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.branch.Taxcode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.branch.Buid == criteria.Buid || criteria.Buid == Guid.Empty)
                           );
            }
            else
            {
                query = query.Where(x =>
                             (x.branch.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.branch.BranchNameEn ?? "").IndexOf(criteria.BranchNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.branch.BranchNameVn ?? "").IndexOf(criteria.BranchNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.branch.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.branch.Taxcode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.branch.Buid == criteria.Buid || criteria.Buid == Guid.Empty)
                            );
            }
            if (query.Count() == 0) return null;
            List<SysBranchViewModel> results = new List<SysBranchViewModel>();
            foreach (var item in query)
            {
                var sysBranch = mapper.Map<SysBranchViewModel>(item.branch);
                results.Add(sysBranch);
            }
            return results;
        }

        public HandleState UpdateBranch(SysBranchModel model)
        {
            var entity = mapper.Map<SysBranch>(model);
            if (entity.Inactive == true)
            {
                entity.InactiveOn = DateTime.Now;
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                cache.Remove(Templates.SysBranch.NameCaching.ListName);
            }
            return hs;
        }

        public HandleState DeleteBranch(Guid id)
        {
            //ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                cache.Remove(Templates.SysBranch.NameCaching.ListName);
            }
            return hs;
        }











    }
}
