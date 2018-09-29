using System;
using System.Collections.Generic;
using System.Text;

namespace SystemManagement.DL.Models.Views
{
    public class vw_catBranch
    {
        public Guid BranchID { get; set; }
        public Int32 IncreasingID { get; set; }
        public Guid HubID { get; set; }
        public string Code { get; set; }
        public string Name_VN { get; set; }
        public string Name_EN { get; set; }
        public string HubCode { get; set; }
        public string HubName_VN { get; set; }
        public string HubName_EN { get; set; }
        public string BranchHubName_VN { get; set; }
        public string BranchHubName_EN { get; set; }
        public string Address { get; set; }
        public Guid DistrictID { get; set; }
        public Guid ProvinceID { get; set; }
        public Int16 CountryID { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public bool IsHub { get; set; }
        public bool IsVirtualBranch { get; set; }
        public string Note { get; set; }
        public string HotLine { get; set; }
        public string Website { get; set; }
        public string Tel { get; set; }
        public string BillingAddress { get; set; }
        public string PublicName_VN { get; set; }
        public string PublicName_EN { get; set; }
        public string TaxCode { get; set; }
        public string BankAccount_VND { get; set; }
        public string BankAccount_USD { get; set; }
        public string Bank { get; set; }
        public string BankAddress { get; set; }
        public string SwiftCode { get; set; }
        public string UserCreated { get; set; }
        public DateTime DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime DatetimeModified { get; set; }
        public bool Inactive { get; set; }
        public bool InactiveOn { get; set; }
    }
}
