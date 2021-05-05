using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountReceivable;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccAccountReceivableService : IRepositoryBase<AccAccountReceivable, AccAccountReceivableModel>
    {
        HandleState AddReceivable(AccAccountReceivableModel model);
        HandleState UpdateReceivable(AccAccountReceivableModel model);
        HandleState InsertOrUpdateReceivable(List<ObjectReceivableModel> models);
        HandleState AddOrUpdateReceivableMulti(List<ObjectReceivableModel> models);
        HandleState CalculatorReceivable(CalculatorReceivableModel model);
        HandleState CalculatorReceivableNotAuthorize(CalculatorReceivableNotAuthorizeModel model);
        AccountReceivableDetailResult GetDetailAccountReceivableByArgeementId(Guid argeementId);
        AccountReceivableDetailResult GetDetailAccountReceivableByPartnerId(string partnerId);
        IEnumerable<object> GetDataARByCriteria(AccountReceivableCriteria criteria);
        IEnumerable<object> Paging(AccountReceivableCriteria criteria, int page, int size, out int rowsCount);
        List<ObjectReceivableModel> GetObjectReceivableBySurcharges(IQueryable<CsShipmentSurcharge> surcharges);
    }
}
