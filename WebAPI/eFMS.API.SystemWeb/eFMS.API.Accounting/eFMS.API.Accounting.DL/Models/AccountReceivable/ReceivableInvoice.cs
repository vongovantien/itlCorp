using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountReceivable
{
    public class ReceivableInvoice
    {
        public string PartnerId { get; set; }
        public Guid? Office { get; set; }
        public string Service { get; set; }
        public AccAccountingManagement Invoice { get; set; }
    }

    public class ReceivableInvoices
    {
        public string PartnerId { get; set; }
        public Guid? Office { get; set; }
        public string Service { get; set; }
        public List<AccAccountingManagement> Invoices { get; set; }
    }
}
