using eFMS.API.Documentation.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class LinkAirSeaInfoModel
    {
        public string HblId { get; set; }
        public string JobId { get; set; }
        public string JobNo { get; set; }
        public decimal? GW { get; set; }
        public decimal? PackageQty { get; set; }
        public decimal? CW { get; set; }

        public List<CsMawbcontainer> Containers { get; set; }
    }
}
