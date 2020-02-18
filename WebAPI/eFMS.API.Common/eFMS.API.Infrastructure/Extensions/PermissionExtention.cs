using eFMS.API.Common.Globals;
using eFMS.API.Infrastructure.Models;
using eFMS.IdentityServer.DL.UserManager;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Infrastructure.Extensions
{
    public static class PermissionExtention
    {
        public static ICurrentUser GetUserMenuPermission(ICurrentUser curUser, Menu menu = Menu.acctAP)
        {

            if (curUser.UserPermissions.Count > 0)
            {
                curUser.UserMenuPermission = curUser.UserPermissions.FirstOrDefault(x => x.MenuId == menu.ToString());
            }
            return curUser;
        }
        public static PermissionRange GetPermissionRange(string permissionRange)
        {
            PermissionRange result = PermissionRange.All;
            switch (permissionRange)
            {
                case Constants.PERMISSION_RANGE_NONE:
                    result = PermissionRange.None;
                    break;
                case Constants.PERMISSION_RANGE_OWNER:
                    result = PermissionRange.Owner;
                    break;
                case Constants.PERMISSION_RANGE_GROUP:
                    result = PermissionRange.Group;
                    break;
                case Constants.PERMISSION_RANGE_DEPARTMENT:
                    result = PermissionRange.Department;
                    break;
                case Constants.PERMISSION_RANGE_OFFICE:
                    result = PermissionRange.Office;
                    break;
                case Constants.PERMISSION_RANGE_COMPANY:
                    result = PermissionRange.Company;
                    break;
            }
            return result;
        }
        public static int GetPermissionCommonItem(BaseUpdateModel model, PermissionRange permissionRange, ICurrentUser currentUser)
        {
            int code = 200;
            switch (permissionRange)
            {
                case PermissionRange.Owner:
                    if (model.UserCreated != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId != currentUser.GroupId && model.DepartmentId != currentUser.DepartmentId && model.OfficeId != currentUser.OfficeID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId != currentUser.DepartmentId && model.OfficeId != currentUser.OfficeID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId != currentUser.OfficeID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId != currentUser.CompanyID)
                    {
                        code = 403;
                    }
                    break;
            }
            return code;
        }
    }
}
