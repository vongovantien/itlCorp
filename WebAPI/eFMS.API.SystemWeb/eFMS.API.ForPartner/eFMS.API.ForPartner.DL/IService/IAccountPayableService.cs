using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Payable;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IAccountPayableService: IRepositoryBase<AccAccountPayable, AccountPayableModel>
    {
        Task<HandleState> InsertAccPayable(VoucherSyncCreateModel model);
        bool IsPayableHasPayment(VoucherSyncDeleteModel model);
        string CheckIsValidPayable(List<AccAccountPayableModel> accountPayables);
        HandleState InsertAccountPayablePayment(List<AccAccountPayableModel> accountPayables, string apiKey);
        HandleState CancelAccountPayablePayment(List<CancelPayablePayment> accountPayables, string apiKey);
    }
}
