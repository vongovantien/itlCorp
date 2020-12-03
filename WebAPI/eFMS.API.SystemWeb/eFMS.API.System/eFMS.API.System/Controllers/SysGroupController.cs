using System;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysGroupController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysGroupService sysGroupService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="groupService"></param>
        /// <param name="imapper"></param>
        /// <param name="currUser"></param>
        public SysGroupController(IStringLocalizer<LanguageSub> localizer,
            ISysGroupService groupService,
            IMapper imapper,
            ICurrentUser currUser) {
            stringLocalizer = localizer;
            sysGroupService = groupService;
            mapper = imapper;
            currentUser = currUser;
        }

        /// <summary>
        /// get all groups
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = sysGroupService.Get();
            return Ok(results);
        }

        /// <summary>
        /// get list of groups by criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("Query")]
        public IActionResult Query(SysGroupCriteria criteria)
        {
            var results = sysGroupService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// paging and query list of groups by criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpPost("Paging")]
        public IActionResult Paging(SysGroupCriteria criteria, int page, int size)
        {
            var data = sysGroupService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get detail group by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(short id)
        {
            var result = sysGroupService.GetById(id);
            return Ok(result);
        }

        /// <summary>
        /// add new group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Add(SysGroupModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var existedMessage = CheckExistCode(model.Code, 0);
            if (existedMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }

            var hs = sysGroupService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// update an existed group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public IActionResult Update(SysGroupModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var existedMessage = CheckExistCode(model.Code, model.Id);
            if (existedMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            var hs = sysGroupService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(short id)
        {
            var item = sysGroupService.GetById(id);
            if (item.Active == true)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_IS_ACTIVE_NOT_ALLOW_DELETED].Value });
            }

            var hs = sysGroupService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetByDepartment/{id}")]
        public IActionResult GetGroupBy(int id)
        {
            var offices = sysGroupService.GetGroupByDepartment(id);
            return Ok(offices);
        }

        [HttpGet]
        [Route("GetDepartmentGroupPermission/{userId}/{officeId}")]
        public IActionResult GetDepartmentGroupPermission(string userId, Guid officeId)
        {
            var departmentGroups = sysGroupService.GetGroupDepartmentPermission(userId, officeId);
            return Ok(departmentGroups);
        }

        private string CheckExistCode(string code, short id)
        {
            string message = string.Empty;
            if(id == 0)
            {
                if (sysGroupService.Any(x => x.Code.ToLower().Trim() == code.ToLower().Trim()))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (sysGroupService.Any(x => x.Code.ToLower().Trim() == code.ToLower().Trim() && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }
    }
}
