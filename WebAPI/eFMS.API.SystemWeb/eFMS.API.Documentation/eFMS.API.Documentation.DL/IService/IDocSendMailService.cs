using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using System;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IDocSendMailService : IRepositoryBase<CsTransaction, CsTransactionModel>
    {
        bool SendMailDocument(EmailContentModel emailContent);
        EmailContentModel GetInfoMailHBLAirImport(Guid hblId);
        EmailContentModel GetInfoMailHBLAirExport(Guid hblId);
        EmailContentModel GetInfoMailSISeaExport(Guid jobId);
        EmailContentModel GetInfoMailHBLSeaImport(Guid jobId, string serviceId);
    }
}
