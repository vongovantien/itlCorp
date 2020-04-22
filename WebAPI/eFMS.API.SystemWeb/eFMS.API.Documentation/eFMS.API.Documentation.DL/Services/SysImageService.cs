﻿using AutoMapper;
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
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace eFMS.API.Documentation.DL.Services
{
    public class SysImageService : RepositoryBase<SysImage, SysImageModel>, ISysImageService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICurrentUser currentUser;
        private readonly IOptions<WebUrl> webUrl;
        public SysImageService(IContextBase<SysImage> repository, 
            IMapper mapper,
            IHostingEnvironment hostingEnvironment,
            ICurrentUser currUser, 
            IOptions<WebUrl> url) : base(repository, mapper)
        {
            _hostingEnvironment = hostingEnvironment;
            currentUser = currUser;
            webUrl = url;
        }

        public async Task<HandleState> DeleteFile(Guid id)
        {
            var item = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (item == null) return new HandleState("Not found data");
            var result = DataContext.Delete(x => x.Id == id);
            if (result.Success)
            {
                var hs = await ImageHelper.DeleteFile(item.Name, item.ObjectId);
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
            string path = this.webUrl.Value.Url;
            try
            {
                var list = new List<SysImage>();
                /* Kiểm tra các thư mục có tồn tại */
                var hs = new HandleState();
                ImageHelper.CreateDirectoryFile(model.FolderName, model.JobId.ToString());
                List<SysImage> resultUrls = new List<SysImage>();
                foreach (var file in model.Files)
                {
                    fileName = file.FileName;
                    string objectId = model.JobId.ToString();
                    await ImageHelper.SaveFile(fileName, model.FolderName, objectId, file);
                    string urlImage = path + "/" + model.FolderName + "files/" + objectId + "/" + fileName;
                    var sysImage = new SysImage
                    {
                        Id = Guid.NewGuid(),
                        Url = urlImage,
                        Name = fileName,
                        Folder = model.FolderName ?? "Shipment",
                        ObjectId = model.JobId.ToString(),
                        UserCreated = currentUser.UserName, //admin.
                        UserModified = currentUser.UserName,
                        DateTimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now
                    };
                    resultUrls.Add(sysImage);
                    if (!DataContext.Any(x => x.ObjectId == objectId && x.Url == urlImage))
                    {
                        list.Add(sysImage);
                    }
                }
                if(list.Count > 0)
                {
                    hs = await DataContext.AddAsync(list);
                }
                return new ResultHandle { Data = resultUrls, Status = hs.Success, Message = hs.Message?.ToString() };

            }
            catch (Exception ex)
            {
                return new ResultHandle { Data = null, Status = false, Message = ex.Message };
            }
        }
    }
}
