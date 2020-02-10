using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    public class CustomBaseController : ControllerBase
    {
        private readonly ICurrentUser _currentUser;
        public CustomBaseController(ICurrentUser currentUser, Menu menu = Menu.catPortindex)
        {
            _currentUser = currentUser;
            if(_currentUser.UserID != null)
            {
                if(currentUser.UserPermissions.Count > 0)
                {
                    _currentUser.UserMenuPermission = currentUser.UserPermissions.Where(x => x.MenuId == menu.ToString()).FirstOrDefault();
                }
            }
        }
    }
}
