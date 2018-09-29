using ITL.NetCore.Connection.BL;
using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public interface ISysBaseEnumDetailService: IRepositoryBase<SysBaseEnumDetail, SysBaseEnumDetailModel>
    {
        string ToString();
    }
}