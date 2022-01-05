using eFMS.API.SystemFileManagement.DL.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.AccountingPayment
{
    public class ExtendDateUpdatedModel
    {
        public string RefId { get; set; }
        [Required]
        public int NumberDaysExtend { get; set; }
        public string Note { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}
