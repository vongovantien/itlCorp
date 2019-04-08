using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.Domain.Report
{
    public class ShippingInstructionContainerListTest
    {
        public string ContainerTypesNote { get; set; }
        public string ContainerSealNo { get; set; }
        public string PackagesNote { get; set; }
        public string DesOfGoods { get; set; }
        public decimal GW { get; set; }
        public decimal CBM { get; set; }
        public decimal SumVolume { get; set; }
        public decimal SumGrossWeight { get; set; }
        public string Remark { get; set; }
        public string Payment { get; set; }
    }
}
