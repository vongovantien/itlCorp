using eFMS.API.Accounting.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IEdocService
    {
        EdocAccUpdateModel MapAdvanceRequest(AcctAdvancePaymentModel model);
    }
}
