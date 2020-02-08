using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Linq;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.DL.IService;

namespace eFMS.IdentityServer.DL.UserManager
{
    public class CurrentUser : ICurrentUser
    {
        readonly ISysEmployeeService employeeService;
        readonly IHttpContextAccessor httpContext;
        readonly IEnumerable<Claim> currentUser;
        public CurrentUser(ISysEmployeeService empService)
        {
            employeeService = empService;
        }
        public CurrentUser(IHttpContextAccessor contextAccessor)
        {
            httpContext = contextAccessor;
            currentUser = httpContext.HttpContext.User.Claims;
        }

        public string UserID => currentUser.FirstOrDefault(x => x.Type == "id").Value;
        public string UserName => currentUser.FirstOrDefault(x => x.Type == "userName").Value;
        public Guid CompanyID => currentUser.FirstOrDefault(x => x.Type == "companyId").Value != null ? new Guid(currentUser.FirstOrDefault(x => x.Type == "companyId").Value) : Guid.Empty;
        public Guid OfficeID => currentUser.FirstOrDefault(x => x.Type == "officeId").Value != null ? new Guid(currentUser.FirstOrDefault(x => x.Type == "officeId").Value) : Guid.Empty;
        public int DepartmentId => currentUser.FirstOrDefault(x => x.Type == "departmentId").Value != null ? Convert.ToInt32(currentUser.FirstOrDefault(x => x.Type == "departmentId").Value) : 0;
        public short GroupId => (short)(currentUser.FirstOrDefault(x => x.Type == "groupId").Value != null ? Convert.ToInt16(currentUser.FirstOrDefault(x => x.Type == "groupId").Value) : 0);
    }
}
