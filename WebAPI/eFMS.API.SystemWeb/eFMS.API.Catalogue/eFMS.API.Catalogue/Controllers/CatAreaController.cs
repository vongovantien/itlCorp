using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatAreaController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatAreaService catAreaService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatAreaService</param>
        public CatAreaController(IStringLocalizer<LanguageSub> localizer, ICatAreaService service)
        {
            stringLocalizer = localizer;
            catAreaService = service;
        }

        /// <summary>
        /// get the list of areas
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(catAreaService.Get());
        }

        /// <summary>
        /// get the list of areas by culture current language
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catAreaService.GetByLanguage();
            return Ok(results);
        }
    }
}