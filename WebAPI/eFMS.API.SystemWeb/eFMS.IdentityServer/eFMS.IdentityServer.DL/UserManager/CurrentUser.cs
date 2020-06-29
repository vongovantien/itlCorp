using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using eFMS.IdentityServer.DL.IService;
using eFMS.API.Common.Models;

namespace eFMS.IdentityServer.DL.UserManager
{
    public class CurrentUser : ICurrentUser
    {
        readonly IUserPermissionService userPermissionService;
        readonly IHttpContextAccessor httpContext;
        readonly IEnumerable<Claim> currentUser;

        public CurrentUser(IHttpContextAccessor contextAccessor,
            IUserPermissionService userPermission)
        {
            httpContext = contextAccessor;
            currentUser = httpContext.HttpContext.User.Claims;
            userPermissionService = userPermission;
        }

        public string UserID => currentUser.FirstOrDefault(x => x.Type == "id").Value;
        public string UserName => currentUser.FirstOrDefault(x => x.Type == "userName").Value;
        public Guid CompanyID => currentUser.FirstOrDefault(x => x.Type == "companyId").Value != null ? new Guid(currentUser.FirstOrDefault(x => x.Type == "companyId").Value) : Guid.Empty;
        public Guid OfficeID => currentUser.FirstOrDefault(x => x.Type == "officeId").Value != null ? new Guid(currentUser.FirstOrDefault(x => x.Type == "officeId").Value) : Guid.Empty;
        //public int DepartmentId => currentUser.FirstOrDefault(x => x.Type == "departmentId").Value != null ? Convert.ToInt32(currentUser.FirstOrDefault(x => x.Type == "departmentId").Value) : 0;
        //public short GroupId => (short)(currentUser.FirstOrDefault(x => x.Type == "groupId").Value != null ? Convert.ToInt16(currentUser.FirstOrDefault(x => x.Type == "groupId").Value) : 0);
        private short? groupId;
        public short? GroupId
        {
            get
            {
                if(groupId == null && currentUser.FirstOrDefault(x => x.Type == "groupId") != null)
                {
                    groupId = (short)Convert.ToInt32(currentUser.FirstOrDefault(x => x.Type == "groupId").Value);
                }
                return groupId;
            }
        }
        private int? departmentId;
        public int? DepartmentId
        {
            get
            {
                if (departmentId == null && currentUser.FirstOrDefault(x => x.Type == "departmentId") != null)
                {
                    var _departmentId = currentUser.FirstOrDefault(x => x.Type == "departmentId").Value;
                    if (string.IsNullOrEmpty(_departmentId))
                    {
                        departmentId = null;
                    }
                    else
                    {
                        departmentId = Convert.ToInt32(_departmentId);
                        //if(departmentId == 0)
                        //{
                        //    departmentId = null;
                        //}
                    }
                }
                return departmentId;
            }
        }
        private List<UserPermissionModel> userPermissions;
        public List<UserPermissionModel> UserPermissions
        {
            get
            {
                if (userPermissions == null)
                {
                    return userPermissions = userPermissionService.Get(UserID, OfficeID);
                }
                return userPermissions;
            }
        }

        private UserPermissionModel userMenuPermission;

        public UserPermissionModel UserMenuPermission { get => userMenuPermission; set => userMenuPermission = value; }
    }
}
