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
            new DepartmentPartner { Id = 1, Name = "Accountant" },
            new DepartmentPartner { Id = 2, Name = "CS" },
            new DepartmentPartner { Id = 3, Name = "ICT" },
            new DepartmentPartner { Id = 4, Name = "OPS" },
            new DepartmentPartner { Id = 5, Name = "Sale" },
            new DepartmentPartner { Id = 6, Name = "Maintenance" }
        };
    }
}
