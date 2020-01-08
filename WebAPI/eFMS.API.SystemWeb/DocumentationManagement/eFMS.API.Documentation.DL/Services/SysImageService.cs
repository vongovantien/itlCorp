using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class SysImageService : RepositoryBase<SysImage, SysImageModel>, ISysImageService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public SysImageService(IContextBase<SysImage> repository, 
            IMapper mapper,
            IHostingEnvironment hostingEnvironment) : base(repository, mapper)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<ResultHandle> UploadDocumentationImages(DocumentFileUploadModel model)
        {
            return await WriteFile(model);
        }

        private async Task<ResultHandle> WriteFile(DocumentFileUploadModel model)
        {
            string fileName = "";
            //string folderName = "images";
            string path = _hostingEnvironment.ContentRootPath;
            try
            {
                /* Kiểm tra các thư mục có tồn tại */
                ImageHelper.CreateDirectoryFile(model.FolderName, model.JobId.ToString());
                foreach (var file in model.Files)
                {
                    fileName = file.FileName;
                    await ImageHelper.SaveFile(fileName, model.FolderName, model.JobId.ToString(), file);
                    string urlFile = path + "/" + model.FolderName + "/" + "files" + "/" + fileName;
                    var result = new { link = urlFile };
                    var sysImage = new SysImage
                    {
                        Id = Guid.NewGuid(),
                        Url = urlFile,
                        Name = fileName,
                        Folder = model.FolderName ?? "Company"
                    };
                }
                return new ResultHandle();

            }
            catch (Exception ex)
            {
                var hs = new ResultHandle { Data = null, Status = false, Message = ex.Message };
                return hs;
            }
        }
    }
}
