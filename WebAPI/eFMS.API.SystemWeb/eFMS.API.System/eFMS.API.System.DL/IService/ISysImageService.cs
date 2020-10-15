using System;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Http;

namespace eFMS.API.System.DL.IService
{
    public interface ISysImageService
    {
        Task<ResultHandle> UploadImage(IFormFile file, string folderName, string ObjectId);
        HandleState Delete(Guid id);
        IQueryable<SysImageModel> GetAll();
        IQueryable<SysImageModel> GetImageCompany();

    }
}
