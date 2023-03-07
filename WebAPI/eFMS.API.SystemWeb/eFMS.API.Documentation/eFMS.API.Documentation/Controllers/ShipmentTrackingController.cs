using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.Criteria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
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
    public class ShipmentTrackingController : ControllerBase
    {

        private readonly IShipmentTrackingService shipmentTrackingService;
        private readonly IStringLocalizer stringLocalizer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        public ShipmentTrackingController(IShipmentTrackingService service, IStringLocalizer<LanguageSub> localizer)
        {
            shipmentTrackingService = service;
            stringLocalizer = localizer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [Authorize]
        [HttpGet("TrackShipmentProgress")]
        public async Task<IActionResult> TrackShipmentProgress([FromQuery] TrackingShipmentCriteria model)
        {
            if (!shipmentTrackingService.CheckExistShipment(model))
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[DocumentationLanguageSub.MSG_SHIPMENT_NOT_EXIST].Value });
            }

            var data = await shipmentTrackingService.TrackShipmentProgress(model);

            return Ok(data);
        }
    }
}
