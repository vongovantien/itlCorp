using eFMS.API.Accounting.Service.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.Service.Models
{
    public class DebitAmountDetail
    {
        public DebitAmountGeneralInfo DebitAmountGeneralInfo { get; set; }
        public List<sp_GetDebitAmountDetailByContract> DebitAmountDetails { get; set; }
    }
}
