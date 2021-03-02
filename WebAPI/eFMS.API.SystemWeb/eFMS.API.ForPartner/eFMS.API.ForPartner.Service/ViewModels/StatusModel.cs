using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.ForPartner.Service.ViewModels
{
    public class StatusModel
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public decimal TotalRowAffected { get; set; }
    }
}
