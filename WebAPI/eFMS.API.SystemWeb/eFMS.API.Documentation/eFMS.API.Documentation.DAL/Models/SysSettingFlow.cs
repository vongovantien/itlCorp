﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysSettingFlow
    {
        public Guid Id { get; set; }
        public Guid? OfficeId { get; set; }
        public string Type { get; set; }
        public string Flow { get; set; }
        public string Leader { get; set; }
        public string Manager { get; set; }
        public string Accountant { get; set; }
        public string Bod { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? CreditLimit { get; set; }
        public bool? OverPaymentTerm { get; set; }
        public bool? ExpiredAgreement { get; set; }
        public string ApplyType { get; set; }
        public string ApplyPartner { get; set; }
        public Guid? ReplicateOfficeId { get; set; }
        public string ReplicatePrefix { get; set; }
        public bool? IsApplyContract { get; set; }
    }
}
