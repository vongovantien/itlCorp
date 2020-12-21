using eFMS.API.Accounting.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctSoaModel : AcctSoa
    {
        public List<Surcharge> Surcharges { get; set; }
    }

    public class RejectSoaModel
    {
        public int Id { get; set; }
        public string Reason { get; set; }
    }
}
