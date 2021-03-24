using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsArrivalFrieghtChargeService: IRepositoryBase<CsArrivalFrieghtCharge, CsArrivalFrieghtChargeModel>
    {
        CsArrivalViewModel GetArrival(Guid hblid, string transactionType);
        CsArrivalDefaultModel GetArrivalDefault(string transactionType, string userDefault);
        DeliveryOrderViewModel GetDeliveryOrder(Guid hblid, string transactionType);
        CsDeliveryOrderDefaultModel GetDeliveryOrderDefault(string transactionType, string userDefault);
        HandleState UpdateArrival(CsArrivalViewModel model);
        HandleState UpdateDeliveryOrder(DeliveryOrderViewModel model);
        HandleState SetArrivalChargeDefault(CsArrivalFrieghtChargeDefaultEditModel model);
        HandleState SetArrivalHeaderFooterDefault(CsArrivalDefaultModel model);
        HandleState SetDeliveryOrderHeaderFooterDefault(CsDeliveryOrderDefaultModel model);
        Crystal PreviewDeliveryOrder(Guid hblid);
        Crystal PreviewArrivalNoticeSIF(PreviewArrivalNoticeCriteria criteria);
        Crystal PreviewArrivalNoticeAir(PreviewArrivalNoticeCriteria criteria);
        ProofOfDeliveryViewModel GetProofOfDelivery(Guid hblid);
        HandleState UpdateProofOfDelivery(ProofOfDeliveryViewModel model);
        Task<ResultHandle> UploadProofOfDeliveryFile(ProofDeliveryFileUploadModel model);
        SysImage GetFileProofOfDelivery(Guid hblId);
        Task<HandleState> DeleteFilePOD(Guid id);

    }
}
