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
            var orderBy = CreateExpression<SysUserGroup, object>(orderByProperty);
            var results = userGroupService.Paging(criteria, page, size, orderBy, isAscendingOrder, out int rowCount);
            return Ok(results);
        }

        static Expression<Func<TModel, TProperty>> CreateExpression<TModel, TProperty>(
        string propertyName)
        {
            var param = Expression.Parameter(typeof(TModel), "x");
            return Expression.Lambda<Func<TModel, TProperty>>(
                Expression.PropertyOrField(param, propertyName), param);
        }


        [HttpPost]
        public IActionResult Post(SysUserGroupModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = userGroupService.Add(model);
            var message = HandleError.GetMessage(result, Crud.Insert);
            if (!result.Success)
            {
                return BadRequest(stringLocalizer[message]);
            }
            return Ok(stringLocalizer[message]);
        }

        [HttpPut("{Id}")]
        public IActionResult Put(short Id, SysUserGroupModel  model)
        {
            var result = userGroupService.Update(model, x => x.Id == Id);
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

    }
}