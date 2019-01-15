using eFMS.API.Documentation.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public static class TermData
    {
        public static List<FreightTerm> FreightTerms = new List<FreightTerm>
        {
            new FreightTerm { Value = "Collect", DisplayName = "Collect" },
            new FreightTerm { Value = "Prepaid", DisplayName = "Prepaid" }
        };
        public static List<ShipmentType> ShipmentTypes = new List<ShipmentType>
        {
            new ShipmentType { Value = "Freehand", DisplayName = "Freehand" },
            new ShipmentType { Value = "Nominated", DisplayName = "Nominated" }
        };
        public static List<BillofLadingType> BillofLadingTypes = new List<BillofLadingType>
        {
            new BillofLadingType { Value = "Copy", DisplayName = "Copy" },
            new BillofLadingType { Value = "Original", DisplayName = "Original" },
            new BillofLadingType { Value = "Sea Waybill", DisplayName = "Sea Waybill" },
            new BillofLadingType { Value = "Surendered", DisplayName = "Surendered" }
        };
        public static List<ServiceType> ServiceTypes = new List<ServiceType>
        {
            new ServiceType { Value = "FCL/FCL", DisplayName = "FCL/FCL" },
            new ServiceType { Value = "LCL/LCL", DisplayName = "LCL/LCL" },
            new ServiceType { Value = "FCL/LCL", DisplayName = "FCL/LCL" },
            new ServiceType { Value = "CY/CFS", DisplayName = "CY/CFS" },
            new ServiceType { Value = "CY/CY", DisplayName = "CY/CY" },
            new ServiceType { Value = "CFS/CY", DisplayName = "CFS/CY" },
            new ServiceType { Value = "CFS/CFS", DisplayName = "CFS/CFS" }
        };
        public static List<TypeOfMove> TypeOfMoves = new List<TypeOfMove>
        {
            new TypeOfMove { Value = "FCL/FCL-CY/CY", DisplayName = "FCL/FCL-CY/CY" },
            new TypeOfMove { Value = "LCL/LCL-CFS/CFS", DisplayName = "LCL/LCL-CFS/CFS" },
            new TypeOfMove { Value = "LCL/FCL-CFS/CY", DisplayName = "LCL/FCL-CFS/CY" },
            new TypeOfMove { Value = "FCL/LCL-CY/CFS", DisplayName = "FCL/LCL-CY/CFS" }
        };
    }
}
