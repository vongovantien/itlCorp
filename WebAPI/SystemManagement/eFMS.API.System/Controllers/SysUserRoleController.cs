using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemManagement.DL.Infrastructure.ErrorHandler;
using SystemManagement.DL.IService;
using SystemManagement.DL.Models;
using SystemManagement.DL.Models.Views;
using SystemManagement.DL.Services;
using SystemManagementAPI.Service.Models;

namespace SystemManagementAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SysUserRoleController : ControllerBase
    {
        private readonly ISysUserRoleService _service;
        private readonly IErrorHandler _errorHandler;
        public SysUserRoleController(ISysUserRoleService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }

        [HttpDelete]
        [Route("DeleteUserRole/{id}")]
        public HandleState DeleteUserRole(int id)
        {
            return _service.Delete(t => t.Id == id);
        }
        
        [HttpPut]
        [Route("ChangeUserRoleStatus/{id}")]
        public HandleState UpdateUserRole([FromRoute]int id,bool status)
        {
            return _service.ChangeUserRoleStatus(id, status);
        }

        [HttpPost]
        [Route("AddUserRole")]
        public HandleState AddUserRole(SysUserRoleModel RoleToAdd)
        {
            RoleToAdd.UserModified = "0100114";
            RoleToAdd.DatetimeModified = DateTime.Now;
            // return _service.AddUserRole(RoleToAdd);
            return _service.Add(RoleToAdd);
        }

        

       



    }
}