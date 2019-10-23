using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models.Criteria;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.API.System.DL.Models;
using Microsoft.AspNetCore.Authorization;
using eFMS.API.Common;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common.Globals;
using eFMS.API.System.Infrastructure.Common;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysUserController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysUserService sysUserService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        private readonly ISysEmployeeService sysEmployeeService;
        public SysUserController(IStringLocalizer<LanguageSub> localizer, ISysUserService service, IMapper iMapper, ICurrentUser currUser, ISysEmployeeService isysEmployeeService)
        {
            stringLocalizer = localizer;
            sysUserService = service;
            mapper = iMapper;
            currentUser = currUser;
            sysEmployeeService = isysEmployeeService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = sysUserService.GetAll();
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SysUserCriteria criteria, int page, int size)
        {
            var data = sysUserService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// add new group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Add(SysUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.Password = "12345678";
            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var existedMessage = CheckExistCode(model.SysEmployeeModel.StaffCode, "0");
            var existedName = CheckExistUserName(model.Username, "0");

            if (existedMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            if (existedName.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedName });
            }

            model.SysEmployeeModel.Id = Guid.NewGuid().ToString();
            var hsEmloyee = sysEmployeeService.Add(model.SysEmployeeModel);
            var messageEmployee = HandleError.GetMessage(hsEmloyee, Crud.Insert);

            ResultHandle resultEmployee = new ResultHandle { Status = hsEmloyee.Success, Message = stringLocalizer[messageEmployee].Value };
            if (hsEmloyee.Success)
            {
                model.EmployeeId = model.SysEmployeeModel.Id;
                model.UserCreated = currentUser.UserID;
                model.Id = Guid.NewGuid().ToString();
                model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                var hs = sysUserService.Add(model);
                var message = HandleError.GetMessage(hs, Crud.Insert);

                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                if (!hs.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            else
            {
                return BadRequest(resultEmployee);
            }
        }

        /// <summary>
        /// update an existed group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public IActionResult Update(SysUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var existedMessage = CheckExistCode(model.SysEmployeeModel.StaffCode, model.Id);
            var existedName = CheckExistUserName(model.Username, model.Id);
            if (existedMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            if (existedName.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedName });
            }
           
            var employeeCurrent = sysEmployeeService.Get(x => x.Id == model.EmployeeId).FirstOrDefault();
            model.SysEmployeeModel.Id = employeeCurrent.Id;
            model.SysEmployeeModel.UserModified = currentUser.UserID;
            model.SysEmployeeModel.DatetimeModified = DateTime.Now;
            var hsEmployee = sysEmployeeService.Update(model.SysEmployeeModel, x => x.Id == model.Id);
            var messageEmployee = HandleError.GetMessage(hsEmployee, Crud.Update);
            ResultHandle resultEmployee = new ResultHandle { Status = hsEmployee.Success, Message = stringLocalizer[messageEmployee].Value };
            if (hsEmployee.Success)
            {
                model.UserModified = currentUser.UserID;
                model.DatetimeModified = DateTime.Now;
                var userCurrent = sysUserService.Get(x => x.Id == model.Id).FirstOrDefault();
                model.Password = userCurrent.Password;
                var hs = sysUserService.Update(model, x => x.Id == model.Id);
                var message = HandleError.GetMessage(hs, Crud.Update);

                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                if (!hs.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            else
            {
                return BadRequest(resultEmployee);
            }
     
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(string id)
        {
            var item = sysUserService.Get(x => x.Id == id).FirstOrDefault();
            //if (item.Active == true)
            //{
            //    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_ITEM_IS_ACTIVE_NOT_ALLOW_DELETED].Value });
            //}
            item.Active = false;
            var hs = sysUserService.Update(item,x=>x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExistUserName(string username, string id)
        {
            string message = string.Empty;
            if (!string.IsNullOrEmpty(username))
            {
                if (id == "0")
                {
                    if (sysUserService.Any(x => x.Username.ToLower().Trim() == username.ToLower().Trim()))
                    {
                        message = stringLocalizer[LanguageSub.MSG_NAME_EXISTED].Value;
                    }
                }
                else
                {
                    if (sysUserService.Any(x => x.Username.ToLower().Trim() == username.ToLower().Trim() && x.Id != id))
                    {
                        message = stringLocalizer[LanguageSub.MSG_NAME_EXISTED].Value;
                    }
                }
            }
            return message;
        }


        private string CheckExistCode(string code, string id)
        {
            string message = string.Empty;
            if (!string.IsNullOrEmpty(code))
            {
                if (id == "0")
                {
                    if (sysEmployeeService.Any(x => x.StaffCode.ToLower().Trim() == code.ToLower().Trim()))
                    {
                        message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                    }
                }
                else
                {
                    if (sysEmployeeService.Any(x => x.StaffCode.ToLower().Trim() == code.ToLower().Trim() && x.Id != id))
                    {
                        message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                    }
                }
            }

            return message;
        }


    }
}
