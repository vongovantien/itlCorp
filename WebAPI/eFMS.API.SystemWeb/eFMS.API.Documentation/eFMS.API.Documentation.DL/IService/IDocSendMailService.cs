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
        EmailContentModel GetMailAuthorizeLetterHBLAirImport(Guid hblId);
        EmailContentModel GetMailProofOfDeliveryHBLAir(Guid hblId);
        EmailContentModel GetMailSendHAWBHBLAir(Guid hblId);
        EmailContentModel GetMailProofOfDeliveryHBLSea(Guid hblId, string serviceId);
        EmailContentModel GetMailSendHBLSeaServices(Guid hblId, string serviceId);
        EmailContentModel GetInfoMailHBLAirExport(Guid? hblId);
        EmailContentModel GetInfoMailAEPreAlert(List<Guid?> hblIds, Guid? jobId);
        EmailContentModel GetInfoMailSISeaExport(Guid jobId);
        EmailContentModel GetInfoMailHBLSeaImport(Guid jobId, string serviceId);
        EmailContentModel GetMailDOHBLSeaImport(Guid jobId, string serviceId);
        EmailContentModel GetInfoMailHBLPreAlerSeaExport(Guid? hblId, string serviceId);
        EmailContentModel GetInfoMailPreAlerSeaExport(List<Guid?> hblIds, Guid? jobId, string serviceId);
        EmailContentModel GetInfoMailDebitInvoice(Guid hblId, Guid jobId);
        bool SendMailContractCashWithOutstandingDebit();
        List<sp_GetShipmentDataWithOutstandingDebit> GetDataOustandingDebit(string salemanId);
    }
}
