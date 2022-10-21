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

        Task<HandleState> PostEDocAsync(EDocUploadModel model, List<IFormFile> files);
        Task<List<EDocGroupByType>> GetEDocByJob(Guid jobId, string transactionType);
        Task<HandleState> DeleteEdoc(Guid edocId);
        Task<HandleState> MappingeDocToShipment(Guid imageId, string billingId, string billingType);
        Task<HandleState> UpdateEDoc(SysImageDetailModel edocUpdate);
        //Task<HandleState> GenEDoc(string type, Guid id, List<IFormFile> files);
        Task<HandleState> GenEdoc(FileUploadModel model);
    }
}
