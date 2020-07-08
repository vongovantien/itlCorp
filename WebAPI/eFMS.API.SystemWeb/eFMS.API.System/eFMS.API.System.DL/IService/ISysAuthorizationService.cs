using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Linq;

namespace eFMS.API.System.DL.IService
{
    public interface ISysAuthorizationService : IRepositoryBase<SysAuthorization, SysAuthorizationModel>
    {
        IQueryable<SysAuthorizationModel> QueryData(SysAuthorizationCriteria criteria);

        IQueryable<SysAuthorizationModel> Paging(SysAuthorizationCriteria criteria, int page, int size, out int rowsCount);

        SysAuthorizationModel GetById(int id);

        SysAuthorizationModel GetAuthorizationById(int id);

        int CheckDetailPermission(int id);

        int CheckDeletePermission(int id);

        HandleState Insert(SysAuthorizationModel model);
    }
}
