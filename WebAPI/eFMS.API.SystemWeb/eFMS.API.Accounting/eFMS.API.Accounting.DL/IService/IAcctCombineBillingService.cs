using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.CombineBilling;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctCombineBillingService : IRepositoryBase<AcctCombineBilling, AcctCombineBillingModel>
    {
        List<AcctCombineBillingResult> Paging(AcctCombineBillingCriteria criteria, int page, int size, out int rowsCount);
        HandleState AddCombineBilling(AcctCombineBillingModel model);
        HandleState UpdateCombineBilling(AcctCombineBillingModel model);
        HandleState DeleteCombineBilling(Guid? id);
        string GenerateCombineBillingNo();
        string CheckDocumentNoExisted(ShipmentCombineCriteria criteria);
        bool CheckAllowViewDetailCombine(Guid id);
        AcctCombineBillingModel GetCombineBillingDetailList(ShipmentCombineCriteria criteria);
        bool CheckExistedCombineData(Guid id);
        AcctCombineBillingModel GetCombineBillingDetailWithId(string Id);
        CombineBillingDebitDetailsModel GetDataPreviewDebitNoteTemplate(AcctCombineBillingModel combineDetail);
        Crystal PreviewCombineDebitTemplate(CombineBillingDebitDetailsModel model);
        CombineOPSModel GetDataExportCombineOps(string combineBillingNo);
        CombineShipmentModel GetDataExportCombineShipment(string combineBillingNo);
        CombineOPSModel GetDataExportCombineOpsByPartner(AcctCombineBillingCriteria criteria);
        CombineShipmentModel GetDataExportCombineShipmentByPartner(AcctCombineBillingCriteria criteria);
        Crystal PreviewConfirmBilling(string combineBillingNo);
    }
}
