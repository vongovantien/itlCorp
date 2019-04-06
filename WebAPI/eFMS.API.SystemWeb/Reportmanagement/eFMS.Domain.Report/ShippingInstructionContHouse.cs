using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.Domain.Report
{
    public class ShippingInstructionContHouse
    {
        public string ContainerSealNo { get; set; }
        public string PackagesNote { get; set; }
        public string GoodsDescription { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal Volume { get; set; }
    }
    public class ContainerObject
    {
        public int Quantity { get; set; }
        public string Name { get; set; }
    }
}
