using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
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

        #region Add User 
        public HandleState AddUser(List<SysUserLevelModel> users)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    users.ForEach(x => { x.GroupId = 11; x.DatetimeCreated = x.DatetimeModified = DateTime.Now; x.UserCreated = x.UserModified = currentUser.UserID; }) ;
                    var hsUser = DataContext.Add(users);
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
