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
    /// <summary>
    /// 
    /// </summary>
    public class CustomAuthcontroller : ControllerBase
    {
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="curUser"></param>
        /// <param name="menu"></param>
        public CustomAuthcontroller(ICurrentUser curUser, Menu menu = Menu.acctAP)
        {
            currentUser = curUser;
            if (curUser.UserPermissions.Count > 0)
            {
                curUser.UserMenuPermission = curUser.UserPermissions.FirstOrDefault(x => x.MenuId == menu.ToString());
            }
        }
    }
}
