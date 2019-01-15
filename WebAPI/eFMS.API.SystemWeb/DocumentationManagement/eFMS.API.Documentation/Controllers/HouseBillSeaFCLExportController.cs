using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class HouseBillSeaFCLExportController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IHouseBillSeaFCLExportService houseBillSeaFCLExportService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        public HouseBillSeaFCLExportController(IStringLocalizer<LanguageSub> localizer, IHouseBillSeaFCLExportService service, IMapper imapper, ICurrentUser user)
        {
            stringLocalizer = localizer;
            houseBillSeaFCLExportService = service;
            mapper = imapper;
            currentUser = user;
        }



    }
}