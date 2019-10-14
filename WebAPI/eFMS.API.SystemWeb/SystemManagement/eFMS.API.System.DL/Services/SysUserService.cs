using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;

namespace eFMS.API.System.DL.Services
{
    public class SysUserService : RepositoryBase<SysUser, SysUserModel>, ISysUserService
    {
        private readonly IContextBase<SysEmployee> employeeRepository;
        private readonly IContextBase<SysUserGroup> usergroupRepository;

        public SysUserService(IContextBase<SysUser> repository, IMapper mapper, 
            IContextBase<SysEmployee> employeeRepo,
            IContextBase<SysUserGroup> usergroupRepo) : base(repository, mapper)
        {
            employeeRepository = employeeRepo;
            usergroupRepository = usergroupRepo;
        }

        public List<SysUserViewModel> GetAll()
        {
            var users = DataContext.Get();
            var employees = employeeRepository.Get();
            var data = users.Join(employees, x => x.EmployeeId, y => y.Id, (x, y) => new { x, y });
            List<SysUserViewModel> results = new List<SysUserViewModel>();
            foreach (var item in data)
            {
                var model = mapper.Map<SysUserViewModel>(item.x);
                model.EmployeeNameEn = item.y.EmployeeNameEn;
                model.EmployeeNameVn = item.y.EmployeeNameVn;
                results.Add(model);
            }
            return results;
        }

        public SysUserViewModel GetUserById(string Id)
        {
            var user = DataContext.Get(x => x.Id == Id).FirstOrDefault();
            if (user == null)
            {
                return null;
            }
            var employee = employeeRepository.Get(x => x.Id == user.EmployeeId).FirstOrDefault();
            var result = mapper.Map<SysUserViewModel>(user);
            result.EmployeeNameEn = employee?.EmployeeNameEn;
            result.EmployeeNameVn = employee?.EmployeeNameVn;
            return result;
        }
        public IQueryable<SysUserViewModel> Paging(SysUserCriteria criteria, int page, int size, out int rowsCount)
        {
            var users = DataContext.Get();
            var employees = employeeRepository.Get();
            var data = users.Join(employees, x => x.EmployeeId, y => y.Id, (x, y) => new { x, y });
            if (criteria.All == null)
            {
                data = data.Where(x => (x.x.Username ?? "").IndexOf(criteria.Username ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            else
            {
                data = data.Where(x => (x.x.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            rowsCount = data.Count();
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            List<SysUserViewModel> results = new List<SysUserViewModel>();
            foreach (var item in data)
            {
                var model = mapper.Map<SysUserViewModel>(item.x);
                model.EmployeeNameEn = item.y.EmployeeNameEn;
                model.EmployeeNameVn = item.y.EmployeeNameVn;
                results.Add(model);
            }
            return results.AsQueryable();
        }

        public IQueryable<SysUserViewModel> Query(SysUserCriteria criteria)
        {
            var users = DataContext.Get();
            var employees = employeeRepository.Get();
            var data = users.Join(employees, x => x.EmployeeId, y => y.Id, (x, y) => new { x, y });
            if (criteria.All == null)
            {
                data = data.Where(x => (x.x.Username ?? "").IndexOf(criteria.Username ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            else
            {
                data = data.Where(x => (x.x.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            List<SysUserViewModel> results = new List<SysUserViewModel>();
            foreach (var item in data)
            {
                var model = mapper.Map<SysUserViewModel>(item.x);
                model.EmployeeNameEn = item.y.EmployeeNameEn;
                model.EmployeeNameVn = item.y.EmployeeNameVn;
                results.Add(model);
            }
            return results.AsQueryable();
        }
    }
}
