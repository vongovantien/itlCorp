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

        [HttpGet]
        [Route("GetFileManagement")]
        [Authorize]
        public IActionResult Get(string folderName, string keyWord, int page, int size)
        {
            var data = fileManagementService.Get(folderName, keyWord, page, size, out int rowsCount);
            if (data == null)
            {
                return BadRequest();
            }
            var result = new { data, totalItems = rowsCount, page, size };
            return Ok(result);
        }

        [HttpGet]
        [Route("GetDetailFileManagement")]
        [Authorize]
        public IActionResult GetDetail(string folderName, string objectId)
        {
            var data = fileManagementService.GetDetail(folderName, objectId);
            if (data == null)
            {
                return BadRequest();
            }
            return Ok(data);
        }
    }
}
