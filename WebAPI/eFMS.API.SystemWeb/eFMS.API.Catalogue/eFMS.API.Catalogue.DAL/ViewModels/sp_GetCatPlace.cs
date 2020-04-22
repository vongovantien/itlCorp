using System;

namespace eFMS.API.Catalogue.Service.ViewModels
{
    public class sp_GetCatPlace
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public Nullable<Guid> DistrictID { get; set; }
        public string DistrictNameEN { get; set; }
        public string DistrictNameVN { get; set; }
        public Nullable<Guid> ProvinceID { get; set; }
        public string ProvinceNameEN { get; set; }
        public string ProvinceNameVN { get; set; }
        public Nullable<short> CountryID { get; set; }
        public string AreaID { get; set; }
        public string LocalAreaID { get; set; }
        public string LocalAreaNameEN { get; set; }
        public string LocalAreaNameVN { get; set; }
        public string ModeOfTransport { get; set; }
        public string GeoCode { get; set; }
        public string PlaceTypeID { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public Nullable<DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> DatetimeModified { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<DateTime> InActiveOn { get; set; }
        public string CountryNameVN { get; set; }
        public string CountryNameEN { get; set; }
        public string AreaNameVN { get; set; }
        public string AreaNameEN { get; set; }
        public string FlightVesselNo { get; set; }
        public Guid? WarehouseId { get; set; }
        public string WarehouseNameEn { get; set; }
        public string WarehouseNameVn { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
