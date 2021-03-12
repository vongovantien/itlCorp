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

        private string userId;
        public string UserID
        {
            get
            {
                userId = !string.IsNullOrEmpty(currentUser.FirstOrDefault(x => x.Type == "id")?.Value) ? currentUser.FirstOrDefault(x => x.Type == "id")?.Value : userId;
                return userId;
            }
            set { userId = value; }
        }

        private string userName;
        public string UserName
        {
            get
            {
                userName = !string.IsNullOrEmpty(currentUser.FirstOrDefault(x => x.Type == "userName")?.Value) ? currentUser.FirstOrDefault(x => x.Type == "userName")?.Value : userName;
                return userName;
            }
            set { userName = value; }
        }

        private Guid companyId;
        public Guid CompanyID
        {
            get
            {
                companyId = currentUser.FirstOrDefault(x => x.Type == "companyId")?.Value != null ? new Guid(currentUser.FirstOrDefault(x => x.Type == "companyId").Value) : companyId;
                return companyId;
            }
            set { companyId = value; }
        }

        private Guid officeId;
        public Guid OfficeID
        {
            get
            {
                officeId = currentUser.FirstOrDefault(x => x.Type == "officeId")?.Value != null ? new Guid(currentUser.FirstOrDefault(x => x.Type == "officeId").Value) : officeId;
                return officeId;
            }
            set { officeId = value; }
        }

        private short? groupId;
        public short? GroupId
        {
            get
            {
                if (groupId == null && currentUser.FirstOrDefault(x => x.Type == "groupId") != null)
                {
                    groupId = (short)Convert.ToInt32(currentUser.FirstOrDefault(x => x.Type == "groupId")?.Value);
                }
                return groupId;
            }
            set
            {
                groupId = value;
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
                    }
                }
                return departmentId;
            }
            set
            {
                departmentId = value;
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
        private decimal? kbExc;
        public decimal? KbExchangeRate
        {
            get
            {
                string kbExcR = currentUser.FirstOrDefault(x => x.Type == "kbExchangeRate").Value;

                if (kbExcR != null)
                {
                    kbExc = Convert.ToDecimal(kbExcR);
                }
                else
                {
                    kbExc = 0;

                }
                return kbExc;
            }
            set
            {
                kbExc = value;
            }
        }
        private string _action;
        public string Action
        {
            get
            {
                return _action;
            }
            set
            {
                _action = value;
            }
        }
    }
}
