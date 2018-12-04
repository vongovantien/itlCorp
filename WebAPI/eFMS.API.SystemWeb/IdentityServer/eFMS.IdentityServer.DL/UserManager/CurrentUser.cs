using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace eFMS.IdentityServer.DL.UserManager
{
    public class CurrentUser : ICurrentUser
    {
        private IHttpContextAccessor httpContext;
        private IEnumerable<Claim> currentUser;
        public CurrentUser(IHttpContextAccessor contextAccessor)
        {
            httpContext = contextAccessor;
            currentUser = httpContext.HttpContext.User.Claims;
        }
        public string UserID => currentUser.FirstOrDefault(x => x.Type == "id").Value;

        //public string EmployeeID => throw new NotImplementedException();
    }
}
