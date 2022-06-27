using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountReceivable;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccAccountReceivableService : IRepositoryBase<AccAccountReceivable, AccAccountReceivableModel>
    {
        HandleState InsertOrUpdateReceivable(List<ObjectReceivableModel> models);
        HandleState CalculatorReceivable(CalculatorReceivableModel model);
        HandleState CalculatorReceivableNotAuthorize(CalculatorReceivableNotAuthorizeModel model);
        AccountReceivableDetailResult GetDetailAccountReceivableByArgeementId(Guid argeementId);
        AccountReceivableDetailResult GetDetailAccountReceivableByPartnerId(string partnerId,string saleManId);
        IEnumerable<object> GetDataARByCriteria(AccountReceivableCriteria criteria);
        IEnumerable<object> Paging(AccountReceivableCriteria criteria, int page, int size, out int rowsCount);
        List<ObjectReceivableModel> GetObjectReceivableBySurcharges(IQueryable<CsShipmentSurcharge> surcharges);
        IEnumerable<object> GetDataARSumaryExport(AccountReceivableCriteria criteria);
        List<sp_GetBillingWithSalesman> GetDataDebitDetail(AcctReceivableDebitDetailCriteria creteria);
        Task<HandleState> UpdateDueDateAndOverDaysAfterChangePaymentTerm(CatContractModel contractModel);
        Task<HandleState> CalculatorReceivableAsync(CalculatorReceivableModel model);
        Task<HandleState> InsertOrUpdateReceivableAsync(List<ObjectReceivableModel> models);
        HandleState CalculatorReceivableOverDue1To15Day(List<string> partnerIds, out List<Guid?> contractIdstoUpdate);
        HandleState CalculatorReceivableOverDue15To30Day(List<string> partnerIds, out List<Guid?> contractIdstoUpdate);
        HandleState CalculatorReceivableOverDue30Day(List<string> partnerIds, out List<Guid?> contractIdstoUpdate);
        Task<HandleState> CalculatorReceivableDebitAmountAsync(List<ObjectReceivableModel> models);
        Task<HandleState> CalculateAgreementFlag(List<Guid?> contractIds, string flag);
        List<ObjectReceivableModel> CalculatorReceivableByBillingCode(string code, string type);
        IEnumerable<object> GetDebitDetailByPartnerId(ArDebitDetailCriteria model);
        DebitAmountDetail GetDebitAmountDetailByContract(AccAccountReceivableCriteria criteria);
    }
}
