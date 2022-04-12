using eFMS.API.Common;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Infrastructure.Middlewares;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
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

        /// <summary>
        /// Upload file report to aws then get href url of file
        /// </summary>
        /// <param name="files"></param>
        /// <param name="moduleName"></param>
        /// <param name="folder"></param>
        /// <param name="id"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        [HttpPut("UploadFilePreview/{moduleName}/{folder}/{id}")]
        public async Task<IActionResult> UploadFilePreview(FileReportUpload files, string moduleName, string folder, Guid id, string child = null)
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

            var hs = await _aWSS3Service.PostFileReportAsync(model);
            if (!string.IsNullOrEmpty(hs))
            {
                return Ok(new ResultHandle { Message = "Upload File Successfully", Status = true, Data = hs });
            };
            return BadRequest(hs);
        }

        [HttpPut("UploadImages/{moduleName}/{folder}/{id}")]
        //[Authorize]
        public async Task<IActionResult> UploadImages(IFormFile file, Guid id, string moduleName, string folder, string child = null)
        {
            List<IFormFile> files = new List<IFormFile>();
            files.Add(file);
            FileUploadModel model = new FileUploadModel
            {
                Files = files,
                FolderName = folder,
                Id = id,
                Child = child,
                ModuleName = moduleName
            };

            HandleState hs = await _aWSS3Service.PostObjectAsync(model);
            var imgUrl = _sysImageRepo.Get(x => x.Folder == folder && x.ObjectId == id.ToString()).OrderByDescending(x => x.DatetimeModified).FirstOrDefault().Url;
            if (hs.Success)
            {
                return Ok(new { link = imgUrl });
            }
            return BadRequest(hs);
        }

        [HttpDelete("DeleteSpecificFile/{moduleName}/{folder}/{id}/{fileName}")]
        public async Task<IActionResult> DeleteSpecificFile(string moduleName, string folder, Guid id, string fileName)
        {
            Guid imgID = _sysImageRepo.Where(x => x.ObjectId == id.ToString() && x.Name == fileName).FirstOrDefault().Id;
            HandleState hs = await _aWSS3Service.DeleteFile(moduleName, folder, imgID);
            if (hs.Success)
                return Ok(new ResultHandle { Message = "Delete File Successfully", Status = true });
            return BadRequest(hs);
        }

        [HttpGet("GetAttachedFiles/{moduleName}/{folder}/{id}")]
        public async Task<IActionResult> GetFiles(string moduleName,string folder, Guid id, string child = null)
        {
            List<SysImage> result = await _aWSS3Service.GetFileSysImage(moduleName,folder,id,child);  
            return Ok(result);
        }

        [HttpDelete("DeleteAttachedFile/{moduleName}/{folder}/{id}")]
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

        [HttpGet("DownloadFile/{moduleName}/{folder}/{objId}/{fileName}")]
        public IActionResult DownloadFile(string moduleName, string folder, Guid objId, string fileName)
        {
            try
            {
                var document = _aWSS3Service.DownloadFileAsync(moduleName, folder, objId, fileName).Result;
                return File(document, "application/octet-stream", fileName);
            }catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("MoveObjectAsync/{srcId}/{destId}/{type?}")]
        public IActionResult MoveObjectAsync(string srcId, string destId, int? type)
        {
            try
            {
                FileCoppyModel fileCoppy = new FileCoppyModel()
                {
                    srcKey = srcId,
                    destKey = destId,
                    Type = type,
                };
                var document = _aWSS3Service.MoveObjectAsync(fileCoppy).Result;
                if (!document.Success)
                {
                    return BadRequest();
                }
                return Ok(document);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

    }
}
