using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatChargeGroupController : ControllerBase
    {
        private readonly ICatChargeGroupService catChargeGroupService;
        public CatChargeGroupController(ICatChargeGroupService service)
        {
            catChargeGroupService = service;
        }

        /// <summary>
        /// get all charges group
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public IActionResult GetAllChareGroup()
        {
            var data = catChargeGroupService.Get();
            return Ok(data);
        }
    }
}