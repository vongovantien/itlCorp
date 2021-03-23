using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class SysImageService : RepositoryBase<SysImage, SysImageModel>, ISysImageService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICurrentUser currentUser;
        private readonly IOptions<ApiUrl> webUrl;

        public SysImageService(IContextBase<SysImage> repository,
            IMapper mapper,
            IHostingEnvironment hostingEnvironment,
            ICurrentUser currentUser, IOptions<ApiUrl> webUrl) : base(repository, mapper)
        {
            _hostingEnvironment = hostingEnvironment;
            this.currentUser = currentUser;
            this.webUrl = webUrl;
        }

        public async Task<HandleState> DeleteFile(string folder, Guid id)
        {
            SysImage item = DataContext.Get(x => x.Id == id).FirstOrDefault();

            if (item == null) return new HandleState("Not found data");

            HandleState result = DataContext.Delete(x => x.Id == id);
            if (result.Success)
            {
                string fileName = Path.GetFileName(item.Url);
                var hs = await ImageHelper.DeleteFile(item.ObjectId + "\\" + fileName, folder);
            }
            return result;
        }

        public List<SysImage> GetFiles(string folderName, Guid Id)
        {
            throw new NotImplementedException();
        }

        public async Task<HandleState> UploadFiles(FileUploadModel model)
        {
            return await WriteFile(model);
        }

        private async Task<HandleState> WriteFile(FileUploadModel model)
        {
            string path = this.webUrl.Value.Url; // Local
            //string path = this.webUrl.Value.Url + "/Accounting"; // Server
            try
            {
                List<SysImage> list = new List<SysImage>();
                HandleState result = new HandleState();

                ImageHelper.CreateDirectoryFile(model.FolderName, model.Id.ToString());
                List<SysImage> resultUrls = new List<SysImage>();

                foreach (IFormFile file in model.Files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    fileName = Regex.Replace(StringHelper.RemoveSign4VietnameseString(fileName), @"\s+", "") + "_" + StringHelper.RandomString(5);

                    string fullFileName = fileName + extension;

                    string objectId = model.Id.ToString();
                    await ImageHelper.SaveFile(fullFileName, model.FolderName, objectId, file);

                    string urlImage = path + "/" + model.FolderName + "/files/" + objectId + "/" + fullFileName;
                    var sysImage = new SysImage
                    {
                        Id = Guid.NewGuid(),
                        Url = urlImage,
                        Name = file.FileName,
                        Folder = model.FolderName,
                        ObjectId = model.Id.ToString(),
                        UserCreated = currentUser.UserName,
                        UserModified = currentUser.UserName,
                        DateTimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        ChildId = model.Child
                    };
                    list.Add(sysImage);
                }
                if (list.Count > 0)
                {
                    result = await DataContext.AddAsync(list);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
    }
}
