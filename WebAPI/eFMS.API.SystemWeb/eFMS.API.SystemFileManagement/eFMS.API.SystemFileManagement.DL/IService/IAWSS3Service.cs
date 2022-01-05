using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;
using eFMS.API.Common;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.IService
{                                                                                                                                                                                  
    public interface IAWSS3Service 
    {
        Task<HandleState> PostObjectAsync(FileUploadModel model);
        Task<HandleState> DeleteFile(string moduleName,string folder, Guid id);
        Task<List<SysImage>> GetFileSysImage(string moduleName, string folder, Guid id, string child = null);
        Task<HandleState> OpenFile(string moduleName, string folder, Guid objId, string fileName);
        //List<SysImage> GetFiles(string folderName, Guid Id);
        Task<HandleState> CreateFileZip(FileDowloadZipModel model);
    }


    public class FileUploadModel
    {
        public List<IFormFile> Files { get; set; }
        public string FolderName { get; set; }
        public Guid Id { get; set; }
        public string Child { get; set; }
        public string ModuleName { get; set; }
    }

    public class FileDowloadZipModel
    {
        public string FolderName { get; set; }
        public string ObjectId { get; set; }
        public string ChillId { get; set; }
        public string FileName { get; set; }
    }
}
