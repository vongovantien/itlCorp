using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    public class CustomAuthcontroller : ControllerBase
    {
        private ICurrentUser currentUser;
        public CustomAuthcontroller(ICurrentUser curUser, Menu menu = Menu.acctAP)
        {
            currentUser = curUser;
            if (curUser.UserPermissions.Count > 0)
            {
                curUser.UserMenuPermission = curUser.UserPermissions.Where(x => x.MenuId == menu.ToString()).FirstOrDefault();
            }
        }
    }
}
