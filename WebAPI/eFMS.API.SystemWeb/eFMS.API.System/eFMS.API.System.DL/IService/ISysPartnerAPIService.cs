using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;


namespace eFMS.API.System.DL.IService
{
    public interface ISysPartnerAPIService : IRepositoryBase<SysPartnerApi, SysPartnerAPIModel>
    {
        string GenerateAPIKey();
    }
}
