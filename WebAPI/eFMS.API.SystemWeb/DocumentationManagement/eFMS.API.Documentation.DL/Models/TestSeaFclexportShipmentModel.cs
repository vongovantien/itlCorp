using eFMS.API.Documentation.Service.Models;

namespace eFMS.API.Documentation.DL.Models
{
    public class TestSeaFclexportShipmentModel: TestSeaFclexportShipment
    {
        public string ColoaderName { get; set; }
        public decimal CBM { get; set; }
        public decimal GrossWeight { get; set; }
        public string UserCreatedName { get; set; }
    }
}
