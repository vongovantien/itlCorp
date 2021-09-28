using eFMS.API.Common.Globals;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.DL.Models.Ecus;
using eFMS.API.Operation.Service.Models;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Operation.DL.IService
{
    public interface IEcusConnectionService : IRepositoryBaseCache<SetEcusconnection, SetEcusConnectionModel>
    {
        List<SetEcusConnectionModel> GetConnections();
        SetEcusConnectionModel GetConnectionDetails(int id);
        List<DTOKHAIMD> GetDataEcusByUser(string userId, string serverName, string dbusername, string dbpassword, string database);
        IQueryable<SetEcusConnectionModel> Paging(SetEcusConnectionCriteria criteria, int page_num, int page_size, out int total_items);

        bool CheckAllowPermissionAction(int id, PermissionRange permissionRange);
        List<D_OLA> GetDataOlaEcusByUser(string userId, string serverName, string dbusername, string dbpassword, string dbname);
    }
}
