using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IDatabaseUpdateService
    {
        sp_InsertRowToDataBase InsertDataToDB(object obj);
        void UpdateSurchargeSettleDataToDB(List<CsShipmentSurcharge> surcharges, string settleCode, decimal kickBackExcRate, string action);
        void UpdateSurchargeAfterSynced(string type, string code);
    }
}
