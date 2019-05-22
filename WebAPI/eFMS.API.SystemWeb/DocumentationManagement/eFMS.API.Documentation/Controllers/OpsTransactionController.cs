using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public OpsTransactionController(IStringLocalizer<LanguageSub> localizer, ICurrentUser user)
        {
            stringLocalizer = localizer;
            currentUser = user;
        }
    }
}
