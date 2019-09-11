using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using Microsoft.AspNetCore.Mvc;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class ShipmentController : ControllerBase
    {
        readonly IShipmentService shipmentService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        public ShipmentController(IShipmentService service)
        {
            shipmentService = service;
        }

        /// <summary>
        /// get list of shipment available
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetShipmentNotLocked")]
        public IActionResult GetShipmentNotLocked()
        {
            var list = shipmentService.GetShipmentNotLocked();
            return Ok(list);
        }

        [HttpGet("GetShipmentsCreditPayer")]
        public IActionResult GetShipmentsCreditPayer()
        {
            var data = shipmentService.GetShipmentsCreditPayer();
            return Ok(data);
        }
    }
}
