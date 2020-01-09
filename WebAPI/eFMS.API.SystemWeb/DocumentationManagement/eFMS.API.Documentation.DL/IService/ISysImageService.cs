using eFMS.API.Common;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ISysImageService: IRepositoryBase<SysImage, SysImageModel>
    {
        Task<ResultHandle> UploadDocumentationFiles(DocumentFileUploadModel model);
        Task<HandleState> DeleteFile(Guid id);
    }
}
