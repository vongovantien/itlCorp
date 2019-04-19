using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatAreaController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatAreaService catAreaService;
        public CatAreaController(IStringLocalizer<LanguageSub> localizer, ICatAreaService service)
        {
            stringLocalizer = localizer;
            catAreaService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(catAreaService.Get());
        }

        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catAreaService.GetByLanguage();
            return Ok(results);
        }
    }
}