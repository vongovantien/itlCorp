using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatStandardChargeCriteria
    {
        public string Type { get; set; }
        public string TransactionType { get; set; }
        public string Service { get; set; }
        public string ServiceType { get; set; }
    }
}
