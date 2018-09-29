using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatDriver
    {
        public CatDriver()
        {
            CatVehicleDriver = new HashSet<CatVehicleDriver>();
        }

        public int Id { get; set; }
        public Guid WorkPlaceId { get; set; }
        public string DriverNameVn { get; set; }
        public string DriverNameEn { get; set; }
        public string IdentityCard { get; set; }
        public string NricissuedBy { get; set; }
        public DateTime? NricissuedOn { get; set; }
        public string AddressVn { get; set; }
        public string AddressEn { get; set; }
        public Guid? ProvinceId { get; set; }
        public short? CountryId { get; set; }
        public string LivingPlace { get; set; }
        public string Dln { get; set; }
        public DateTime? DlnexpiryDate { get; set; }
        public string Tel { get; set; }
        public string WorkingPhone { get; set; }
        public string RelativeTel { get; set; }
        public string EmployeeIdnumber { get; set; }
        public string Team { get; set; }
        public string TeamPosition { get; set; }
        public string Note { get; set; }
        public string BankAccountNumber { get; set; }
        public string Bank { get; set; }
        public string BankAddress { get; set; }
        public string PaymentBeneficiary { get; set; }
        public byte[] Signature { get; set; }
        public string EmployeeNumber { get; set; }
        public string Password { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public ICollection<CatVehicleDriver> CatVehicleDriver { get; set; }
    }
}
