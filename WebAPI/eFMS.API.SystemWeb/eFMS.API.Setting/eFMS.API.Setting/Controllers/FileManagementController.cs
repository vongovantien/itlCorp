﻿using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public IActionResult GetEdocManagement(EDocManagementCriterial criteria)
        {
            var data = fileManagementService.GetEdocManagement(criteria).Result;
            if (data == null)
            {
                return BadRequest();
            }
            return Ok(data);
        }
    }
}
