using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IDatabaseUpdateService
    {
        sp_InsertRowToDataBase InsertDataToDB(object obj);
        void LogAddEntity(object entity);
        sp_InsertChargesAutoRate InsertChargesAutoRateToDB(List<CsShipmentSurcharge> surcharges, List<CsLinkCharge> linkCharges);
    }
}
