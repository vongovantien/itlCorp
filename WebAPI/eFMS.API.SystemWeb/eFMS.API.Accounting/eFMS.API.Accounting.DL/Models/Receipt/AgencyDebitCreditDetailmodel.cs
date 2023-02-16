using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class AgencyDebitCreditDetailModel
    {
        public IQueryable<GroupShimentAgencyModel> GroupShipmentsAgency { get; set; }
        public List<AgencyDebitCreditModel> Invoices { get; set; }

    }
}
