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
using ITL.NetCore.Common;
using eFMS.API.System.DL.Common;
using Microsoft.Extensions.Caching.Distributed;
using eFMS.API.System.Service.Contexts;
using Microsoft.Extensions.Localization;

namespace eFMS.API.System.DL.Services
{
    public class SysUserService : RepositoryBase<SysUser, SysUserModel>, ISysUserService
    {
        private readonly IContextBase<SysEmployee> employeeRepository;
        private readonly IContextBase<SysUserGroup> usergroupRepository;
        private readonly IDistributedCache cache;
        private readonly IStringLocalizer stringLocalizer;

        public SysUserService(IContextBase<SysUser> repository, IMapper mapper,
            IContextBase<SysEmployee> employeeRepo,
            IContextBase<SysUserGroup> usergroupRepo, IDistributedCache distributedCache) : base(repository, mapper)
        {
            employeeRepository = employeeRepo;
            usergroupRepository = usergroupRepo;
            cache = distributedCache;
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
                data = data.Where(x => (x.x.Username ?? "").IndexOf(criteria.Username ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          && (x.y.EmployeeNameEn ?? "").IndexOf(criteria.EmployeeNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          && (x.y.Title ?? "").IndexOf(criteria.Title ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          && (x.x.UserType ?? "").IndexOf(criteria.UserType ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          && (x.x.Active == criteria.Active || criteria.Active == null)
                );
            }
            else
            {
                if(criteria.All.Contains("Active"))
                {
                    criteria.Active = true;
                }
                if(criteria.All.Contains("InActive"))
                {
                    criteria.Active = false;
                }
                data = data.Where(x => (
                          ((x.x.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                          || (x.y.EmployeeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          || (x.y.Title ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          || (x.x.UserType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          || (x.x.Active == criteria.Active)
                          ));
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
                model.Title = item.y.Title;
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
                data = data.Where(x => (x.x.Username ?? "").IndexOf(criteria.Username ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                         && (x.y.EmployeeNameEn ?? "").IndexOf(criteria.EmployeeNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                         && (x.y.Title ?? "").IndexOf(criteria.Title ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                         && (x.x.UserType ?? "").IndexOf(criteria.UserType ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                         && (x.x.Active == criteria.Active || criteria.Active == null));
            }
            else
            {
                if (criteria.All.Contains("Active"))
                {
                    criteria.Active = true;
                }
                if (criteria.All.Contains("InActive"))
                {
                    criteria.Active = false;
                }
                data = data.Where(x => (
                          ((x.x.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                          || (x.y.EmployeeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          || (x.y.Title ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          || (x.x.UserType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          || (x.x.Active == criteria.Active)
                          ));
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

        public List<SysUserImportModel> CheckValidImport(List<SysUserImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            list.ForEach(item =>
            {
                //check empty username
                string userName = item.Username;
                item.UsernameValid = true;
                if (string.IsNullOrEmpty(userName))
                {
                    item.Username = stringLocalizer[LanguageSub.MSG_USER_USERNAME_EMPTY];
                    item.IsValid = false;
                    item.UsernameValid = false;
                }
                //check empty Name EN
                string nameEN = item.EmployeeNameEn;
                item.EmployeeNameEnValid = true;
                if (string.IsNullOrEmpty(nameEN))
                {
                    item.EmployeeNameEn = stringLocalizer[LanguageSub.MSG_USER_NAMEEN_EMPTY];
                    item.IsValid = false;
                    item.EmployeeNameEnValid = false;
                }

                //check empty Full Name
                string nameVN = item.EmployeeNameVn;
                item.EmployeeNameVnValid = true;
                if (string.IsNullOrEmpty(nameVN))
                {
                    item.EmployeeNameVn = stringLocalizer[LanguageSub.MSG_USER_NAMEVN_EMPTY];
                    item.IsValid = false;
                    item.EmployeeNameVnValid = false;
                }

                //check empty and valid User Type
                string userType = item.UserType;
                item.UserTypeValid = true;
                if (string.IsNullOrEmpty(userType))
                {
                    item.UserType = stringLocalizer[LanguageSub.MSG_USER_USERTYPE_EMPTY];
                    item.IsValid = false;
                    item.UserTypeValid = false;
                }
                else if(!userType.Equals("Normal User") && !userType.Equals("Local Admin") && !userType.Equals("Super Admin"))
                {
                    item.UserType = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                    item.IsValid = false;
                    item.UserTypeValid = false;
                }

                //check empty and valid working status
                string workingStatus = item.WorkingStatus;
                item.WorkingStatusValid = true;
                if (string.IsNullOrEmpty(workingStatus))
                {
                    item.WorkingStatus = stringLocalizer[LanguageSub.MSG_USER_WORKINGSTATUS_EMPTY];
                    item.IsValid = false;
                    item.WorkingStatusValid = false;
                }else if(!workingStatus.Equals("Working") && !workingStatus.Equals("Maternity leave") && !workingStatus.Equals("Off"))
                {
                    item.WorkingStatus = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                    item.IsValid = false;
                    item.WorkingStatusValid = false;
                }

                //check empty and valid status 
                string status = item.Status;
                item.StatusValid = true;
                if (string.IsNullOrEmpty(status))
                {
                    item.Status = stringLocalizer[LanguageSub.MSG_USER_STATUS_EMPTY];
                    item.IsValid = false;
                    item.StatusValid = false;
                }
                else if(!status.Equals("Active") && !status.Equals("Inactive"))
                {
                    item.Status = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                    item.IsValid = false;
                    item.StatusValid = false;
                }

            });
            return list;
        }


        public HandleState Insert(SysUserModel sysUserModel)
        {
            try
            {
                return DataContext.Add(sysUserModel);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }

        }

        public HandleState Update(SysUserModel model)
        {
            try
            {
                var entity = mapper.Map<SysUser>(model);
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
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

    }
}
