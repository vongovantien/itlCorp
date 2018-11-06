using eFMS.API.Catalogue.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Common
{
    public static class DataEnums
    {
        public static List<ModeOfTransport> ModeOfTransportData = new List<ModeOfTransport>
        {
            new ModeOfTransport { Id = "AIR", Name = "AIR"},
            new ModeOfTransport { Id = "SEA", Name = "SEA"},
            new ModeOfTransport { Id = "INLAND", Name = "INLAND" },
            new ModeOfTransport { Id = "SEA&AIR&EXPRESS", Name= "SEA&AIR&EXPRESS" },
            new ModeOfTransport { Id = "SEA&INLAND&EXPRESS", Name = "SEA&INLAND&EXPRESS" },
            new ModeOfTransport { Id = "AIR&INLAND&EXPRESS", Name = "AIR&INLAND&EXPRESS" },
            new ModeOfTransport { Id = "DEPOT", Name = "DEPOT" },
            new ModeOfTransport { Id = "SEA&DEPOT&EXPRESS", Name = "SEA&DEPOT&EXPRESS" },
            new ModeOfTransport { Id = "INLAND&DEPOT&EXPRESS", Name = "INLAND&DEPOT&EXPRESS" },
            new ModeOfTransport { Id = "SEA&AIR&INLAND&DEPOT&EXPRESS", Name = "SEA&AIR&INLAND&DEPOT&EXPRESS" },
            new ModeOfTransport { Id = "EXPRESS", Name = "EXPRESS" },
            new ModeOfTransport { Id = "AIR&EXPRESS", Name = "AIR&EXPRESS" }
        };

        public static List<DepartmentPartner> Departments = new List<DepartmentPartner> {
            new DepartmentPartner { Id = 1, Code = "00", Name = "Head Office" },
            new DepartmentPartner { Id = 2, Code = "01", Name = "OPS" },
            new DepartmentPartner { Id = 3, Code = "02", Name = "HR" },
            new DepartmentPartner { Id = 4, Code = "03", Name = "Sale" },
            new DepartmentPartner { Id = 5, Code= "04", Name = "Admin" },
            new DepartmentPartner { Id = 6, Code="05", Name = "Accounting" },
            new DepartmentPartner { Id = 7, Code="06", Name = "CS" }
        };

        public static List<CatPartnerGroupModel> CatPartnerGroups = new List<CatPartnerGroupModel>
        {
            new CatPartnerGroupModel { Id = "AGENT", GroupNameVn = "Agent", GroupNameEn ="Agent" },
            new CatPartnerGroupModel { Id = "AIRSHIPSUP", GroupNameVn = "AIRSHIPSUP", GroupNameEn = "Air Ship Sub" },
            new CatPartnerGroupModel { Id = "CARRIER", GroupNameVn = "Người vận chuyển", GroupNameEn = "Carrier"},
            new CatPartnerGroupModel { Id = "CONSIGNEE", GroupNameVn = "Người nhận hàng", GroupNameEn = "Consignee" },
            new CatPartnerGroupModel { Id = "CUSTOMER", GroupNameVn = "Khách hàng", GroupNameEn = "Customer" },
            new CatPartnerGroupModel { Id = "SHIPPER", GroupNameVn = "Người gửi", GroupNameEn = "Shipper" },
            new CatPartnerGroupModel { Id = "ALL", GroupNameVn = "All", GroupNameEn = "All" }
        };
    }
}
