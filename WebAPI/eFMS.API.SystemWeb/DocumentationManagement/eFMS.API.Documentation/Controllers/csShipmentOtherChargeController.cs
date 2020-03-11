using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class csShipmentOtherChargeController : ControllerBase
    {
        private readonly ICsShipmentOtherChargeService otherChargeService;

        public csShipmentOtherChargeController(ICsShipmentOtherChargeService ochargeService)
        {
            otherChargeService = ochargeService;
        }

        [HttpGet("GetByMasterBill")]
        public IActionResult GetByMasterBill(Guid jobId)
        {
            var results = otherChargeService.Get(x => x.JobId == jobId);
            return Ok(results);
        }

        [HttpGet("GetByHouseBill")]
        public IActionResult GetByHouseBill(Guid hblId)
        {
            var results = otherChargeService.Get(x => x.Hblid == hblId);
            return Ok(results);
        }
    }
}