using eFMS.API.Setting.DL.Models;
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
    }
}
