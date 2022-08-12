using eFMS.API.Common;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IDocSendMailService : IRepositoryBase<CsTransaction, CsTransactionModel>
    {
        bool SendMailDocument(EmailContentModel emailContent);
        EmailContentModel GetInfoMailHBLAirImport(Guid hblId);
        EmailContentModel GetInfoMailHBLAirExport(Guid? hblId);
        EmailContentModel GetInfoMailAEPreAlert(Guid? jobId);
        EmailContentModel GetInfoMailSISeaExport(Guid jobId);
        EmailContentModel GetInfoMailHBLSeaImport(Guid jobId, string serviceId);
        EmailContentModel GetInfoMailHBLPreAlerSeaExport(Guid? hblId, string serviceId);
        EmailContentModel GetInfoMailPreAlerSeaExport(Guid? jobId, string serviceId);
        bool SendMailContractCashWithOutstandingDebit();
        List<sp_GetShipmentDataWithOutstandingDebit> GetDataOustandingDebit(string salemanId);
    }
}
