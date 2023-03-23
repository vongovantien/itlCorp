using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Linq;

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
        [Route("GetFileManagement")]
        [Authorize]
        public IActionResult Get(FileManagementCriteria criteria, int page, int size)
        {
            var data = fileManagementService.Get(criteria, page, size, out int rowsCount);
            if (data == null)
            {
                return BadRequest();
            }
            var result = new ResponsePagingModel<SysImageViewModel> { Data = data, Page = page, Size = size, TotalItems = rowsCount };
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

        [HttpPost]
        [Route("GetEdocManagement")]
        [Authorize]
        public IActionResult GetEdocManagement(EDocManagementCriterial criteria, int pageNumber, int pageSize)
        {
            var data = fileManagementService.GetEdocManagement(criteria,pageNumber,pageSize).Result;
            if (data == null)
            {
                return BadRequest();
            }
            var result = new ResponsePagingModel<EDocFile>()
            {
                Data = data.Data,
                Page = pageNumber,
                Size = pageSize,
                TotalItems = data.TotalItem
            };
            return Ok(result);
        }
    }
}
