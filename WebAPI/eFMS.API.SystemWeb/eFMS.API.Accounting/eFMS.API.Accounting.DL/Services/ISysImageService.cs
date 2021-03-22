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
using System.Linq;
using System.Text;
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

        public async Task<HandleState> DeleteFile(Guid id)
        {
            SysImage item = DataContext.Get(x => x.Id == id).FirstOrDefault();

            if (item == null) return new HandleState("Not found data");

            HandleState result = DataContext.Delete(x => x.Id == id);
            if (result.Success)
            {
                var hs = await ImageHelper.DeleteFile(item.Name, item.ObjectId);
            }
            return result;
        }

        public async Task<ResultHandle> UploadFiles(FileUploadModel model)
        {
            return await WriteFile(model);
        }

        private async Task<ResultHandle> WriteFile(FileUploadModel model)
        {
            string fileName = "";
           
            string path = this.webUrl.Value.Url + "/Accounting";
            try
            {
                List<SysImage> list = new List<SysImage>();
                ResultHandle result = new ResultHandle();

                ImageHelper.CreateDirectoryFile(model.FolderName, model.Id.ToString());
                List<SysImage> resultUrls = new List<SysImage>();

                foreach (IFormFile file in model.Files)
                {
                    fileName = file.FileName;
                    string objectId = model.Id.ToString();
                    await ImageHelper.SaveFile(fileName, model.FolderName, objectId, file);

                    string urlImage = path + "/" + model.FolderName + "/" + objectId + "/" + fileName;
                    var sysImage = new SysImage
                    {
                        Id = Guid.NewGuid(),
                        Url = urlImage,
                        Name = file.Name,
                        Folder = model.FolderName,
                        ObjectId = model.Id.ToString(),
                        UserCreated = currentUser.UserName, 
                        UserModified = currentUser.UserName,
                        DateTimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now
                    };
                    list.Add(sysImage);
                }
                HandleState hs = new HandleState();
                if (list.Count > 0)
                {
                    hs = await DataContext.AddAsync(list);

                    result.Status = hs.Success;
                    result.Message = hs.Message?.ToString();
                    result.Data = list;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ResultHandle { Data = null, Status = false, Message = ex.Message };
            }
        }
    }
}
