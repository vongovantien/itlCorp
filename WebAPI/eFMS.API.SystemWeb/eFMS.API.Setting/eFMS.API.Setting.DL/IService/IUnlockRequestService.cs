using System;
using System.Collections.Generic;
using System.Text;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;

namespace eFMS.API.Setting.DL.IService
{
    public interface IUnlockRequestService : IRepositoryBase<SetUnlockRequest, SetUnlockRequestModel>
    {
        HandleState AddUnlockRequest(SetUnlockRequestModel model);
        HandleState DeleteUnlockRequest(Guid id);
        HandleState UpdateUnlockRequest(SetUnlockRequestModel model);
        List<SetUnlockRequestJobModel> GetJobToUnlockRequest(UnlockJobCriteria criteria);
        List<UnlockRequestResult> Paging(UnlockRequestCriteria criteria, int page, int size, out int rowsCount);
        SetUnlockRequestModel GetDetailUnlockRequest(Guid id);
        HandleState CheckExistVoucherNoOfAdvance(UnlockJobCriteria criteria);
        HandleState CheckExistInvoiceNoOfSettlement(UnlockJobCriteria criteria);
    }
}
