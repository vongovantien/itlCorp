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
    public class SysOfficeService : RepositoryBase<SysOffice, SysOfficeModel>, ISysOfficeService
    {
        private readonly IDistributedCache cache;
        private readonly IContextBase<SysCompany> sysBuRepository;



        public SysOfficeService(IContextBase<SysOffice> repository, IMapper mapper, IContextBase<SysCompany> sysBuRepo, IDistributedCache distributedCache) : base(repository, mapper)
        {
            sysBuRepository = sysBuRepo;
            cache = distributedCache;
            SetChildren<CatDepartment>("Id", "BranchId");
        }

        public HandleState AddOffice(SysOfficeModel SysOffice)
        {
            SysOffice.UserCreated = "admin";
            SysOffice.DatetimeCreated = DateTime.Now;
            return DataContext.Add(SysOffice);
        }

        public IQueryable<SysOffice> GetOffices()
        {
            //var lstSysOffice = RedisCacheHelper.GetObject<List<SysOffice>>(cache, Templates.SysBranch.NameCaching.ListName);
            var lstSysOffice = new List<SysOffice>(); 
            IQueryable<SysOffice> data = null;
            if (lstSysOffice != null)
            {
                //data = lstSysOffice.AsQueryable();
                data = DataContext.Get();

            }
            else
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.SysBranch.NameCaching.ListName, data);
            }
            var results = data?.Select(x => mapper.Map<SysOfficeModel>(x));
            return results;
        }

        public IQueryable<SysOfficeViewModel> Paging(SysOfficeCriteria criteria, int page, int size, out int rowsCount)
        {
            List<SysOfficeViewModel> results = null;
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

        public List<SysOfficeViewModel> Query(SysOfficeCriteria criteria)
        {
            var SysOffices = GetOffices();
            var sysBu = sysBuRepository.Get();
            var query = (from branch in SysOffices
                         join bu in sysBu on branch.Buid equals bu.Id
                         select new { branch, companyName = bu.Code });

            if (criteria.All == null)
            {
                query = query.Where(x =>
                           ((x.branch.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase)) >= 0
                           && (x.branch.BranchNameEn ?? "").IndexOf(criteria.BranchNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.branch.BranchNameVn ?? "").IndexOf(criteria.BranchNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.branch.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.branch.Taxcode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.companyName ?? "").IndexOf(criteria.CompanyName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.branch.Active == criteria.Active || criteria.Active == null));
            }
            else
            {
                query = query.Where(x => (
                            ((x.branch.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                            || (x.branch.BranchNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.branch.BranchNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.branch.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            || (x.branch.Taxcode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            && (x.companyName ?? "").IndexOf(criteria.CompanyName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                            && (x.branch.Active == criteria.Active || criteria.Active == null)
                            ));
            }
            if (query.Count() == 0) return null;
            List<SysOfficeViewModel> results = new List<SysOfficeViewModel>();
            foreach (var item in query)
            {
                var SysOffice = mapper.Map<SysOfficeViewModel>(item.branch);
                SysOffice.CompanyName = item.companyName;
                results.Add(SysOffice);
            }
            return results;
        }

        public HandleState UpdateOffice(SysOfficeModel model)
        {

            var entity = mapper.Map<SysOffice>(model);
            entity.UserModified = "admin";
            entity.DatetimeModified = DateTime.Now;
            if (entity.Active == true)
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

        public HandleState DeleteOffice(Guid id)
        {
            //ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                cache.Remove(Templates.SysBranch.NameCaching.ListName);
            }
            return hs;
        }

        public IQueryable<SysOfficeViewModel> GetOfficeByCompany(Guid id)
        {
            var lstSysOffice = DataContext.Where( office => office.Buid == id);
            var sysBu = sysBuRepository.Get();

            //join với company.
            var query = (from branch in lstSysOffice
                         join bu in sysBu on branch.Buid equals bu.Id
                         select new { branch, companyName = bu.BunameEn });
            var result = query.Select(item => new SysOfficeViewModel
            {
                Id = item.branch.Id,
                BranchNameEn = item.branch.BranchNameEn,
                BranchNameVn = item.branch.BranchNameVn,
                AddressEn = item.branch.AddressEn,
                AddressVn = item.branch.AddressVn,
                CompanyName = item.companyName,
                Inactive = item.branch.Active,
                ShortName = item.branch.ShortName
            });

            return result;
        }












    }
}
