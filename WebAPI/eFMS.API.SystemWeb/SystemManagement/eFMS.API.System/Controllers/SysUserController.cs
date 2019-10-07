using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Infrastructure.Common;
using ITL.NetCore.Common;
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
    public class SysUserController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysUserService sysUserService;
        private readonly IMapper mapper;
        public SysUserController(IStringLocalizer<LanguageSub> localizer, ISysUserService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            sysUserService = service;
            mapper = iMapper;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = sysUserService.GetAll();
            return Ok(results);
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public SysUserViewModel GetUserById(string id)
        {
            return sysUserService.GetUserById(id);
        }
        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SysUserCriteria criteria, int page, int size)
        {
            var data = sysUserService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }
        [HttpPost]
        public IActionResult Post(SysUserAddModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.Id);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var sysUser = mapper.Map<SysUserAddModel>(model);
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = sysUserService.AddUser(sysUser);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        private string CheckExist(string id)
        {
            string message = string.Empty;

            if (sysUserService.Any(x => (x.Id == id)))
            {
                message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
            }
            return message;
        }
    }
}