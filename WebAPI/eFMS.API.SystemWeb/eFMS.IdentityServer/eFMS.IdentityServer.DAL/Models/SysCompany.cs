using System;
using System.Collections.Generic;

namespace eFMS.IdentityServer.Service.Models
{
    public partial class SysCompany
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string BunameVn { get; set; }
        public string BunameEn { get; set; }
        public string BunameAbbr { get; set; }
        public string DescriptionVn { get; set; }
        public string DescriptionEn { get; set; }
        public string AddressVn { get; set; }
        public string AddressEn { get; set; }
        public string AreaId { get; set; }
        public short? CountryId { get; set; }
        public string ManagerId { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Taxcode { get; set; }
        public string AccountNoVn { get; set; }
        public string AccountNoOverSea { get; set; }
        public string Notes { get; set; }
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        public string LogoPath { get; set; }
        public byte[] Logo { get; set; }
        public string Tax { get; set; }
        public string TaxAccount { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public decimal? KbExchangeRate { get; set; }
    }
}
