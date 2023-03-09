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

        Task<HandleState> PostEDocAsync(EDocUploadModel model, List<IFormFile> files, string type);
        Task<List<EDocGroupByType>> GetEDocByJob(Guid jobId, string transactionType);
        EDocGroupByType GetEDocByAccountant(Guid billingId, string transactionType);
        Task<HandleState> DeleteEdoc(Guid edocId, Guid JobId);
        Task<HandleState> DeleteEdocAcc(string billingNo);
        Task<HandleState> MappingeDocToShipment(Guid imageId, string billingId, string billingType);
        Task<HandleState> UpdateEDoc(SysImageDetailModel edocUpdate);
        Task<HandleState> PostFileAttacheDoc(FileUploadModel model);
        Task<string> PostAttachFileTemplateToEDoc(FileUploadModel model);
        Task<HandleState> AttachPreviewTemplate(List<EDocAttachPreviewTemplateUploadModel> models);
        Task<HandleState> OpenFile(Guid Id);
        Task<HandleState> CreateEDocZip(FileDowloadZipModel model);
        Task<HandleState> GenEdocByBilling(string billingNo, string billingType);
        bool CheckAllowSettleEdocSendRequest(Guid settleId);
        Task<HandleState> UpdateEdocByAcc(EdocAccUpdateModel model);
    }
}
