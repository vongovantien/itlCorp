using System;
using System.Collections.Generic;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
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

        /// <summary>
        /// Add New CD Note
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Update CD Note
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Delete CD Note
        /// </summary>
        /// <param name="cdNoteId"></param>
        /// <returns></returns>
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
        [Authorize]
        public List<object> Get(Guid Id, bool IsShipmentOperation)
        {
            return cdNoteServices.GroupCDNoteByPartner(Id, IsShipmentOperation);
        }

        [HttpGet]
        [Route("GetDetails")]
        public AcctCDNoteDetailsModel Get(Guid jobId, string cdNo)
        {
            return cdNoteServices.GetCDNoteDetails(jobId, cdNo);
        }

        /// <summary>
        /// Preview CD Note (OPS)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Preview CD Note (Sea)
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewSIFCdNote")]
        public IActionResult PreviewSIFCdNote(PreviewCdNoteCriteria criteria)
        {
            var result = cdNoteServices.PreviewSIF(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Preview CD Note (AIR)
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewAirCdNote")]
        public IActionResult PreviewAirCdNote(PreviewCdNoteCriteria criteria)
        {
            var result = cdNoteServices.PreviewAir(criteria);
            return Ok(result);
        }

        /// <summary>
        /// get invoice - cd note
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(CDNoteCriteria criteria, int page, int size)
        {
            var results = cdNoteServices.Paging(criteria, page, size, out int rowsCount);
            return Ok(results);
        }
    }
}