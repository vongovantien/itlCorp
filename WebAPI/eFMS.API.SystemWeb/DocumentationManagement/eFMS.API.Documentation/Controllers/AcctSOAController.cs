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
    public class AcctSOAController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAcctSOAServices acctSOAServices;
        private readonly ICurrentUser currentUser;

        public AcctSOAController(IStringLocalizer<LanguageSub> localizer, IAcctSOAServices service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            acctSOAServices = service;
            currentUser = user;
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddNew(AcctSOAModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserCreated = currentUser.UserID;
            model.DatetimeCreated = DateTime.Now;
            var hs = acctSOAServices.AddNewSOA(model);
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
        public IActionResult Update(AcctSOAModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserCreated = currentUser.UserID;
            model.DatetimeCreated = DateTime.Now;
            var hs = acctSOAServices.UpdateSOA(model);
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
            var hs = acctSOAServices.DeleteSOA(cdNoteId);
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
        //[Authorize]
        public List<object> Get(Guid Id,bool IsHouseBillID)
        {
            return acctSOAServices.GroupSOAByPartner(Id, IsHouseBillID);
        }

        [HttpGet]
        [Route("GetDetails")]
        //[Authorize]
        public AcctSOADetailsModel Get(Guid JobId,string soaNo)
        {
            return acctSOAServices.GetSOADetails(JobId, soaNo);
        }

        [HttpPost]
        [Route("PreviewOpsCdNote")]
        public IActionResult PreviewOpsCdNote(AcctSOADetailsModel model)
        {
            var result = acctSOAServices.Preview(model);
            return Ok(result);
        }


    }
}