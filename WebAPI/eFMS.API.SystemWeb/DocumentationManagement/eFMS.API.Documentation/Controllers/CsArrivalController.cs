using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsArrivalController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsArrivalFrieghtChargeService arrivalFreightChargeServices;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="freightChargeService"></param>
        /// <param name="user"></param>
        public CsArrivalController(IStringLocalizer<LanguageSub> localizer, ICsArrivalFrieghtChargeService freightChargeService, ICurrentUser user)
        {
            stringLocalizer = localizer;
            arrivalFreightChargeServices = freightChargeService;
            currentUser = user;
        }
    }
}
