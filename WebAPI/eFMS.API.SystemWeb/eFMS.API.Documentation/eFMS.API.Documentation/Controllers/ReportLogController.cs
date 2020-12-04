using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class ReportLogController : ControllerBase
    {
        readonly IReportLogService reportLogService;
        private readonly IStringLocalizer stringLocalizer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="localizer"></param>
        public ReportLogController(IReportLogService service, IStringLocalizer<LanguageSub> localizer)
        {
            reportLogService = service;
            stringLocalizer = localizer;
        }

        /// <summary>
        /// Add Report Log
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddNew")]
        [Authorize]
        public IActionResult AddNew(SysReportLogModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = reportLogService.AddNew(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString(), Data = model };
                return BadRequest(_result);
            }
            return Ok(result);
        }
    }
}