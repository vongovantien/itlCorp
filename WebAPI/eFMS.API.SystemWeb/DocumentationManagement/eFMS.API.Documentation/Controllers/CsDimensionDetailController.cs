using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using Microsoft.AspNetCore.Mvc;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsDimensionDetailController : ControllerBase
    {
        private readonly ICsDimensionDetailService dimensionDetailService;

        public CsDimensionDetailController(ICsDimensionDetailService dimensionService)
        {
            dimensionDetailService = dimensionService;
        }

        [HttpGet("GetByMasterBill")]
        public IActionResult GetByMasterBill(Guid mblId)
        {
            var results = dimensionDetailService.Get(x => x.Mblid == mblId);
            return Ok(results);
        }


        [HttpGet("GetByHouseBill")]
        public IActionResult GetByHouseBill(Guid hblId)
        {
            var results = dimensionDetailService.Get(x => x.Hblid == hblId);
            return Ok(results);
        }

        /// <summary>
        /// get dimension from all house bills by a job
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetDIMFromHouseBillsByJob")]
        public IActionResult GetDIMFromHouseBillsByJob([Required]Guid id)
        {
            var results = dimensionDetailService.GetDIMFromHouseByJob(id);
            return Ok(results);
        }
    }
}
