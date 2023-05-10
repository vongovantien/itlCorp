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

    public class AgencyDebitCreditCombineDetailModel
    {
        public string OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string PaymentMethod { get; set; }
        public string ReceiptNo { get; set; }
        public string Description { get; set; }
        public IQueryable<AgencyDebitCreditModel> CDCombineList { get; set; }
    }
}
