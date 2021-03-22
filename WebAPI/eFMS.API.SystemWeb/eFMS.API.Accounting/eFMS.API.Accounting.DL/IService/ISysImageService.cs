using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface ISysImageService: IRepositoryBase<SysImage, SysImageModel> 
    {
        Task<ResultHandle> UploadFiles(FileUploadModel model);
        Task<HandleState> DeleteFile(Guid id);
    }

    public class FileUploadModel
    {
        public List<IFormFile> Files { get; set; }
        public string FolderName { get; set; }
        public Guid Id { get; set; }
    }
}
