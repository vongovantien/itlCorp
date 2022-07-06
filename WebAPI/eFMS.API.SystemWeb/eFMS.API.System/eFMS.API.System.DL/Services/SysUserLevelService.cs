﻿using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace eFMS.API.System.DL.Services
{
    public class SysUserLevelService : RepositoryBase<SysUserLevel, SysUserLevelModel>, ISysUserLevelService
    {
        private readonly IContextBase<SysEmployee> employeeRepository;
        private readonly IContextBase<SysUser> userRepository;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysGroup> groupRepository;
        private readonly IContextBase<SysOffice> officeRepository;
        private readonly IContextBase<CatDepartment> departmentRepository;
        private readonly IContextBase<SysCompany> companyRepository;


        public SysUserLevelService(IContextBase<SysUserLevel> repository,
            IMapper mapper,
            IContextBase<SysEmployee> employeeRepo,
            IContextBase<SysUser> userRepo, ICurrentUser user,
            IContextBase<SysGroup> groupRepo,
            IContextBase<SysOffice> officeRepo,
            IContextBase<CatDepartment> departmentRepo,
            IContextBase<SysCompany> companyRepo

            ) : base(repository, mapper)
        {
            employeeRepository = employeeRepo;
            userRepository = userRepo;
            currentUser = user;
            groupRepository = groupRepo;
            officeRepository = officeRepo;
            departmentRepository = departmentRepo;
            companyRepository = companyRepo;
        }

        public IQueryable<SysUserLevelModel> GetByLevel(short groupId)
        {
            var userGroups = DataContext.Get(x => x.GroupId == groupId && x.Active == true);
            var users = userRepository.Get();
            var employess = employeeRepository.Get();
            var results = userGroups.Join(users, x => x.UserId, y => y.Id, (x, y) => new { User = y, UserGroup = x })
                                    .Join(employess, x => x.User.EmployeeId, y => y.Id, (x, y) => new { User = x, Employee = y })
                                    .Select(x => new SysUserLevelModel
                                    {
                                        Id = x.User.UserGroup.Id,
                                        UserId = x.User.UserGroup.UserId,
                                        GroupId = x.User.UserGroup.GroupId,
                                        UserName = x.User.User.Username,
                                        EmployeeName = x.Employee.EmployeeNameVn,
                                        Active = x.User.User.Active
                                    });
            return results;
        }

        public SysUserLevelModel GetDetail(int id)
        {
            var data = DataContext.Get(x => x.Id == id).FirstOrDefault();
            return mapper.Map<SysUserLevelModel>(data);
        }

        public IQueryable<SysUserLevelModel> GetByUserId(string id)
        {
            var data = DataContext.Get(x => x.UserId == id);
            var groups = groupRepository.Get();
            var offices = officeRepository.Get();
            var companies = companyRepository.Get();
            var departments = departmentRepository.Get();
            var results = from d in data
                          join g in groups on d.GroupId equals g.Id into groupss
                          from g in groupss.DefaultIfEmpty()
                          join o in offices on d.OfficeId equals o.Id into office
                          from o in office.DefaultIfEmpty()
                          join c in companies on d.CompanyId equals c.Id into company
                          from c in company.DefaultIfEmpty()
                          join depart in departments on d.DepartmentId equals depart.Id into departs
                          from depart in departs.DefaultIfEmpty()
                          select new SysUserLevelModel
                          {
                              Id = d.Id,
                              CompanyId = d.CompanyId,
                              OfficeId = d.OfficeId,
                              DepartmentId = d.DepartmentId,
                              GroupId = d.GroupId,
                              GroupName = g.NameVn,
                              CompanyName = c.BunameVn,
                              OfficeName = o.BranchNameVn,
                              DepartmentName = depart.DeptName,
                              Position = d.Position,
                              GroupAbbrName = g.ShortName,
                              CompanyAbbrName = c.BunameAbbr,
                              OfficeAbbrName = o.ShortName,
                              DepartmentAbbrName = depart.DeptNameAbbr,
                              IsDefault = d.IsDefault,
                              UserId = d.UserId,
                              Active = d.Active,
                              UserCreated = d.UserCreated,
                              UserModified = d.UserModified,
                              DatetimeCreated = d.DatetimeCreated,
                              DatetimeModified = d.DatetimeModified,
                          };

            return results;
        }

        public IQueryable<SysUserLevelModel> Query(SysUserLevelCriteria criteria)
        {
            var userLevels = DataContext.Get(x => x.Active == true && x.CompanyId == criteria.CompanyId);
            var users = userRepository.Get();
            var employess = employeeRepository.Get();
            var results = userLevels.Join(users, x => x.UserId, y => y.Id, (x, y) => new { User = y, UserLevel = x })
                        .Join(employess, x => x.User.EmployeeId, y => y.Id, (x, y) => new { User = x, Employee = y })
                        .Select(x => new SysUserLevelModel
                        {
                            Id = x.User.UserLevel.Id,
                            UserId = x.User.UserLevel.UserId,
                            UserName = x.User.User.Username,
                            GroupId = x.User.UserLevel.GroupId,
                            EmployeeName = x.Employee.EmployeeNameVn,
                            Active = x.User.UserLevel.Active,
                            CompanyId = x.User.UserLevel.CompanyId,
                            OfficeId = x.User.UserLevel.OfficeId,
                            DepartmentId = x.User.UserLevel.DepartmentId,
                            DatetimeCreated = x.User.UserLevel.DatetimeCreated,
                            DatetimeModified = x.User.UserLevel.DatetimeModified,
                            UserCreated = x.User.UserLevel.UserCreated,
                            UserModified = x.User.UserLevel.UserModified,
                            Position = x.User.UserLevel.Position
                        });
            if (criteria.Type == "company")
            {
                results = results.Where(x => x.GroupId == SystemConstants.SpecialGroup && x.OfficeId == null && x.DepartmentId == null);
            }
            else if (criteria.Type == "office")
            {
                results = results.Where(x => x.OfficeId == criteria.OfficeId && x.GroupId == SystemConstants.SpecialGroup && x.DepartmentId == null);
            }
            else if (criteria.Type == "department")
            {
                results = results.Where(x => x.OfficeId == criteria.OfficeId && x.DepartmentId == criteria.DepartmentId && x.GroupId == SystemConstants.SpecialGroup);
            }
            else if (criteria.Type == "group")
            {
                results = results.Where(x => x.OfficeId == criteria.OfficeId && x.DepartmentId == criteria.DepartmentId && x.GroupId == criteria.GroupId);
            }

            return results;

        }

        #region Add User 
        public HandleState AddUser(List<SysUserLevelModel> users)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hsUser = new HandleState();
                    foreach (var item in users)
                    {
                        if (item.Id != 0)
                        {
                            item.UserModified = currentUser.UserID;
                            item.DatetimeModified = DateTime.Now;
                            hsUser = DataContext.Update(item, x => x.Id == item.Id);
                        }
                        else
                        {
                            item.Active = true;
                            item.GroupId = item.GroupId == 0 ? SystemConstants.SpecialGroup : item.GroupId;
                            item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                            item.UserCreated = item.UserModified = currentUser.UserID;
                            hsUser = DataContext.Add(item, true);
                        }
                    }
                    trans.Commit();
                    return hsUser;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
        #endregion

        public IQueryable<SysUserLevelModel> GetUsersByType(UserLevelCriteria criteria)
        {
            var userLevels = DataContext.Get(x => x.Active == true);
            var users = userRepository.Get();
            var employess = employeeRepository.Get();
            var results = userLevels.Join(users, x => x.UserId, y => y.Id, (x, y) => new { User = y, UserLevel = x })
                        .Join(employess, x => x.User.EmployeeId, y => y.Id, (x, y) => new { User = x, Employee = y })
                        .Select(x => new SysUserLevelModel
                        {
                            Id = x.User.UserLevel.Id,
                            UserId = x.User.UserLevel.UserId,
                            UserName = x.User.User.Username,
                            GroupId = x.User.UserLevel.GroupId,
                            EmployeeName = x.Employee.EmployeeNameVn,
                            Active = x.User.UserLevel.Active,
                            CompanyId = x.User.UserLevel.CompanyId,
                            OfficeId = x.User.UserLevel.OfficeId,
                            DepartmentId = x.User.UserLevel.DepartmentId,
                            DatetimeCreated = x.User.UserLevel.DatetimeCreated,
                            DatetimeModified = x.User.UserLevel.DatetimeModified,
                            UserCreated = x.User.UserLevel.UserCreated,
                            UserModified = x.User.UserLevel.UserModified,
                            Position = x.User.UserLevel.Position
                        });
            if (criteria.Type == "company")
            {
                results = results.Where(x => x.CompanyId == criteria.CompanyId);
            }
            else if (criteria.Type == "office")
            {
                results = results.Where(x => (x.CompanyId == criteria.CompanyId || criteria.CompanyId == null) && x.OfficeId == criteria.OfficeId);
            }
            else if (criteria.Type == "department")
            {
                results = results.Where(x => x.CompanyId == criteria.CompanyId && x.OfficeId == criteria.OfficeId && x.DepartmentId == criteria.DepartmentId);
            }
            else if (criteria.Type == "group")
            {
                results = results.Where(x => x.CompanyId == criteria.CompanyId && x.OfficeId == criteria.OfficeId && x.DepartmentId == criteria.DepartmentId && x.GroupId == criteria.GroupId);
            }
            else if (criteria.Type == "owner")
            {
                results = results.Where(x => x.CompanyId == criteria.CompanyId && x.OfficeId == criteria.OfficeId && x.DepartmentId == criteria.DepartmentId && x.GroupId == criteria.GroupId && x.UserId == criteria.UserId);
            }
            return results;
        }

        /// <summary>
        /// Get list user info by current user's company
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<SysUserLevelModel> GetListUsersByCurrentCompany(SysUserLevelModel userModel)
        {
            List<SysUserLevelModel> userList = new List<SysUserLevelModel>();
            var companies = companyRepository.Get(x => x.Id == userModel.CompanyId);
            if (companies?.Count() == 0)
            {
                return userList;
            }
            var userLvs = DataContext.Get(x => x.Active == userModel.Active && x.CompanyId == userModel.CompanyId);
            var users = userRepository.Get();
            var groups = groupRepository.Get();
            var offices = officeRepository.Get();
            userList = (from usLV in userLvs
                              join us in users on usLV.UserId equals us.Id into user
                              from us in user.DefaultIfEmpty()
                              join o in offices on usLV.OfficeId equals o.Id into office
                              from o in office.DefaultIfEmpty()
                              join gr in groups on usLV.GroupId equals gr.Id into sysGroup
                              from grp in sysGroup.DefaultIfEmpty()
                              orderby us.Username
                              select new SysUserLevelModel
                              {
                                  UserId = usLV.UserId,
                                  CompanyId = usLV.CompanyId,
                                  OfficeId = usLV.OfficeId,
                                  GroupId = usLV.GroupId,
                                  UserName = us.Username,
                                  GroupName = (grp == null ? string.Empty : grp.ShortName),
                                  OfficeName = (o == null ? string.Empty : o.ShortName)
                              }).ToList();
            return userList;
        }

        public async Task<HandleState> SetDefault(int Id)
        {
            HandleState hs = new HandleState();
            var usrLv = DataContext.Get(x => x.Id == Id)?.FirstOrDefault();

            if(usrLv != null)
            {
                usrLv.IsDefault = true;

                await DataContext.UpdateAsync(usrLv, x => x.Id == Id, false);

                var usrLvLeast = DataContext.Get(x => x.Id != Id && usrLv.UserId == x.UserId)?.ToList();

                if(usrLvLeast.Count > 0)
                {
                    foreach (var item in usrLvLeast)
                    {
                        item.IsDefault = false;
                        await DataContext.UpdateAsync(item, x => x.Id == Id, false);
                    }

                }

                hs = DataContext.SubmitChanges();
            }

            return hs;
        }

        /// <summary>
        /// Ger Data User Info
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserInfoViewModel> GetUserActiveInfo()
        {
            var users = userRepository.Get(x=>x.Active == true);
            var employees = employeeRepository.Get();
            var groups = groupRepository.Get();
            var offices = officeRepository.Get();
            var companies = companyRepository.Get();
            var departments = departmentRepository.Get();
            var datas = from u in users
                        join e in employees on u.EmployeeId equals e.Id into em
                        from e in em.DefaultIfEmpty()
                        join uLv in DataContext.Get() on u.Id equals uLv.UserId
                        join grp in groupRepository.Get() on uLv.GroupId equals grp.Id
                        join dept in departmentRepository.Get() on uLv.DepartmentId equals dept.Id
                        select new UserInfoViewModel
                        {
                            Id = u.Id + "-" + uLv.GroupId + "-" + uLv.DepartmentId,
                            UserId = u.Id,
                            UserName = u.Username,
                            UserGroupId = uLv.GroupId,
                            UserGroupName = grp.ShortName,
                            UserDeparmentId = uLv.DepartmentId,
                            UserDeparmentName = dept.DeptNameAbbr,
                            UserOfficeId = uLv.OfficeId,
                            UserCompanyId = uLv.CompanyId,
                            EmployeeNameEn = e.EmployeeNameEn,
                            EmployeeNameVn = e.EmployeeNameVn
                        };
            datas = datas.OrderBy(x => x.UserName).ThenBy(x => x.UserGroupName);
            return datas;
        }
    }
}
