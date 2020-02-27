using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatPartnerChargeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPartnerChargeService catPartnerChargeService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        public CatPartnerChargeController(IStringLocalizer<LanguageSub> localizer, ICatPartnerChargeService service)
        {
            stringLocalizer = localizer;
            catPartnerChargeService = service;
        }

        /// <summary>
        /// get charges by partnerId
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [HttpGet("{partnerId}")]
        public IActionResult Get(string partnerId)
        {
            var results = catPartnerChargeService.Get(x => x.PartnerId == partnerId);
            return Ok(results);
        }

        [HttpPut("AddAndUpdate")]
        [Authorize]
        public IActionResult AddAndUpdate(string partnerId, List<DL.Models.CatPartnerChargeModel> charges)
        {
            ResultHandle result = new ResultHandle();
            var group = charges.GroupBy(x => new { x.ChargeId, x.PartnerId });
            if (group.Any(x => x.Count() > 1))
            {
                result.Status = false;
                result.Message = "Duplicate charge";
                return BadRequest(result);
            }
            HandleState hs = catPartnerChargeService.AddAndUpdate(partnerId, charges);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
