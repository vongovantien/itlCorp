using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class TerminologyController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ITerminologyService terminologyService;
        public TerminologyController(IStringLocalizer<LanguageSub> localizer, ITerminologyService service)
        {
            stringLocalizer = localizer;
            terminologyService = service;
        }

        [HttpGet("GetFreightTerms")]
        public IActionResult GetFreightTerms()
        {
            return Ok(terminologyService.GetFreightTerms());
        }

        [HttpGet("GetShipmentTypes")]
        public IActionResult GetShipmentTypes()
        {
            return Ok(terminologyService.GetShipmentTypes());
        }

        [HttpGet("GetBillofLoadingTypes")]
        public IActionResult GetBillofLoadingTypes()
        {
            return Ok(terminologyService.GetBillofLoadingTypes());
        }

        [HttpGet("ServiceTypes")]
        public IActionResult GetServiceTypes()
        {
            return Ok(terminologyService.GetServiceTypes());
        }

        [HttpGet("GetTypeOfMoves")]
        public IActionResult GetTypeOfMoves()
        {
            return Ok(terminologyService.GetTypeOfMoves());
        }
    }
}
