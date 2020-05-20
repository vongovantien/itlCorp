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
using System;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{

    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsBookingNoteController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsBookingNoteService csBookingNoteService;
        private readonly ICurrentUser currentUser;

        public CsBookingNoteController(IStringLocalizer<LanguageSub> localizer, ICsBookingNoteService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            csBookingNoteService = service;
            currentUser = user;
        }

        [HttpPost("Query")]
        [Authorize]
        public IActionResult QueryData(CsBookingNoteCriteria criteria)
        {
            var data = csBookingNoteService.Query(criteria);
            return Ok(data);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(CsBookingNoteCriteria criteria, int page, int size)
        {
            var data = csBookingNoteService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CsBookingNoteEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.BookingNo,Guid.Empty);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = csBookingNoteService.AddCsBookingNote(model);
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
        public IActionResult Update(CsBookingNoteEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.BookingNo,model.Id);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = csBookingNoteService.UpdateCsBookingNote(model);
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
        public IActionResult Delete(Guid id)
        {
            var hs = csBookingNoteService.DeleteCsBookingNote(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(string bookingNo,Guid id)
        {
            string message = string.Empty;
            if(id != Guid.Empty)
            {
                if(csBookingNoteService.Any(x => x.BookingNo == bookingNo && x.Id != id))
                {
                    message = "Booking No is existed !";
                }
            }
            else
            {
                if(csBookingNoteService.Any(x=>x.BookingNo == bookingNo))
                {
                    message = "Booking No is existed !";
                }
            }
            return message;
        }

    }
}