using eFMS.API.Accounting.Service.Models;
using System;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctAdvanceRequestModel : AcctAdvanceRequest
    {
        public DateTime? RequestDate { get; set; }
    }
}
