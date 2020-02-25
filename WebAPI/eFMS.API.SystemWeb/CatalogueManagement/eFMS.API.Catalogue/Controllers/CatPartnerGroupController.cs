using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatPartnerGroupController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPartnerGroupService catPartnerGroupService;
        public CatPartnerGroupController(IStringLocalizer<LanguageSub> localizer, ICatPartnerGroupService service)
        {
            stringLocalizer = localizer;
            catPartnerGroupService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            //return Ok(catPartnerGroupService.Get());
            return Ok(DataEnums.CatPartnerGroups);
        }
    }
}