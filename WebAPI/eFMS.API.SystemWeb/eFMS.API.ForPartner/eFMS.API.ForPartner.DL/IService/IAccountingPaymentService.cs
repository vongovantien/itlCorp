﻿using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.BL;
using System;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IAccountingPaymentService: IRepositoryBase<AccAccountingPayment, AccAccountingPaymentModel>
    {
        bool CheckInvoicePayment(Guid Id);
    }
}
