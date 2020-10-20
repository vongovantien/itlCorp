using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.System.DL.Services
{
    public class SysImageService : RepositoryBase<SysImage, SysImageModel>, ISysImageService
    {
        private readonly IOptions<WebUrl> webUrl;

        public SysImageService(IContextBase<SysImage> repository, IMapper mapper, IOptions<WebUrl> url) : base(repository, mapper)
        {
            //currentUser = user;
            webUrl = url;

        }

        public async Task<ResultHandle> UploadImage(IFormFile file, string folderName, string ObjectId = null)
        {
            if (ImageHelper.CheckIfImageFile(file))
            {
                return await WriteFile(file, folderName, ObjectId);
            }

            return new ResultHandle { Data = 1, Message = "Có lỗi xảy ra", Status = false };
        }

        public async Task<ResultHandle> WriteFile(IFormFile file, string folderName, string ObjectId)
        {
            string fileName = "";
            //string folderName = "images";
            string path = webUrl.Value.Url;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1]; //lấy extension

                fileName = file.FileName;

                /* Kiểm tra các thư mục có tồn tại */
                ImageHelper.CreateDirectoryImage(folderName);
                await ImageHelper.SaveImage(fileName, folderName, file);
                string urlImage = path + "/images/" + folderName + "/" + fileName;
                var result = new { link = urlImage };
                var sysImage = new SysImage
                {
                    Id = Guid.NewGuid(),
                    Url = urlImage,
                    Name = fileName,
                    Folder = folderName ?? "Company",
                    ObjectId = ObjectId,
                };
                HandleState x =  Add(sysImage); // lưu db
                if(x.Success)
                {
                    return new ResultHandle { Data = result, Status = x.Success, Message = "" };
                }
                return new ResultHandle { Data = result, Status = x.Success, Message = "" };

            }
            catch (Exception ex)
            {
                var hs = new ResultHandle { Data = null, Status = false, Message = ex.Message };
                return hs;
            }
        }

        private HandleState Add(SysImage sysImage)
        {
            try
            {
                var userCurrent = "admin"; // TODO
                sysImage.Id = Guid.NewGuid();
                sysImage.DateTimeCreated = sysImage.DatetimeModified = DateTime.Now;
                sysImage.UserCreated = sysImage.UserModified = userCurrent;

                var hs = DataContext.Add(sysImage);
                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex);
                return hs;
            }
        }

        public HandleState Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<SysImageModel> GetAll()
        {
            var sysImage = DataContext.Get();
            return sysImage.ProjectTo<SysImageModel>(mapper.ConfigurationProvider);
        }

        public IQueryable<SysImageModel> GetImageCompany()
        {
            var result =  DataContext.Where(x => x.Folder == "Company");
            return result.ProjectTo<SysImageModel>(mapper.ConfigurationProvider);

        }

        public IQueryable<SysImageModel> GetImageUser(string userId)
        {
            var result = DataContext.Where(x => x.Folder == "User" && x.ObjectId == userId);
            return result.ProjectTo<SysImageModel>(mapper.ConfigurationProvider);
        }
    }

}
