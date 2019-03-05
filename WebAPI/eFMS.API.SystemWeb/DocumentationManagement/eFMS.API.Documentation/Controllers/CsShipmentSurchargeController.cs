using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
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
    public class CsShipmentSurchargeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsShipmentSurchargeService csShipmentSurchargeService;
        public CsShipmentSurchargeController(IStringLocalizer<LanguageSub> localizer,ICsShipmentSurchargeService service)
        {
            stringLocalizer = localizer;
            csShipmentSurchargeService = service;
        }

        [HttpGet]
        [Route("GetByHB")]
        public IActionResult GetByHouseBill(Guid hbId)
        {

            return Ok(csShipmentSurchargeService.GetByHB(hbId));
        }


        [HttpPost]
        [Route("Add")]
        public IActionResult AddNew(CsShipmentSurchargeModel model)
        {
            model.Id = Guid.NewGuid();
            if (!ModelState.IsValid) return BadRequest();
            var hs = csShipmentSurchargeService.Add(model);           
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
        public IActionResult Update(CsShipmentSurchargeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = csShipmentSurchargeService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }
    }
}