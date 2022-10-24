using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class TransctionTypeJobModel
    {
        public Guid JobId { get; set; }
        public string TransactionType { get; set; }
    }
}
