using eFMS.API.Common;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Infrastructure.Middlewares;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AWSS3Controller : ControllerBase
    {
        private IAWSS3Service _aWSS3Service;
        private IContextBase<SysImage> _sysImageRepo;

        public AWSS3Controller(IAWSS3Service aWSS3Service, IContextBase<SysImage> SysImageRepo) 
        {
            _aWSS3Service = aWSS3Service;
            _sysImageRepo = SysImageRepo;
        }
        [HttpPost]
        public IActionResult Index()
        {
            ResultHandle _result = new ResultHandle();
            return Ok(_result);
        }

        [HttpPut("UploadAttachedFiles/{moduleName}/{folder}/{id}")]
        //[Authorize]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files, Guid id, string moduleName, string folder, string child = null)
        {
            FileUploadModel model = new FileUploadModel
            {
                Files = files,
                FolderName = folder,
                Id = id,
                Child = child,
                ModuleName = moduleName
            };
            HandleState hs = await _aWSS3Service.PostObjectAsync(model);
            if (hs.Success)
            {
                return Ok(new ResultHandle { Message = "Upload File Successfully", Status = true });
            }
            return BadRequest(hs);
        }

        [HttpGet("GetAttachedFiles/{moduleName}/{folder}/{id}")]
        public async Task<IActionResult> GetFiles(string moduleName,string folder, Guid id, string child = null)
        {
            List<SysImage> result = await _aWSS3Service.GetFileSysImage(moduleName,folder,id,child);  
            return Ok(result);
        }

        [HttpDelete("DeleteAttachedFile/{moduleName}/{folder}/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAttachedFile(string moduleName, string folder, Guid id)
        {
            HandleState hs = await _aWSS3Service.DeleteFile(moduleName,folder, id);
            if (hs.Success)
                return Ok(new ResultHandle { Message = "Delete File Successfully", Status = true });
            return BadRequest(hs);
        }

        [HttpGet("OpenFile/{moduleName}/{folder}/{objId}/{fileName}")]
        public async Task<IActionResult> OpenFile(string moduleName, string folder, Guid objId, string fileName)
        {
            HandleState hs = await _aWSS3Service.OpenFile(moduleName, folder, objId, fileName);
            if (hs.Success)
                return Ok(hs.Message);
            return BadRequest(hs);
        }

        [HttpPost("DowloadAllFileAttached")]
        public async Task<IActionResult> DowloadAllFileAttached(FileDowloadZipModel m)
        {
            HandleState hs = await _aWSS3Service.CreateFileZip(m);
            if (hs.Success)
                return File((byte[])hs.Message, "application/zip", m.FileName);
            return BadRequest(hs);
        }
    }
}
