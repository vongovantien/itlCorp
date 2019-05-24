using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class OpsTransactionController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IOpsTransactionService transactionService;

        public OpsTransactionController(IStringLocalizer<LanguageSub> localizer, ICurrentUser user, IOpsTransactionService service)
        {
            stringLocalizer = localizer;
            currentUser = user;
            transactionService = service;
        }

        [HttpPost("Paging")]
        public IActionResult Query(OpsTransactionCriteria criteria, int page, int size)
        {
            var results = transactionService.Query(criteria);
            return Ok(results);
        }

        [HttpPost("Query")]
        public IActionResult Query(OpsTransactionCriteria criteria)
        {
            var results = transactionService.Query(criteria);
            return Ok(results);
        }
    }
}
