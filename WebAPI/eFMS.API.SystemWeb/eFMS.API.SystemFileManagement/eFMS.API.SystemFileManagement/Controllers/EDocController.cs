using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Infrastructure.Middlewares;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
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
        public async Task<IActionResult> UploadEdoc([FromForm] EDocUploadModel edocUploadModel, List<IFormFile> files, string type)
        {
            HandleState hs = await _edocService.PostEDocAsync(edocUploadModel, files, type);
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
        public async Task<IActionResult> GetEDocByAccountant(Guid billingId, string transactionType)
        {
            var result = _edocService.GetEDocByAccountant(billingId, transactionType);
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

        [HttpPut("UploadAttachedFileEdoc/{moduleName}/{folder}/{id}")]
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
            HandleState hs = await _edocService.PostFileAttacheDoc(model);
            if (hs.Success)
            {
                return Ok(new ResultHandle { Message = "Upload File Successfully", Status = true });
            }
            return BadRequest(new ResultHandle { Message = "Upload File fail", Status = false, Data = model });
        }

        [HttpPut("PostAttachFileTemplateToEDoc/{moduleName}/{folder}/{id}")]
        public async Task<IActionResult> UploadAttachedFileEdoc(FileReportUpload files, string moduleName, string folder, Guid id, string child = null)
        {
            var stream = new MemoryStream(files.FileContent);
            var fFile = new FormFile(stream, 0, stream.Length, null, files.FileName);
            var fFiles = new List<IFormFile>() { fFile };
            FileUploadModel model = new FileUploadModel
            {
                Files = fFiles,
                FolderName = folder,
                Id = id,
                Child = child,
                ModuleName = moduleName
            };

            string fileUrl = await _edocService.PostAttachFileTemplateToEDoc(model);
            if (!string.IsNullOrEmpty(fileUrl))
            {
                return Ok(new ResultHandle { Message = "Upload File Successfully", Status = true, Data = fileUrl });
            };
            return BadRequest(new ResultHandle { Message = "Upload File fail", Status = false, Data = fileUrl });
        }

        [HttpPost("UploadPreviewTemplateToEDoc")]
        [Authorize]
        public async Task<IActionResult> UploadPreviewTemplateToEDoc(List<EDocAttachPreviewTemplateUploadModel> models)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultHandle { Message = "Not found files", Status = false, Data = models });
            }

            HandleState hs = await _edocService.AttachPreviewTemplate(models);
            if (hs.Success)
            {
                return Ok(new ResultHandle { Message = "Upload File Successfully", Status = true });
            }
            return BadRequest(new ResultHandle { Message = string.IsNullOrEmpty(hs.Message.ToString()) ? "Upload File fail" : hs.Message.ToString(), Status = false, Data = models });
        }

        //[HttpGet("OpenEDocFile/{moduleName}/{folder}/{objId}/{aliasName}")]
        //public async Task<IActionResult> OpenEdocFile(string moduleName, string folder, Guid objId, string aliasName)
        //{
        //    HandleState hs = await _edocService.OpenEdocFile(moduleName, folder, objId, aliasName);
        //    if (hs.Success)
        //        return Ok(hs.Message);
        //    return BadRequest(hs);
        //}

        [HttpGet("OpenFile/{Id}")]
        public async Task<IActionResult> OpenFileAliasName(Guid Id)
        {
            HandleState hs = await _edocService.OpenFile(Id);
            if (hs.Success)
                return Ok(hs.Message);
            return BadRequest(hs);
        }

        [HttpPost("DowloadAllEDoc")]
        public async Task<IActionResult> DowloadAllEDoc(FileDowloadZipModel m)
        {
            HandleState hs = await _edocService.CreateEDocZip(m);
            if (hs.Success)
                return File((byte[])hs.Message, "application/zip", m.FileName);
            return BadRequest(hs);
        }

        [HttpGet("GenEdocFromBilling")]
        public async Task<IActionResult> GenEdocFromBilling(string BillingNo, string BillingType)
        {
            HandleState hs = await _edocService.GenEdocByBilling(BillingNo, BillingType);
            if (hs.Success)
                return Ok(new ResultHandle { Message = "Get File Successfully", Status = true });
            if (hs.Exception.Message == "Not found file")
            {
                return Ok(new ResultHandle { Message = "Not found file", Status = false });
            }
            return BadRequest(hs);
        }
        [HttpGet("CheckAllowSettleEdocSendRequest")]
        public async Task<IActionResult> CheckAllowSettleEdocSendRequest(Guid billingId)
        {
            var result = _edocService.CheckAllowSettleEdocSendRequest(billingId);
            return Ok(result);
        }

        [HttpPut("UploadEdoc")]
        //[Authorize]
        public async Task<IActionResult> UploadEdocBankInfo([FromForm] EDocUploadModel edocUploadModel, List<IFormFile> files, string type)
        {
            HandleState hs = await _edocService.PostEDocAsync(edocUploadModel, files, type);
            if (hs.Success)
            {
                return Ok(new ResultHandle { Message = "Upload File Successfully", Status = true });
            }
            return BadRequest(hs);
        }
    }
}
