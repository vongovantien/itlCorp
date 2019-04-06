using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.Domain.Report
{
    public class OCLModel
    {
        public string VesselNo { get; set; }
        public string VoyNo { get; set; }
        public string POL { get; set; }
        public string POD { get; set; }
        public string SLD { get; set; }
        public string SlotOwner { get; set; }
        public string OP { get; set; }
        public string FDestination { get; set; }
    }
}
