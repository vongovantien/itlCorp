using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.System.Models
{
    public class SysOfficeEditModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string BranchNameEn { get; set; }
        public string BranchNameVn { get; set; }
        public string ShortName { get; set; }
        public string AddressEn { get; set; }
        public Guid Buid { get; set; }
        public string AddressVn { get; set; }
        public string Taxcode { get; set; }

        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string BankAccountVnd { get; set; }
        public string BankAccountUsd { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        public string BankAccountNameVn { get; set; }
        public string BankAccountNameEn { get; set; }
        public string BankAddressLocal { get; set; }
        public string BankAddressEn { get; set; }
        public byte[] Logo { get; set; }
   
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string SwiftCode { get; set; }
 
        public bool? Active { get; set; }
        public string Location { get; set; }
        public string BankNameEn { get; set; }
        public string BankNameLocal { get; set; }
        public string OfficeType { get; set; }
        public string InternalCode { get; set; }
    }
}
