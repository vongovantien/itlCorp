using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class TermBase
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }
    public class FreightTerm: TermBase { }
    public class ShipmentType: TermBase { }
    public class BillofLadingType : TermBase { }
    public class ServiceType: TermBase { }
    public class TypeOfMove: TermBase { }

}
