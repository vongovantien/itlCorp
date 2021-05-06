using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class AgencyDebitCreditDetailModel
    {
        public List<GroupShimentAgencyModel> GroupShipmentsAgency { get; set; }
        public List<AgencyDebitCreditModel> Invoices { get; set; }

    }
}
