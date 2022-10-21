using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
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
        public async Task<IActionResult> UploadEdoc([FromForm] EDocUploadModel edocUploadModel, List<IFormFile> files)
        {
            HandleState hs = await _edocService.PostEDocAsync(edocUploadModel, files);
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

        [HttpPut]
        [Route("GenEdoc")]
        //[Authorize]
        public async Task<IActionResult> GenEdoc(string type,Guid id,List<IFormFile> files)
        {
            //var hs = await _edocService.GenEDoc(type, id, files);
            //var hs = await _edocService.GenEDoc(x);
            var hs = new HandleState();
            if (!hs.Success)
            {
                return BadRequest(hs);
            }
            return Ok(new ResultHandle { Status = hs.Success, Message = "Update Edoc Success" });
        }
    }
}
