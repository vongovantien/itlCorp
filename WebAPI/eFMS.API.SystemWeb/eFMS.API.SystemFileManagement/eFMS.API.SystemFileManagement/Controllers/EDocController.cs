using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.DL.Services;
using eFMS.API.SystemFileManagement.Infrastructure.Middlewares;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class EDocController : ControllerBase
    {

        private IEDocService _edocService;
        private IContextBase<SysImage> _sysImageRepo;
        private readonly IStringLocalizer stringLocalizer;

        public EDocController(IEDocService edocService, IContextBase<SysImage> SysImageRepo, IStringLocalizer<LanguageSub> localizer)
        {
            _edocService = edocService;
            _sysImageRepo = SysImageRepo;
            stringLocalizer = localizer;
        }


        [HttpPut("UploadEdoc")]
        //[Authorize]
        public async Task<IActionResult> UploadEdoc([FromForm] EDocUploadModel edocUploadModel, List<IFormFile> files,string type)
        {
            HandleState hs = await _edocService.PostEDocAsync(edocUploadModel, files,type);
            if (hs.Success)
            {
                return Ok(new ResultHandle { Message = "Upload File Successfully", Status = true });
            }
            return BadRequest(hs);
        }

        [HttpGet("GetEDocByJob")]
        public async Task<IActionResult> GetEDocByJob(Guid jobId, string transactionType)
        {
            var result = await _edocService.GetEDocByJob(jobId, transactionType);
            if (result == null)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("GetEDocByAccountant")]
        public async Task<IActionResult> GetEDocByAccountant(string billingNo, string transactionType)
        {
            var result = await _edocService.GetEDocByAccountant(billingNo, transactionType);
            if (result == null)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("DeleteEDoc/{edocId}")]
        public async Task<IActionResult> DeleteEDoc(Guid edocId)
        {
            HandleState hs = await _edocService.DeleteEdoc(edocId);
            if (hs.Success)
                return Ok(new ResultHandle { Message = "Delete File Successfully", Status = true });
            return BadRequest(hs);
        }

        [HttpPut]
        [Route("UpdateEdoc")]
        //[Authorize]
        public async Task<IActionResult> Update(SysImageDetailModel model)
        {
            var hs = await _edocService.UpdateEDoc(model);
            if (!hs.Success)
            {
                return BadRequest(hs);
            }
            return Ok(new ResultHandle { Status = hs.Success, Message = "Update Edoc Success" });
        }

        [HttpPut("UploadEDocFromAccountant/{moduleName}/{folder}/{id}")]
        //[Authorize]
        public async Task<IActionResult> UploadFilesAttachEDoc(List<IFormFile> files, Guid id, string moduleName, string folder)
        {
            FileUploadModel model = new FileUploadModel
            {
                Files = files,
                FolderName = folder,
                Id = id,
                Child = null,
                ModuleName = moduleName,
            };
            HandleState hs = await _edocService.UploadEDocFromAccountant(model);
            if (hs.Success)
            {
                return Ok(new ResultHandle { Message = "Upload File Successfully", Status = true });
            }
            return BadRequest(hs);
        }

    }
}
