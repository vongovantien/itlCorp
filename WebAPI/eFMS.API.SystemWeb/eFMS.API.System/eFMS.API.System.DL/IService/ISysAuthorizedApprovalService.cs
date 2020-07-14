using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Linq;

namespace eFMS.API.System.DL.IService
{
    public interface ISysAuthorizedApprovalService : IRepositoryBase<SysAuthorizedApproval, SysAuthorizedApprovalModel>
    {
        IQueryable<SysAuthorizedApprovalModel> Query(SysAuthorizedApprovalCriteria criteria);

        IQueryable<SysAuthorizedApprovalModel> Paging(SysAuthorizedApprovalCriteria criteria, int page, int size, out int rowsCount);


        bool CheckDetailPermission(Guid id);

        bool CheckDeletePermission(Guid id);

        HandleState Update(SysAuthorizedApprovalModel model);
        HandleState Delete(Guid authorId);

    }
}
