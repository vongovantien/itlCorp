using eFMS.API.Accounting.Service.ViewModels;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IDatabaseUpdateService
    {
        sp_InsertRowToDataBase InsertDataToDB(object obj);
    }
}
