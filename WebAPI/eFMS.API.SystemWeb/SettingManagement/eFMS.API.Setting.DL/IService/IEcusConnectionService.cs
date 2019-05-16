using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.IService
{
    public interface IEcusConnectionService:IRepositoryBase<SetEcusconnection,SetEcusConnectionModel>
    {
        List<SetEcusConnectionModel> getConnections();
        SetEcusConnectionModel getConnectionDetails(int connection_id);
        List<SetEcusConnectionModel> Paging(SetEcusConnectionCriteria criteria, int page_num, int page_size,out int total_items);

    }
}
