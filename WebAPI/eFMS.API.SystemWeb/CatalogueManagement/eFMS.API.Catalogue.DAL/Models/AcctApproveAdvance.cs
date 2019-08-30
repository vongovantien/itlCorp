﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class AcctApproveAdvance
    {
        public Guid Id { get; set; }
        public string AdvanceNo { get; set; }
        public string Requester { get; set; }
        public DateTime? RequesterAprDate { get; set; }
        public string Leader { get; set; }
        public DateTime? LeaderAprDate { get; set; }
        public string Manager { get; set; }
        public DateTime? ManagerAprDate { get; set; }
        public string Accountant { get; set; }
        public DateTime? AccountantAprDate { get; set; }
        public string Buhead { get; set; }
        public DateTime? BuheadAprDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
