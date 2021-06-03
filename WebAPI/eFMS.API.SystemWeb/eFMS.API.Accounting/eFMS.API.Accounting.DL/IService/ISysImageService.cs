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
        Task<HandleState> UploadFiles(FileUploadModel model);
        Task<HandleState> DeleteFile(string folder, Guid id);
        List<SysImage> GetFiles(string folderName, Guid Id);
        Task<HandleState> CreateFileZip(FileDowloadZipModel model);
    }

    public class FileUploadModel
    {
        public List<IFormFile> Files { get; set; }
        public string FolderName { get; set; }
        public Guid Id { get; set; }
        public string Child { get; set; }
    }

    public class FileDowloadZipModel   
    {
        public string FolderName { get; set; }
        public string ObjectId { get; set; }
        public string ChillId { get; set; }
        public string FileName { get; set; }
    }
}
