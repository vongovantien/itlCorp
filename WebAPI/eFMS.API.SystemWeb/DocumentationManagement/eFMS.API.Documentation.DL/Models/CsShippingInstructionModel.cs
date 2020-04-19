using eFMS.API.Documentation.Service.Models;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsShippingInstructionModel: CsShippingInstruction
    {
        public string IssuedUserName { get; set; }
        public string SupplierName { get; set; }
        public string ConsigneeName { get; set; }
        public string ActualConsigneeName { get; set; }
        public string ActualShipperName { get; set; }
        public string PolName { get; set; }
        public string PodName { get; set; }
    }
}
