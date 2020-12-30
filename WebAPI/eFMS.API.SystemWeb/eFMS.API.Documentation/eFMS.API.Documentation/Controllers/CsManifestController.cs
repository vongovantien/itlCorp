using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsManifestController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsManifestService manifestService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        public CsManifestController(IStringLocalizer<LanguageSub> localizer, ICsManifestService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            manifestService = service;
            currentUser = user;
        }

        /// <summary>
        /// get manifest by jobId
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get(Guid jobId)
        {
            var result = manifestService.GetById(jobId);
            return Ok(result);
        }

        /// <summary>
        /// add/ update manifest
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddOrUpdateManifest")]
        [Authorize]
        public IActionResult AddOrUpdateManifest(CsManifestEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserCreated = model.UserModified = currentUser.UserID;
            model.CreatedDate = model.ModifiedDate = DateTime.Now;
            var hs = manifestService.AddOrUpdateManifest(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// preview manifest
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewSeaExportManifest")]
        [Authorize]
        public IActionResult PreviewSeaExportManifest(ManifestReportModel model)
        {
            var result = manifestService.PreviewSeaExportManifest(model);
            return Ok(result);
        }

        /// <summary>
        /// preview FCL Import Manifest
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewSeaImportManifest")]
        public IActionResult PreviewSeaImportManifest(ManifestReportModel model)
        {
            var result = manifestService.PreviewSeaImportManifest(model);
            return Ok(result);
        }

        /// <summary>
        /// preview air export manifest
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewAirExportManifest")]
        [Authorize]
        public IActionResult PreviewAirExportManifest(ManifestReportModel model)
        {
            var result = manifestService.PreviewAirExportManifest(model);
            return Ok(result);
        }

        /// <summary>
        /// preview air export manifest by jobId
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PreviewAirExportManifestByJobId")]
        public IActionResult PreviewAirExportManifestByJobId(Guid jobId)
        {
            var result = manifestService.PreviewAirExportManifestByJobId(jobId);
            return Ok(result);
        }

        /// <summary>
        /// preview sea export manifest by jobId
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PreviewSeaExportManifestByJobId")]
        public IActionResult PreviewSeaExportManifestByJobId(Guid jobId)
        {
            var result = manifestService.PreviewSeaExportManifestByJobId(jobId);
            return Ok(result);
        }

        /// <summary>
        /// Check if Job has manifest
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("CheckExistManifestExport")]
        [Authorize]

        public IActionResult CheckExistManifestExport(Guid jobId)
        {
            return Ok(manifestService.CheckExistManifestExport(jobId));
        }
    }
}
