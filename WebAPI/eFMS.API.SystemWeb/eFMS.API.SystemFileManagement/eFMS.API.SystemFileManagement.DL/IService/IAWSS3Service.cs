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
        Task<string> PostFileReportAsync(FileUploadModel model);
        Task<HandleState> DeleteFile(string moduleName,string folder, Guid id);
        Task<List<SysImage>> GetFileSysImage(string moduleName, string folder, Guid id, string child = null);
        Task<HandleState> OpenFile(string moduleName, string folder, Guid objId, string fileName);
        Task<HandleState> CreateFileZip(FileDowloadZipModel model);
    }
}
