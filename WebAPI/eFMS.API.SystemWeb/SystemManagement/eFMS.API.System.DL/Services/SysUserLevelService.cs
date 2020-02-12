using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysUserLevelService : RepositoryBase<SysUserLevel, SysUserLevelModel>, ISysUserLevelService
    {
        private readonly IContextBase<SysEmployee> employeeRepository;
        private readonly IContextBase<SysUser> userRepository;
        private readonly ICurrentUser currentUser;

        public SysUserLevelService(IContextBase<SysUserLevel> repository,
            IMapper mapper,
            IContextBase<SysEmployee> employeeRepo,
            IContextBase<SysUser> userRepo, ICurrentUser user
            ) : base(repository, mapper)
        {
            employeeRepository = employeeRepo;
            userRepository = userRepo;
            currentUser = user;

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

        public IQueryable<SysUserLevel> Query(SysUserLevelCriteria criteria)
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
            if (criteria.Type == "office")
            {
                results = results.Where(x => x.CompanyId == criteria.CompanyId && x.OfficeId == criteria.OfficeId);
            }
            else if(criteria.Type == "company")
            {
                results = results.Where(x => x.CompanyId == criteria.CompanyId);
            }
            else if(criteria.Type == "department")
            {
                results = results.Where(x => x.CompanyId == criteria.CompanyId && x.OfficeId == criteria.OfficeId && x.DepartmentId == criteria.DepartmentId);
            }
            else if(criteria.Type == "group")
            {
                results = results.Where(x => x.CompanyId == criteria.CompanyId && x.OfficeId == criteria.OfficeId && x.DepartmentId == criteria.DepartmentId && x.GroupId == criteria.GroupId);
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
                        if(item.Id != 0)
                        {
                            item.UserModified = currentUser.UserID;
                            item.DatetimeModified = DateTime.Now;
                            hsUser = DataContext.Update(item, x => x.Id == item.Id);
                        }
                        else
                        {
                            item.Active = true;
                            item.GroupId = item.GroupId == 0 ? Constants.SpecialGroup : item.GroupId;
                            item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                            item.UserCreated = item.UserModified = currentUser.UserID;
                            hsUser = DataContext.Add(item,true);
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
    }
}
