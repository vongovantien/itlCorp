using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using System.Linq;

namespace eFMS.API.Documentation.DL.Common
{
    public static class PermissionEx
    {
        public static ICurrentUser GetUserMenuPermission(ICurrentUser curUser, Menu menu = Menu.acctAP)
        {

            if (curUser.UserPermissions.Count > 0)
            {
                curUser.UserMenuPermission = curUser.UserPermissions.Where(x => x.MenuId == menu.ToString()).FirstOrDefault();
            }
            return curUser;
        }

        public static PermissionRange GetPermissionRange(string permissionRange)
        {
            PermissionRange result = PermissionRange.All;
            switch (permissionRange)
            {
                case Constants.PERMISSION_RANGE_ALL:
                    result = PermissionRange.All;
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
    }
}
