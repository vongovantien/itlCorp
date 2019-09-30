using System.Collections.Generic;
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

        /// <summary>
        /// get list shipment credit payer
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="productServices"></param>
        /// <returns></returns>
        [HttpGet("GetShipmentsCreditPayer")]
        public IActionResult GetShipmentsCreditPayer(string partner, List<string> productServices)
        {
            var data = shipmentService.GetShipmentsCreditPayer(partner, productServices);
            return Ok(data);
        }

        /// <summary>
        /// Get list shipment copy by search option and keywords
        /// </summary>
        /// <param name="searchOption"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        [HttpGet("GetShipmentsCopyListBySearchOption")]
        public IActionResult GetShipmentsCopyListBySearchOption(string searchOption, List<string> keywords)
        {
            var data = shipmentService.GetListShipmentBySearchOptions(searchOption, keywords);
            return Ok(data);
        }
    }
}
