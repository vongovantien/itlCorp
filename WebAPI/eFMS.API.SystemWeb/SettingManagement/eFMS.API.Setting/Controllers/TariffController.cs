using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Infrastructure.Common;
using eFMS.API.Setting.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class TariffController : ControllerBase
    {
        private readonly ITariffService tariffService;
        private readonly IStringLocalizer stringLocalizer;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        public TariffController(IStringLocalizer<LanguageSub> localizer, ITariffService service)
        {
            stringLocalizer = localizer;
            tariffService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var data = tariffService.Get();
            return Ok(data);
        }

        /// <summary>
        /// Add tariff and list tariff details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        public IActionResult AddTariff(TariffModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var hs = tariffService.AddTariff(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

    }
}