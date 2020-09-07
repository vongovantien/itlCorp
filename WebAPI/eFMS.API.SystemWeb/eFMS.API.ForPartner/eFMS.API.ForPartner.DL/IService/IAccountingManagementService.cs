using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Criteria;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IAccountingManagementService: IRepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IForPartnerApiService
    {
        AccAccountingManagementModel GetById(Guid id);

        List<ChargeOfAcctMngtResult> GetChargeInvoice(SearchAccMngtCriteria dataSearch);

    }
}
