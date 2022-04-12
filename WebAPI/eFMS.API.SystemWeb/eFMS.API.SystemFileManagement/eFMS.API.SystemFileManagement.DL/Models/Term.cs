using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
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
    public class FreightPayment : TermBase { }
    public class ProductService: TermBase { }
    public class ServiceMode: TermBase { }
    public class ShipmentMode: TermBase { }
}
