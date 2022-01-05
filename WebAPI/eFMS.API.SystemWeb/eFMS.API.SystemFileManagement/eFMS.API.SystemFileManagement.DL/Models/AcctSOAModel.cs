using eFMS.API.SystemFileManagement.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class AcctSoaModel : AcctSoa
    {
        public List<Surcharge> Surcharges { get; set; }
    }

    public class RejectSoaModel
    {
        public string Id { get; set; }
        public string Reason { get; set; }
    }
}
