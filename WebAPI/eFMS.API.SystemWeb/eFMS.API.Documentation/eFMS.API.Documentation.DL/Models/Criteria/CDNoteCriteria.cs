﻿using System;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CDNoteCriteria
    {
        public string ReferenceNos { get; set; }
        public string PartnerId { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string CreatorId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }

        public DateTime? FromExportDate { get; set; }
        public DateTime? ToExportDate { get; set; }
        public DateTime? FromAccountingDate { get; set; }
        public DateTime? ToAccountingDate { get; set; }
    }
}
