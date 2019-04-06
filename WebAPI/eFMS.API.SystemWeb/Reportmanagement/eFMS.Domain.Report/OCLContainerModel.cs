using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.Domain.Report
{
    public class OCLContainerModel
    {
        public int STT { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
        public string ST { get; set; }
        public string SZTY { get; set; }
        public decimal GW { get; set; }
        public decimal CBM { get; set; }

    }
}
