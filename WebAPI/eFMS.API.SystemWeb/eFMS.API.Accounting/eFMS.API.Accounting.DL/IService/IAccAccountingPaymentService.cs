using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccAccountingPaymentService : IRepositoryBase<AccAccountingPayment, AccAccountingPaymentModel>
    {
        IQueryable<AccAccountingPaymentModel> GetBy(string refNo);
    }
}
