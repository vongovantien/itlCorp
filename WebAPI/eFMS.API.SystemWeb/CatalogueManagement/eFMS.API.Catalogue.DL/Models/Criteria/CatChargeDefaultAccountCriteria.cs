using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatChargeDefaultAccountCriteria
    {
        public string All { get; set; }
        public int Id { get; set; }
        public Guid ChargeId { get; set; }
        public string DebitAccountNo { get; set; }
        public decimal? DebitVat { get; set; }
        public string CreditAccountNo { get; set; }
        public decimal? CreditVat { get; set; }
        public string Type { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? ActiveOn { get; set; }
    }
}
