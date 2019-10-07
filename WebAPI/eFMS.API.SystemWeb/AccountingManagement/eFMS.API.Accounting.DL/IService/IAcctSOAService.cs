using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Linq;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctSOAService : IRepositoryBase<AcctSoa, AcctSoaModel>
    {
        HandleState AddSOA(AcctSoaModel model);

        HandleState UpdateSOA(AcctSoaModel model);

        HandleState UpdateSOASurCharge(string soaNo);

        IQueryable<AcctSOAResult> Paging(AcctSOACriteria criteria, int page, int size, out int rowsCount);
        
        //AcctSOADetailResult GetBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal);

        //object GetListServices();

        object GetListStatusSoa();

        //string GetInfoServiceOfSoa(string soaNo);

        //List<ChargeShipmentModel> GetListMoreChargeByCondition(MoreChargeShipmentCriteria criteria);

        IQueryable<ChargeShipmentModel> GetListMoreCharge(MoreChargeShipmentCriteria criteria);

        AcctSOADetailResult AddMoreCharge(AddMoreChargeCriteria criteria);

        ExportSOADetailResult GetDataExportSOABySOANo(string soaNo, string currencyLocal);

        IQueryable<ChargeSOAResult> GetChargeShipmentDocAndOperation();

        ChargeShipmentResult GetListChargeShipment(ChargeShipmentCriteria criteria);

        AcctSOADetailResult GetDetailBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal);

        IQueryable<AcctSOAResult> GetListSOA(AcctSOACriteria criteria);
    }
}
