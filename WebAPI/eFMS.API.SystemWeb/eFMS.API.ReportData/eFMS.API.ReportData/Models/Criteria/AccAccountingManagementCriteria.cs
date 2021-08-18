﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class AccAccountingManagementCriteria
    {
        public string ReferenceNos { get; set; }
        public string PartnerId { get; set; }
        public string CreatorId { get; set; }
        public string InvoiceStatus { get; set; }
        public string VoucherType { get; set; }
        public string TypeOfAcctManagement { get; set; }
        public DateTime? FromIssuedDate { get; set; }
        public DateTime? ToIssuedDate { get; set; }

        public DateTime? FromExportDate { get; set; }
        public DateTime? ToExportDate { get; set; }
        public DateTime? FromAccountingDate { get; set; }
        public DateTime? ToAccountingDate { get; set; }
    }
}
