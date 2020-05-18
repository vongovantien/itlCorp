using System;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsAirWayBillController : ControllerBase
    {
        private ICsAirWayBillService airWayBillService;
        private readonly IStringLocalizer stringLocalizer;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="wayBillService"></param>
        /// <param name="localizer"></param>
        public CsAirWayBillController(ICsAirWayBillService wayBillService,
            IStringLocalizer<LanguageSub> localizer)
        {
            airWayBillService = wayBillService;
            stringLocalizer = localizer;
        }

        /// <summary>
        /// get
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetBy/{jobId}")]
        public IActionResult Get(Guid jobId)
        {
            var result = airWayBillService.GetBy(jobId);
            if (result == null) return Ok();
            return Ok(result);
        }

        /// <summary>
        /// add an air way bill
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Add(CsAirWayBillModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = airWayBillService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// update an air way bill
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public IActionResult Update(CsAirWayBillModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = airWayBillService.Update(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Export airway bill
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("AirwayBillExport")]
        public IActionResult AirwayBillExport(Guid jobId)
        {
            var result = airWayBillService.AirwayBillExport(jobId);
            return Ok(result);
        }

        /// <summary>
        /// preview house airway bill
        /// </summary>
        /// <param name="jobId">Id of Job</param>
        /// <param name="reportType">report type</param>
        /// 
        /// <returns></returns>
        [HttpGet("PreviewAirwayBill")]
        public IActionResult PreviewAirwayBill(Guid jobId,string reportType)
        {
            var result = airWayBillService.PreviewAirwayBill(jobId,reportType);
            return Ok(result);
        }
    }
}
