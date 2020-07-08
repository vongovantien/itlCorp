using System;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
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
    public class CsShippingInstructionController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsShippingInstructionService shippingInstructionService;
        private readonly ICurrentUser currentUser;

        public CsShippingInstructionController(IStringLocalizer<LanguageSub> localizer, ICsShippingInstructionService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            shippingInstructionService = service;
            currentUser = user;
        }
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            return Ok(shippingInstructionService.GetById(id));
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult AddOrUpdate(CsShippingInstructionModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserCreated = currentUser.UserID;
            model.CreatedDate = DateTime.Now;
            var hs = shippingInstructionService.AddOrUpdate(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("PreviewSISummary")]
        public IActionResult PreviewSISummary(CsShippingInstructionReportModel model)
        {
            var result = shippingInstructionService.PreviewSISummary(model);
            return Ok(result);
        }
        [HttpPost]
        [Route("PreviewFCLShippingInstruction")]
        public IActionResult PreviewFCLShippingInstruction(CsShippingInstructionReportModel model)
        {
            var result = shippingInstructionService.PreviewFCLShippingInstruction(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("PreviewFCLContShippingInstruction/{JobId}")]
        public IActionResult PreviewFCLContShippingInstruction(Guid JobId)
        {
            var result = shippingInstructionService.PreviewFCLContShippingInstruction(JobId);
            return Ok(result);
        }


        [HttpPost]
        [Route("PreviewLCLContShippingInstruction/{JobId}")]
        public IActionResult PreviewLCLContShippingInstruction(Guid JobId)
        {
            var result = shippingInstructionService.PreviewLCLContShippingInstruction(JobId);
            return Ok(result);
        }

        [HttpPost]
        [Route("PreviewFCLOCL")]
        public IActionResult PreviewFCLOCL(CsShippingInstructionReportModel model)
        {
            var result = shippingInstructionService.PreviewOCL(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("PreviewFCLShippingInstructionByJobId")]
        [Authorize]
        public IActionResult PreviewFCLShippingInstructionByJobId(Guid jobId)
        {
            var result = shippingInstructionService.PreviewFCLShippingInstructionByJobId(jobId);
            return Ok(result);
        }

        [HttpPost]
        [Route("PreviewSISummaryByJobId/{jobId}")]
        public IActionResult PreviewSISummaryByJobId(Guid jobId)
        {
            var result = shippingInstructionService.PreviewSISummaryByJobId(jobId);
            return Ok(result);
        }
    }
}
