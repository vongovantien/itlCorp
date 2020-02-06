using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// Controller Authorization
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysAuthorizationController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysAuthorizationService sysAuthorizationService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="imapper"></param>
        /// <param name="currUser"></param>
        public SysAuthorizationController(IStringLocalizer<LanguageSub> localizer, ISysAuthorizationService service, IMapper imapper, ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            sysAuthorizationService = service;
            mapper = imapper;
            currentUser = currUser;
        }

        /// <summary>
        /// Query Data
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("QueryData")]
        public IActionResult QueryData(SysAuthorizationCriteria criteria)
        {
            var data = sysAuthorizationService.QueryData(criteria);
            return Ok(data);
        }

        /// <summary>
        /// Paging
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpPost("Paging")]
        public IActionResult Paging(SysAuthorizationCriteria criteria, int page, int size)
        {
            var data = sysAuthorizationService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// Get Authorization by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetAuthorizationById(int id)
        {
            var result = sysAuthorizationService.GetAuthorizationById(id);
            return Ok(result);
        }

        /// <summary>
        /// Insert Authorization
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Insert(SysAuthorizationModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var hs = sysAuthorizationService.Insert(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update Authorization
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(SysAuthorizationModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            if (model.Active == false)
            {
                model.InactiveOn = DateTime.Now;
            }
            var hs = sysAuthorizationService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Delete Authorization
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var item = sysAuthorizationService.GetAuthorizationById(id);
            if (item.Active == true)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_ITEM_IS_ACTIVE_NOT_ALLOW_DELETED].Value });
            }

            var hs = sysAuthorizationService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}