﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// Controller Authorized Approval
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysAuthorizedApprovalController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysAuthorizedApprovalService authorService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        public SysAuthorizedApprovalController(IStringLocalizer<LanguageSub> localizer, ISysAuthorizedApprovalService service, ICurrentUser user, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            authorService = service;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("Query")]
        [Authorize]
        public IActionResult Query(SysAuthorizedApprovalCriteria criteria)
        {
            var data = authorService.Query(criteria);
            return Ok(data);
        }

        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(SysAuthorizedApprovalCriteria criteria, int page, int size)
        {
            var data = authorService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }


        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Add(SysAuthorizedApprovalModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = authorService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(SysAuthorizedApprovalModel model)
        {
            var hs = authorService.Update(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = authorService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}