using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.API.System.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysUserLevelController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysUserLevelService userLevelService;
        private readonly ICurrentUser currentUser;
        private readonly IMapper mapper;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sysUserLevel"></param>
        /// <param name="localizer"></param>
        public SysUserLevelController(ISysUserLevelService sysUserLevel, IStringLocalizer<LanguageSub> localizer, ICurrentUser currUser, IMapper iMapper)
        {
            userLevelService = sysUserLevel;
            stringLocalizer = localizer;
            currentUser = currUser;
            mapper = iMapper;
        }

        /// <summary>
        /// get by group id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetByLevel/{id}")]
        public IActionResult GetByLevel(short id)
        {
            var results = userLevelService.GetByLevel(id);
            return Ok(results);
        }

        /// <summary>
        /// get by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = userLevelService.GetDetail(id);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Add(SysUserLevelModel model)
        {
            model.UserCreated = currentUser.UserID;
            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
            model.Active = true;
            var hs = userLevelService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Route("addUser")]
        //[Authorize]
        public IActionResult AddUser(List<SysUserLevelModel> users)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = userLevelService.AddUser(users);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public IActionResult Update(SysUserLevelModel model)
        {
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            var hs = userLevelService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var item = userLevelService.GetDetail(id);
            if(item.Active == true)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_ITEM_IS_ACTIVE_NOT_ALLOW_DELETED].Value });
            }
            item.Active = false;
            item.UserModified = currentUser.UserID;
            item.InactiveOn = DateTime.Now;
            var hs = userLevelService.Update(item, x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
