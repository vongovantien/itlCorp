using eFMS.API.Common;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.DL.Services;
using eFMS.API.SystemFileManagement.Infrastructure.Middlewares;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public EDocController(IEDocService edocService, IContextBase<SysImage> SysImageRepo)
        {
            _edocService = edocService;
            _sysImageRepo = SysImageRepo;
        }
        [HttpGet("GetDocumentType")]
        public async Task<IActionResult> GetDocumentTypeAsync(string transactionType)
        {
            var result = await _edocService.GetDocumentType(transactionType);
            if (result == null)
            {
                return BadRequest(result);
            }
            return Ok(result);
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
    }
}
