﻿using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using System.Linq;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccountingPrePaidPaymentService : IRepositoryBase<AcctCdnote, AcctCdNoteModel>
    {
        IQueryable<AccPrePaidPaymentResult> Paging(AccountingPrePaidPaymentCriteria criteria, int page, int size, out int rowsCount);
    }

}
