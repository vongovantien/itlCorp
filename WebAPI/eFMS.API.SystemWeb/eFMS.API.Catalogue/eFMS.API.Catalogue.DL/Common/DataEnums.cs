using eFMS.API.Catalogue.DL.Models;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Common
{
    public static class DataEnums
    {
        public static readonly string EnActive = "Active";
        public static readonly string EnInActive = "In Active";
        public static readonly string VnActive = "Hoạt động";
        public static readonly string VnInActive = "Ngưng hoạt động";
        public static readonly string CustomerPartner = "CUSTOMER";
        public static readonly string AgenPartner = "AGENT";
        public static readonly string CarrierPartner = "CARRIER";
        public static readonly string ConsigneePartner = "CONSIGNEE";
        public static readonly string ShipperPartner = "SHIPPER";
        public static readonly string StaffPartner = "STAFF";
        public static readonly string PersonalPartner = "PERSONAL";
        public static readonly string AllPartner = "ALL";
        public static readonly string PARTNER_GROUP = "CARRIER;CONSIGNEE;SHIPPER;STAFF;PERSONAL";

        public static List<ModeOfTransport> ModeOfTransportData = new List<ModeOfTransport>
        {
            new ModeOfTransport { Id = "AIR", Name = "AIR"},
            new ModeOfTransport { Id = "SEA", Name = "SEA"},
            new ModeOfTransport { Id = "INLAND", Name = "INLAND" },
            new ModeOfTransport { Id = "AIR - SEA", Name= "AIR - SEA" },
            new ModeOfTransport { Id = "INLAND - SEA", Name = "INLAND - SEA" },
            new ModeOfTransport { Id = "AIR - INLAND", Name = "AIR - INLAND" },
            new ModeOfTransport { Id = "INLAND - AIR - SEA", Name = "INLAND - AIR - SEA" }
        };

        public static List<DepartmentPartner> Departments = new List<DepartmentPartner> {
            new DepartmentPartner { Id = "Head Office", Code = "00", Name = "Head Office" },
            new DepartmentPartner { Id = "OPS", Code = "01", Name = "OPS" },
            new DepartmentPartner { Id = "HR", Code = "02", Name = "HR" },
            new DepartmentPartner { Id = "Sale", Code = "03", Name = "Sale" },
            new DepartmentPartner { Id = "Admin", Code= "04", Name = "Admin" },
            new DepartmentPartner { Id = "Accounting", Code="05", Name = "Accounting" },
            new DepartmentPartner { Id = "CS", Code="06", Name = "CS" }
        };

        public static List<CatPartnerGroupModel> CatPartnerGroups = new List<CatPartnerGroupModel>
        {
            // new CatPartnerGroupModel { Id = AgenPartner, GroupNameVn = "Agent", GroupNameEn ="Agent" },
            //new CatPartnerGroupModel { Id = "AIRSHIPSUP", GroupNameVn = "AIRSHIPSUP", GroupNameEn = "Air Ship Sub" },
            new CatPartnerGroupModel { Id = CarrierPartner, GroupNameVn = "Người vận chuyển", GroupNameEn = "Carrier"},
            new CatPartnerGroupModel { Id = ConsigneePartner, GroupNameVn = "Người nhận hàng", GroupNameEn = "Consignee" },
            // new CatPartnerGroupModel { Id = CustomerPartner, GroupNameVn = "Khách hàng", GroupNameEn = "Customer" },
            new CatPartnerGroupModel { Id = ShipperPartner, GroupNameVn = "Người gửi", GroupNameEn = "Shipper" },
            new CatPartnerGroupModel { Id = StaffPartner, GroupNameVn = "Nhân viên", GroupNameEn = "Staff" },
            new CatPartnerGroupModel { Id = PersonalPartner, GroupNameVn = "Cá nhân", GroupNameEn = "Personal" },
            new CatPartnerGroupModel { Id = AllPartner, GroupNameVn = "All", GroupNameEn = "All" }
        };
        public static List<UnitType> UnitTypes = new List<UnitType>
        {
            new UnitType { Value = "Container", DisplayName = "Container" },
            new UnitType { Value = "Package", DisplayName = "Package" },
            new UnitType { Value = "WeightMeasurement", DisplayName = "Weight Measurement" },
            new UnitType { Value = "LengthMeasurement", DisplayName = "Length Measurement" },
            new UnitType { Value = "VolumnMeasurement", DisplayName = "Volumn Measurement" }
        };
    }
}
