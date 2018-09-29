using ITL.NetCore.Connection.BL;
using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public interface ICatHubService: IRepositoryBase<CatHub, CatHubModel>
    {
        string ToString();
    }
}