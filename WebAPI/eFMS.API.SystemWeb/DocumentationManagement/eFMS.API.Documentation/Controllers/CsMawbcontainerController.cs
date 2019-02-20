using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Resources;

namespace eFMS.API.Documentation.Controllers
{
    public class CsMawbcontainerController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsMawbcontainerService csContainerService;
        public CsMawbcontainerController(IStringLocalizer<LanguageSub> localizer, ICsMawbcontainerService service)
        {
            stringLocalizer = localizer;
            csContainerService = service;
        }
    }
}
