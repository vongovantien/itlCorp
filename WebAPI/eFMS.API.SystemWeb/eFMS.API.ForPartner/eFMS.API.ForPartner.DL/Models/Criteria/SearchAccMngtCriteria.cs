using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.Models.Criteria
{
    public class SearchAccMngtCriteria
    {
        public List<string> CdNotes { get; set; }
        public List<string> SoaNos { get; set; }
        public List<string> JobNos { get; set; }
        public List<string> Hbls { get; set; }
        public List<string> Mbls { get; set; }
        public List<string> PartnerID { get; set; }

    }
}
