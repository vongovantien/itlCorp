using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class FileManagementController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private ICurrentUser currentUser;
        private IFileManagementService fileManagementService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public FileManagementController(IStringLocalizer<LanguageSub> localizer, IFileManagementService service, ICurrentUser currUser, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            fileManagementService = service;
            currentUser = currUser;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        [Route("Get")]
        [Authorize]
        public IActionResult Get(string folderName, List<string> Ids)
        {
            var data = fileManagementService.Get(folderName, Ids);
            if (data == null)
            {
                return BadRequest();
            }
            var result = new { data };
            return Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [Authorize]
        public IActionResult Search(SysImageCriteria criteria, int pageNumber, int pageSize)
        {
            var data = fileManagementService.Search(criteria, pageNumber, pageSize, out int rowCount);
            if (data == null)
            {
                return BadRequest();
            }
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }
    }
}
