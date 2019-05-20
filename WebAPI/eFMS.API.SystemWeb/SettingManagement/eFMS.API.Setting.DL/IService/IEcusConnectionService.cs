using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Ecus;
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
        object Test();
        List<SetEcusConnectionModel> GetConnections();
        SetEcusConnectionModel GetConnectionDetails(int connection_id);
        List<DTOKHAIMD> GetDataEcusByUser(string userId, string serverName, string dbusername, string dbpassword, string database);
    }
}
