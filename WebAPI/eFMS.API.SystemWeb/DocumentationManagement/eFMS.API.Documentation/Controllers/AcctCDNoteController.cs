using System;
using System.Collections.Generic;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;


namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AcctCDNoteController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAcctCDNoteServices cdNoteServices;
        private readonly ICurrentUser currentUser;

        public AcctCDNoteController(IStringLocalizer<LanguageSub> localizer, IAcctCDNoteServices service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            cdNoteServices = service;
            currentUser = user;
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddNew(AcctCdnoteModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = cdNoteServices.AddNewCDNote(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(AcctCdnoteModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            var hs = cdNoteServices.UpdateCDNote(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid cdNoteId)
        {
            var hs = cdNoteServices.DeleteCDNote(cdNoteId);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("Get")]
        public List<object> Get(Guid Id,bool IsShipmentOperation)
        {
             return cdNoteServices.GroupCDNoteByPartner(Id, IsShipmentOperation);
        }

        [HttpGet]
        [Route("GetDetails")]
        //[Authorize]
        public AcctCDNoteDetailsModel Get(Guid jobId,string cdNo)
        {
            return cdNoteServices.GetCDNoteDetails(jobId, cdNo);
        }

        [HttpPost]
        [Route("PreviewOpsCdNote")]
        public IActionResult PreviewOpsCdNote(AcctCDNoteDetailsModel model)
        {
            var result = cdNoteServices.Preview(model);
            return Ok(result);
        }

        /// <summary>
        /// check allow delete an existed item
        /// </summary>
        /// <param name="cdNoteId"></param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{cdNoteId}")]
        public IActionResult CheckAllowDelete(Guid cdNoteId)
        {
            return Ok(cdNoteServices.CheckAllowDelete(cdNoteId));
        }

        [HttpPost]
        [Route("PreviewSIFCdNote")]
        public IActionResult PreviewSIFCdNote(PreviewCdNoteCriteria criteria)
        {
            var result = cdNoteServices.PreviewSIF(criteria);
            return Ok(result);
        }
    }
}