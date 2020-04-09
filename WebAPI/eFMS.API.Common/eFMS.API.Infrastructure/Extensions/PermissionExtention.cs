using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.IdentityServer.DL.UserManager;
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
            int code = 403;
            switch (permissionRange)
            {
                case PermissionRange.All:
                    code = 200;
                    break;
                case PermissionRange.Owner:
                    if (model.UserCreated == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId == currentUser.GroupId 
                        && model.DepartmentId == currentUser.DepartmentId 
                        && model.OfficeId == currentUser.OfficeID 
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId == currentUser.DepartmentId 
                        && model.OfficeId == currentUser.OfficeID 
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId == currentUser.OfficeID 
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;                
            }
            return code;
        }

        public static bool GetPermissionDetail(PermissionRange permissionRange, BaseUpdateModel model, ICurrentUser currentUser)
        {
            bool result = false;

            switch (permissionRange)
            {
                case PermissionRange.All:
                    result = true;
                    break;
                case PermissionRange.Owner:
                    if (model.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId == currentUser.GroupId
                        && model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID
                        )
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }
    }
}
