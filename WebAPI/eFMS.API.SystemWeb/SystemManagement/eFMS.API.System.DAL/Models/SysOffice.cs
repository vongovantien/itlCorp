using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysOffice
    {
        public Guid Id { get; set; }
        public string BranchNameVn { get; set; }
        public string BranchNameEn { get; set; }
        public string ShortName { get; set; }
        public Guid Buid { get; set; }
        public string ManagerId { get; set; }
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
        public string BankAccountNameEn { get; set; }
        public string BankAccountNameVn { get; set; }
        public string BankAddressLocal { get; set; }
        public string BankAddressEn { get; set; }
        public string BankNameEn { get; set; }
        public string BankNameLocal { get; set; }
        public string Location { get; set; }
        public bool Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public byte[] Logo { get; set; }
        public string SwiftCode { get; set; }
        public string Code { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
