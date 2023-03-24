﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class WorkOrderCriteria
    {
        public List<string> ReferenceNos { get; set; }
        public string PartnerId { get; set; }
        public string SalesmanId { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public bool? Active { get; set; }
        public string TransactionType { get; set; }
        public string POL { get; set; }
        public string POD { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string ApprovedStatus { get; set; }
    }
}
