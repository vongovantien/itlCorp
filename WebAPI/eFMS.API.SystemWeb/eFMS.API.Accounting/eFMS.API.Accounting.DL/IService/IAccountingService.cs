using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccountingService : IRepositoryBase<AccAccountingManagement, AccAccountingManagementModel>
    {
        List<BravoAdvanceModel> GetListAdvanceToSyncBravo(List<Guid> Ids);
    }
}
