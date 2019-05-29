using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Ecus;
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
        List<SetEcusConnectionModel> GetConnections();
        SetEcusConnectionModel GetConnectionDetails(int id);
        List<DTOKHAIMD> GetDataEcusByUser(string userId, string serverName, string dbusername, string dbpassword, string database);
        List<SetEcusConnectionModel> Paging(SetEcusConnectionCriteria criteria, int page_num, int page_size,out int total_items);

    }
}
