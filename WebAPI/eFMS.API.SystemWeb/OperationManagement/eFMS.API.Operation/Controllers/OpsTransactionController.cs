using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Operation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class OpsTransactionController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IOpsTransactionService opsTransactionService;
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface IOpsTransactionService</param>
        /// <param name="distributedCache"></param>
        public OpsTransactionController(IStringLocalizer<DL.Common.LanguageSub> localizer, IOpsTransactionService service, IDistributedCache distributedCache, ICurrentUser user)
        {
            stringLocalizer = localizer;
            opsTransactionService = service;
            cache = distributedCache;
            currentUser = user;
        }

        /// <summary>
        /// get the list of opstransaction
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = opsTransactionService.Get();
            return Ok(results);
        }

        /// <summary>
        /// get and paging the list of ops transaction by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost("Paging")]
        public IActionResult Paging(OpsTransactionCriteria criteria, int pageNumber, int pageSize)
        {
            var data = opsTransactionService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }
    }
}