using eFMS.API.SystemFileManagement.DL.Models;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.IService
{
    public interface IEDocService
    {
        Task<HandleState> GetDocumentType(string transactionType);
        Task<HandleState> PostEDocAsync(EDocUploadModel model, List<IFormFile> files);
        Task<HandleState> GetEDocByJob(Guid jobId, string transactionType);
        Task<HandleState> DeleteEdoc(Guid edocId);
    }
}
