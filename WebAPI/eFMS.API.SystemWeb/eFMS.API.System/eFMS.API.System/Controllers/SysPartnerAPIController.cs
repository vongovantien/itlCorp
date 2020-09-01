using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysPartnerAPIController : ControllerBase
    {

        // private readonly IStringLocalizer stringLocalizer;
        private readonly IMapper Imapper;
        private readonly ICurrentUser currentUser;
        private readonly ISysPartnerAPIService sysPartnerAPIService;

        public SysPartnerAPIController(
            //IStringLocalizer localizer,
            IMapper mapper,
            ICurrentUser user, 
            ISysPartnerAPIService service)
        {
            //stringLocalizer = localizer;
            Imapper = mapper;
            currentUser = user;
            sysPartnerAPIService = service;
        }

        [HttpGet("GenerateAPIKey")]
        public IActionResult GetSettingFlowByOffice()
        {
            var apiKey = sysPartnerAPIService.GenerateAPIKey();

            return Ok(new { apiKey });
        }

        [HttpPost("AddNew")]
        [Authorize]
        public IActionResult AddNew(string apiKey)
        {
            HandleState result = sysPartnerAPIService.Add(apiKey);
            if(result.Success)
            {
                return Ok(result);
            }
            return BadRequest();
        }
    }
}