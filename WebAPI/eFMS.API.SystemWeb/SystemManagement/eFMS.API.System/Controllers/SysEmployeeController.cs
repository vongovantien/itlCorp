using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysEmployeeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysEmployeeService sysEmployeeService;
        private readonly IMapper mapper;
        public SysEmployeeController(IStringLocalizer<LanguageSub> localizer, ISysEmployeeService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            sysEmployeeService = service;
            mapper = iMapper;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = sysEmployeeService.Get();
            return Ok(results);
        }
        [HttpPost]
        [Route("Query")]
        public IActionResult Query(EmployeeCriteria criteria)
        {
            var results = sysEmployeeService.Query(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("addNew")]
        //[Authorize]
        public IActionResult AddEmployee(SysEmployeeModel sysEmployee)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(sysEmployee);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            sysEmployee.DatetimeCreated = DateTime.Now;
            sysEmployee.UserCreated = "admin"; // currentUser.UserID;
            sysEmployee.Inactive = false;
            var hs = sysEmployeeService.AddEmployee(sysEmployee);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        public IActionResult UpdateEmployee(SysEmployeeModel sysEmployee)
        {
            sysEmployee.DatetimeModified = DateTime.Now;
            sysEmployee.UserModified = "admin";
            var hs = sysEmployeeService.UpdateEmployee(sysEmployee);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }










        private string CheckExist(SysEmployeeModel model)
        {
            string message = string.Empty;        
            if (sysEmployeeService.Any(x => x.Id == model.Id))
            {
                message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
            }                  
            return message;
        }

    }
}