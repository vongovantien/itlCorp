using eFMS.API.Documentation.Service.Models;

namespace eFMS.API.Documentation.DL.Models
{
    public class CustomsDeclarationModel: CustomsDeclaration
    {
        public string CustomerName { get; set; }
        public string ImportCountryName { get; set; }
        public string ExportCountryName { get; set; }
        public string GatewayName { get; set; }
        public bool IsReplicate { get; set; }
    }
}
