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
using System.Linq.Expressions;
using System.Data.SqlClient;

namespace SystemManagement.DL.Services
{
    public class SysUserService : RepositoryBase<SysUser, SysUserModel>, ISysUserService
    {
        public SysUserService(IContextBase<SysUser> repository, IMapper mapper) : base(repository, mapper)
       {
            SetChildren<SysAuthorization>("Id", "UserId");
            SetChildren<SysUserOtherWorkPlace>("Id", "UserId");
            SetChildren<SysUserRole>("Id", "UserId");
            SetChildren<SysUserLog>("Id", "UserId");
        }

        protected override IContextBase<SysUser> DataContext => base.DataContext;

        public object GetNecessaryData()
        {
            var listEmployees = ((eTMSDataContext)DataContext.DC).GetViewData<vw_sysEmployee>();
            var listUserGroup = ((eTMSDataContext)DataContext.DC).SysUserGroup;
            var listWorkPlace = ((eTMSDataContext)DataContext.DC).GetViewData<vw_WorkPlace>();
            var listRole = ((eTMSDataContext)DataContext.DC).SysRole.ToList();
            var listUser = ((eTMSDataContext)DataContext.DC).GetViewData<vw_sysUser>();
     
            List<object> _listEmployee = new List<object>();
            List<object> _listUserGroup = new List<object>();
            List<object> _listWorkPlace = new List<object>();
            List<object> _listRole = new List<object>();
            List<object> _listUser = new List<object>();
            foreach (var em in listEmployees)
            {
                var _em = new { em.ID, em.EmployeeName_VN, em.EmployeeName_EN };
                _listEmployee.Add(_em);
            }

            foreach (var ug in listUserGroup)
            {
                var _ug = new { ug.Id, ug.Code, ug.Name };
                _listUserGroup.Add(_ug);
            }

            foreach (var wp in listWorkPlace)
            {
                var _wp = new { wp.ID, wp.Code, wp.Name_EN, wp.Name_VN, wp.PlaceTypeID, wp.PlaceTypeName_EN, wp.PlaceTypeName_VN};
                _listWorkPlace.Add(_wp);
            }

            foreach(var role in listRole)
            {
                var _role = new { role.Id, role.Code, role.Name, role.Description };
                _listRole.Add(_role);

            }

            foreach(var user in listUser)
            {
                var _user = new { user.ID, user.EmployeeName_VN, user.EmployeeName_EN };
                _listUser.Add(_user);
            }




            return new { _listEmployee, _listUserGroup, _listWorkPlace,_listRole , _listUser };

        }

        public List<vw_SysUserWithRoles> GetViewUsers()
        {
            var UserRoles = ((eTMSDataContext)DataContext.DC).GetViewData<vw_SysUserRole>();
            var Users = ((eTMSDataContext)DataContext.DC).GetViewData<vw_sysUser>();
            List<vw_SysUserWithRoles> returnList = new List<vw_SysUserWithRoles>();
            foreach (var user in Users)
            {
                var roles = UserRoles.Where(role => role.UserID == user.ID).ToList();
                returnList.Add(new vw_SysUserWithRoles { sysUser = user, listRole = roles });
            }

            return returnList;
        }

        public object GetUserDetails(string id)
        {
            var UserRoles = ((eTMSDataContext)DataContext.DC).GetViewData<vw_SysUserRole>().Where(t => t.UserID == id);
            var User = ((eTMSDataContext)DataContext.DC).GetViewData<vw_sysUser>().Where(t => t.ID == id).FirstOrDefault();
            return new  {User,UserRoles};
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public HandleState Update(vw_sysUser user)
        {
            var SysUser = ((eTMSDataContext)DataContext.DC).SysUser.Where(t => t.Id == user.ID).SingleOrDefault();
            SysUser.Username = user.Username;
            SysUser.Password = user.Password!=SysUser.Password?Utility.GetSHA512(user.Password):SysUser.Password;
            SysUser.EmployeeId = user.EmployeeID;           
            SysUser.UserGroupId = user.UserGroupID;
            SysUser.WorkPlaceId = user.WorkPlaceID;
            SysUser.DatetimeModified = DateTime.Now;
            SysUser.Inactive = user.Inactive;
            SysUser.UserModified = "";

            return DataContext.Update(SysUser, t => t.Id == user.ID);
           
        }

        public HandleState ResetPassword(string id)
        {
            var SysUser = ((eTMSDataContext)DataContext.DC).SysUser.Where(t => t.Id == id).SingleOrDefault();
            string Pass = string.Format("{0}{1}", SysUser.Username, "123");
            Pass = Utility.GetSHA512(Pass);
            SysUser.Password = Pass;
            return DataContext.Update(SysUser, t => t.Id == id);
        }

        public string GenerateId(Guid WorkPlaceId)
        {
            string funcName = "dbo.fn_GenerateUserID";
            SqlParameter pa = new SqlParameter("@BranchID", WorkPlaceId);
            var result = ((eTMSDataContext)DataContext.DC).ExecuteFuncScalar(funcName, pa);
            return result.ToString();
        }

        public HandleState AddNewUser(SysUserNoRelaModel SysUser)
        {            
            SysUser _sysUser = new SysUser
            {
                Id = GenerateId(SysUser.WorkPlaceId),
                Username = SysUser.Username,
                Password = Utility.GetSHA512(SysUser.Password),
                WorkPlaceId = SysUser.WorkPlaceId,
                EmployeeId = SysUser.EmployeeId,
                UserGroupId = SysUser.UserGroupId,
                UserCreated = "0100114",
                DatetimeCreated = DateTime.Now,
                Inactive = false
            };

            return DataContext.Add(_sysUser);

        }

    }
}
