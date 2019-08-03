using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IAcctSOAService : IRepositoryBase<AcctSoa, AcctSoaModel>
    {
        HandleState AddSOA(AcctSoaModel model);

        List<AcctSOAResult> Paging(AcctSOACriteria criteria, int page, int size, out int rowsCount);

        HandleState UpdateSOASurCharge(string soaNo);

        AcctSOADetailResult GetBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal);

        object GetListServices();

        object GetListStatusSoa();

        string GetInfoServiceOfSoa(string soaNo);

        HandleState UpdateSOA(AcctSoaModel model);

        List<ChargeShipmentModel> GetListMoreChargeByCondition(MoreChargeShipmentCriteria criteria);

        AcctSOADetailResult AddMoreCharge(AddMoreChargeCriteria criteria);

        ExportSOADetailResult GetDataExportSOABySOANo(string soaNo);
    }
}
