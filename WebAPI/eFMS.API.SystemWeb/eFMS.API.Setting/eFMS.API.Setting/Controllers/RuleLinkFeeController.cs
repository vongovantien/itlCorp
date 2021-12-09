using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Setting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    //[Authorize]
    public class RuleLinkFeeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private ICurrentUser currentUser;
        private IRuleLinkFeeService ruleLinkFeeService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="curUser"></param>
        public RuleLinkFeeController(IStringLocalizer<LanguageSub> localizer,
            ICurrentUser curUser, IRuleLinkFeeService service)
        {
            stringLocalizer = localizer;
            currentUser = curUser;
            ruleLinkFeeService = service;
        }

        /// <summary>
        /// Add New Rule Link Fee
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddNewRule(CsRuleLinkFeeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            currentUser.Action = "AddNewRuleLinkFee";
            var hs = ruleLinkFeeService.AddNewRuleLinkFee(model);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// get and paging the list of Rule Link Fee by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(CsRuleLinkFeeCriteria criteria, int pageNumber, int pageSize)
        {
            var data = ruleLinkFeeService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete")]
        public IActionResult DeleteRuleLinkFee(Guid id)
        {
            currentUser.Action = "DeleteRuleLinkFee";
            var data = ruleLinkFeeService.GetRuleLinkFeeById(id);
            if (data == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND].Value });
            }
            HandleState hs = ruleLinkFeeService.DeleteRuleLinkFee(data.Id);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
                return BadRequest(_result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update Settlement Payment
        /// </summary>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]   
        [Authorize]
        public IActionResult UpdateRuleLinkFee(CsRuleLinkFeeModel model)
        {
            currentUser.Action = "UpdateCsRuleLinkFee";
            if (!ModelState.IsValid) return BadRequest();
            var hs = ruleLinkFeeService.UpdateRuleLinkFee(model);

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        [HttpGet]
        [Route("getRuleByID")]
        [Authorize]
        public IActionResult getdetailrulelinkfeebyid(Guid id)
        {
            var rule = ruleLinkFeeService.GetRuleLinkFeeById(id);
            if (rule==null)
            {
                return BadRequest();
            }
            return Ok(rule);
        }

    }
}
