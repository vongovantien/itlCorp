using eFMS.API.Common;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ISysImageService: IRepositoryBase<SysImage, SysImageModel>
    {
        Task<ResultHandle> UploadDocumentationFiles(DocumentFileUploadModel model);
        Task<HandleState> DeleteFile(Guid id);
        HandleState UpdateFilesToShipment(List<SysImageModel> files);
        Task<HandleState> DeleteFileTempPreAlert(Guid jobId);

    }
}
