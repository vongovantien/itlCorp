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
using SystemManagement.DL.Models;
using SystemManagement.DL.Models.Views;
using SystemManagement.DL.Services;
using SystemManagementAPI.Service.Models;

namespace SystemManagementAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SysUsersController : ControllerBase
    {
        private readonly ISysUserService _service;
        private readonly IErrorHandler _errorHandler;
        public SysUsersController(ISysUserService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
            
        }

        [HttpGet]
        [Route("GetViewUsers")]
        public List<vw_SysUserWithRoles> GetViewUsers()
        {
            return _service.GetViewUsers();
        }

        [HttpGet]
        [Route("GetUserDetails/{id}")]
        public object GetUserDetails(string id)
        {
            return _service.GetUserDetails(id);
        }

        [HttpGet]
        [Route("GetNecessaryData")]
        public object GetNecessaryData()
        {
            return _service.GetNecessaryData();
        }

        [HttpDelete]
        [Route("DeleteUser/{id}")]
        public HandleState Delete(string id)
        {

            return _service.Delete(x => x.Id == id);
        }

        [HttpPut]
        [Route("UpdateUser")]
        public HandleState UpdateUserInfo(vw_sysUser user)
        {
            return _service.Update(user);
        }

        [HttpPut]
        [Route("ResetPassword/{id}")]
        public HandleState ResetPassword(string id)
        {
            return _service.ResetPassword(id);
        }

        [HttpPost]
        [Route("AddNewUser")]
        public HandleState AddNewUser(SysUserNoRelaModel sysUser)
        {
            return _service.AddNewUser(sysUser);
        }



    }
}