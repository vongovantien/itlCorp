using System.Collections.Generic;
using AutoMapper;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;
using ITL.NetCore.Connection;
using System.Linq;
using SystemManagement.DL.Models.Views;
using ITL.NetCore.Common;
using System;
using SystemManagement.DL.IService;
using System.Linq.Expressions;

namespace SystemManagement.DL.Services
{
    public class SysUserRoleService : RepositoryBase<SysUserRole, SysUserRoleModel>, ISysUserRoleService
    {
        public SysUserRoleService(IContextBase<SysUserRole> repository, IMapper mapper) : base(repository, mapper)
        {
          //  SetUnique(new string[] { "UserId", "WorkPlaceId", "RoleId" });
        }


        public HandleState AddUserRole(SysUserRole RoleToAdd)
        {
            SysUserRole NewUserRole = new SysUserRole();
            NewUserRole.RoleId = RoleToAdd.RoleId;
            NewUserRole.UserId = RoleToAdd.UserId;
            NewUserRole.WorkPlaceId = RoleToAdd.WorkPlaceId;            
            NewUserRole.Inactive = false;
            NewUserRole.UserModified = "0100114";
            NewUserRole.DatetimeModified = DateTime.Now;

            return DataContext.Add(NewUserRole);
        }

        public HandleState ChangeUserRoleStatus(int id, bool status)
        {
            var UserRole = ((eTMSDataContext)DataContext.DC).SysUserRole.Where(t => t.Id == id).FirstOrDefault();
            UserRole.Inactive = status;

            return DataContext.Update(UserRole, t => t.Id == id);
        }

        //public override HandleState Add(IEnumerable<SysUserRoleModel> entities)
        //{

        //    return base.Add(entities);
        //}


    }
}
