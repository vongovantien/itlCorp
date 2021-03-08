using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctSOAService : IRepositoryBase<AcctSoa, AcctSoaModel>
    {
        HandleState AddSOA(AcctSoaModel model);

        HandleState UpdateSOA(AcctSoaModel model);

        HandleState DeleteSOA(string soaNo);

        //HandleState UpdateSOASurCharge(string soaNo);

        IQueryable<AcctSOAResult> Paging(AcctSOACriteria criteria, int page, int size, out int rowsCount);
        
        //AcctSOADetailResult GetBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal);

        //object GetListServices();

        object GetListStatusSoa();

        //string GetInfoServiceOfSoa(string soaNo);

        //List<ChargeShipmentModel> GetListMoreChargeByCondition(MoreChargeShipmentCriteria criteria);

        IQueryable<ChargeShipmentModel> GetListMoreCharge(MoreChargeShipmentCriteria criteria);

        AcctSOADetailResult AddMoreCharge(AddMoreChargeCriteria criteria);

        ExportSOADetailResult GetDataExportSOABySOANo(string soaNo, string currencyLocal);

        ChargeShipmentResult GetListChargeShipment(ChargeShipmentCriteria criteria);

        AcctSOADetailResult GetDetailBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal);

        IQueryable<AcctSOAResult> QueryData(AcctSOACriteria criteria);

        bool CheckDetailPermission(string soaNo);

        bool CheckDeletePermission(string soaNo);

        bool CheckUpdatePermission(string soaNo);

        IQueryable<ExportImportBravoFromSOAResult> GetDataExportImportBravoFromSOA(string soaNo);

        ExportSOAAirfreightModel GetSoaAirFreightBySoaNo(string soaNo,string officeId);

        ExportSOAAirfreightModel GetSoaSupplierAirFreightBySoaNo(string soaNo, string officeId);

        SOAOPSModel GetSOAOPS(string soaNo);

        Crystal PreviewAccountStatementFull(string soaNo);

        List<Guid> GetSurchargeIdBySoaId(int soaId);

        HandleState RejectSoaCredit(RejectSoaModel model);
    }
}
