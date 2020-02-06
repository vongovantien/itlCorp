﻿using Microsoft.AspNetCore.Http;
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
        private IHttpContextAccessor httpContext;
        private readonly IEnumerable<Claim> currentUser;
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
        public string EmployeeID => currentUser.FirstOrDefault(x => x.Type == "employeeId").Value;
        public string UserName => currentUser.FirstOrDefault(x => x.Type == "userName").Value;

        public EmployeeModel CurrentEmployee => employeeService.First(x => x.Id == EmployeeID);

        public Guid CompanyID => new Guid(currentUser.FirstOrDefault(x => x.Type == "companyId").Value);
        public Guid OfficeID => new Guid(currentUser.FirstOrDefault(x => x.Type == "officeId").Value);
        public int DepartmentId => Convert.ToInt32(currentUser.FirstOrDefault(x => x.Type == "departmentId").Value);
        public short GroupId => Convert.ToInt16(currentUser.FirstOrDefault(x => x.Type == "groupId").Value);
    }
}
