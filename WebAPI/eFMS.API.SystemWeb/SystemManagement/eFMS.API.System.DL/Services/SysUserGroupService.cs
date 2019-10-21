using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysUserGroupService : RepositoryBase<SysUserGroup, SysUserGroupModel>, ISysUserGroupService
    {
        private readonly IContextBase<SysEmployee> employeeRepository;
        private readonly IContextBase<SysUser> userRepository;

        public SysUserGroupService(IContextBase<SysUserGroup> repository, 
            IMapper mapper,
            IContextBase<SysEmployee> employeeRepo,
            IContextBase<SysUser> userRepo
            ) : base(repository, mapper)
        {
            employeeRepository = employeeRepo;
            userRepository = userRepo;
        }

        public IQueryable<SysUserGroupModel> GetByGroup(short groupId)
        {
            var userGroups = DataContext.Get(x => x.GroupId == groupId && x.Active == true);
            var users = userRepository.Get();
            var employess = employeeRepository.Get();
            var results = userGroups.Join(users, x => x.UserId, y => y.Id, (x, y) => new { User = y, UserGroup = x })
                                    .Join(employess, x => x.User.EmployeeId, y => y.Id, (x, y) => new { User = x, Employee = y })
                                    .Select(x => new SysUserGroupModel
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

        public SysUserGroupModel GetDetail(int id)
        {
            var data = DataContext.Get(x => x.Id == id).FirstOrDefault();
            return mapper.Map<SysUserGroupModel>(data);
        }
    }
}
