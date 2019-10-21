using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysEmployeeController : ControllerBase
    {
        private readonly ISysEmployeeService  sysEmployeeService;

        public SysEmployeeController(IStringLocalizer<LanguageSub> localizer, ISysEmployeeService isysEmployeeService,
      IMapper mapper, ICurrentUser currUser
      )
        {
            sysEmployeeService = isysEmployeeService;
        }
    }
}