using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

namespace eFMS.API.Catalogue.Authorize
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthorizeExAttribute : AuthorizeAttribute
    {
        public AuthorizeExAttribute(params object[] permissions)
        {
            Roles = string.Join(",", permissions.Select(permission => string.Format("{0}", permission)));
        }
    }
}
