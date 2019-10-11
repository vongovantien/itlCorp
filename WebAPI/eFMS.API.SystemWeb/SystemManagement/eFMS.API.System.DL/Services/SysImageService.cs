using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Common;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public async Task<ResultHandle> UploadImage(IFormFile file)
        {
            if (CheckIfImageFile(file))
            {
                return await WriteFile(file);
            }

            return new ResultHandle { Data = 1, Message = "Có lỗi xảy ra", Status = false };
        }

        private bool CheckIfImageFile(IFormFile file)
        {
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            return ImageHelper.GetImageFormat(fileBytes) != ImageHelper.ImageFormat.unknown;
        }

        public async Task<ResultHandle> WriteFile(IFormFile file)
        {
            string fileName = "";
            string folderName = "images";
            string path = webUrl.Value.Url.ToString();
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1]; //lấy extension

                fileName = file.FileName;
                //fileName = Guid.NewGuid().ToString(); // lấy filename

                var physicPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + folderName, fileName); // lưu vào folder

                using (var bits = new FileStream(physicPath, FileMode.Create))
                {
                    await file.CopyToAsync(bits);
                }

                string urlImage = path + "/images/" + fileName;
                var result = new { link = urlImage };


                sysImageCriteria image = new sysImageCriteria
                {
                    Name = fileName,
                    Url = urlImage,
                };

                HandleState x =  Add(image); // lưu db
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

        private HandleState Add(sysImageCriteria imageModel)
        {
            try
            {
                var userCurrent = "admin"; // TODO

                var sysImage = new SysImage
                {
                    Id = Guid.NewGuid(),
                    Url = imageModel.Url,
                    Name = imageModel.Name,
                };

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
    }

}
