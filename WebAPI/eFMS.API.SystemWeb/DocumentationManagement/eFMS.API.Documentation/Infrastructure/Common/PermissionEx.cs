using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.Infrastructure.Common
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
    }
}
