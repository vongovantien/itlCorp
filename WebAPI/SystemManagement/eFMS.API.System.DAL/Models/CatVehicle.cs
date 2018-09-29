using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatVehicle
    {
        public CatVehicle()
        {
            CatVehicleDriver = new HashSet<CatVehicleDriver>();
        }

        public int Id { get; set; }
        public string LicensePlate { get; set; }
        public short? VehicleTypeId { get; set; }
        public string Made { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string Color { get; set; }
        public string EngineNumber { get; set; }
        public string FrameNumber { get; set; }
        public string OdometerType { get; set; }
        public byte[] Image { get; set; }
        public string Renewal { get; set; }
        public string Engine { get; set; }
        public string Vendor { get; set; }
        public string Transmission { get; set; }
        public string TireSize { get; set; }
        public string InsurancePartner { get; set; }
        public string InsuranceAccount { get; set; }
        public decimal? InsurancePremium { get; set; }
        public DateTime? InsuranceDue { get; set; }
        public string Note { get; set; }
        public decimal? Gw { get; set; }
        public string Status { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PuchaseCost { get; set; }
        public short? Gpsprovider { get; set; }
        public string Owner { get; set; }
        public bool? Shared { get; set; }
        public Guid BranchId { get; set; }
        public int? MaxContermetNo { get; set; }
        public string RepairStatus { get; set; }
        public int? LastContermetNumber { get; set; }
        public decimal? SubsidizedFuel { get; set; }
        public DateTime? SubsidizedEffectiveDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public SysGpsprovider GpsproviderNavigation { get; set; }
        public CatVehicleType VehicleType { get; set; }
        public ICollection<CatVehicleDriver> CatVehicleDriver { get; set; }
    }
}
