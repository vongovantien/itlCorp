﻿using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysBranch
    {
        public SysBranch()
        {
            SysUserOtherWorkPlace = new HashSet<SysUserOtherWorkPlace>();
        }

        public Guid Id { get; set; }
        public string BranchNameVn { get; set; }
        public string BranchNameEn { get; set; }
        public string AddressVn { get; set; }
        public string AddressEn { get; set; }
        public short? CountryId { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Taxcode { get; set; }
        public string BankAccountVnd { get; set; }
        public string BankAccountUsd { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        public bool Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public byte[] Logo { get; set; }
        public string Code { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }

        public virtual ICollection<SysUserOtherWorkPlace> SysUserOtherWorkPlace { get; set; }
    }
}
