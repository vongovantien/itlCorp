using System;
using System.Linq.Expressions;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Service.Models;
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
    public class UserGroupController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IUserGroupService userGroupService;
        public UserGroupController(IStringLocalizer<LanguageSub> localizer, IUserGroupService service)
        {
            stringLocalizer = localizer;
            userGroupService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = userGroupService.Get();
            return Ok(results);
        }

        [HttpGet]
        [Route("Query")]
        public IActionResult Get(UserGroupCriteria criteria, string orderByProperty, bool isAscendingOrder)
        {
            var results = userGroupService.Query(criteria, orderByProperty, isAscendingOrder);
            return Ok(results);
        }

        [HttpGet("{Id}")]
        public IActionResult Get(short Id)
        {
            var result = userGroupService.First(x => x.Id == Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(UserGroupCriteria criteria, int page, int size, string orderByProperty, bool isAscendingOrder)
        {
            var data = userGroupService.Paging(criteria, page, size, orderByProperty, isAscendingOrder, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }


        [HttpPost]
        [Route("Add")]
        public IActionResult Post(SysUserGroupModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var messageExisted = CheckExisted(model);
            if (messageExisted.Length > 0) return BadRequest(stringLocalizer[messageExisted]);
            var result = userGroupService.Add(model);
            var message = HandleError.GetMessage(result, Crud.Insert);
            if (!result.Success)
            {
                return BadRequest(stringLocalizer[message]);
            }
            return Ok(stringLocalizer[message]);
        }

        [HttpPut("{Id}")]
        public IActionResult Put(SysUserGroupModel  model)
        {
            if(!ModelState.IsValid) return BadRequest();
            var messageExisted = CheckExisted(model);
            if (messageExisted.Length > 0) return BadRequest(stringLocalizer[messageExisted]);
            var result = userGroupService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(result, Crud.Update);
            if (!result.Success)
            {
                return BadRequest(stringLocalizer[message]);
            }
            return Ok(stringLocalizer[message]);
        }

        [HttpDelete("{Id}")]
        public IActionResult Delete(short Id)
        {
            var result = userGroupService.Delete(x => x.Id == Id);
            var message = HandleError.GetMessage(result, Crud.Delete);
            if (!result.Success)
            {
                return BadRequest(stringLocalizer[message]);
            }
            return Ok(stringLocalizer[message]);
        }

        private string CheckExisted(SysUserGroupModel model)
        {
            if (model.Id == 0)
            {
                if (userGroupService.Any(x => x.Code == model.Code))
                {
                    return LanguageSub.MSG_CODE_EXISTED;
                }
                if (userGroupService.Any(x => x.Name == model.Name))
                {
                    return LanguageSub.MSG_NAME_EXISTED;
                }
            }
            else
            {
                if (userGroupService.Any(x => x.Code == model.Code && x.Id != model.Id))
                {
                    return LanguageSub.MSG_CODE_EXISTED;
                }
                if (userGroupService.Any(x => x.Name == model.Name && x.Id != model.Id))
                {
                    return LanguageSub.MSG_NAME_EXISTED;
                }
            }
            return string.Empty;
        }
    }
}