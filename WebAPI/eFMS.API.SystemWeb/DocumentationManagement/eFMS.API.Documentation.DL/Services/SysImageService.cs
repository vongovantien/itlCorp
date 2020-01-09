using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class SysImageService : RepositoryBase<SysImage, SysImageModel>, ISysImageService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICurrentUser currentUser;
        public SysImageService(IContextBase<SysImage> repository, 
            IMapper mapper,
            IHostingEnvironment hostingEnvironment,
            ICurrentUser currUser) : base(repository, mapper)
        {
            _hostingEnvironment = hostingEnvironment;
            currentUser = currUser;
        }

        public async Task<HandleState> DeleteFile(Guid id)
        {
            var item = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (item == null) return new HandleState("Not found data");
            var result = DataContext.Delete(x => x.Id == id);
            if (result.Success)
            {
                var hs = await ImageHelper.DeleteFile(item.Url);
            }
            return result;
        }

        public async Task<ResultHandle> UploadDocumentationFiles(DocumentFileUploadModel model)
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
                var list = new List<SysImage>();
                /* Kiểm tra các thư mục có tồn tại */
                var hs = new HandleState();
                ImageHelper.CreateDirectoryFile(model.FolderName, model.JobId.ToString());
                foreach (var file in model.Files)
                {
                    fileName = file.FileName;
                    await ImageHelper.SaveFile(fileName, model.FolderName, model.JobId.ToString(), file);
                    //var s = Path.Combine(path, "wwwroot", model.FolderName, "files", model.JobId.ToString(), fileName);
                    //using (var fileStream = new FileStream(s, FileMode.Create))
                    //{
                    //    await file.CopyToAsync(fileStream);
                    //}

                    string urlFile = path + "/" + model.FolderName + "/files/" + model.JobId.ToString() + "/" + fileName;
                    var result = new { link = urlFile };
                    var sysImage = new SysImage
                    {
                        Id = Guid.NewGuid(),
                        Url = urlFile,
                        Name = fileName,
                        Folder = model.FolderName ?? "Company",
                        ObjectId = model.JobId.ToString(),
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        DateTimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now
                    };
                    list.Add(sysImage);
                    hs = await DataContext.AddAsync(sysImage);
                }
                return new ResultHandle { Data = list, Status = hs.Success, Message = hs.Message?.ToString() };

            }
            catch (Exception ex)
            {
                return new ResultHandle { Data = null, Status = false, Message = ex.Message };
            }
        }
    }
}
