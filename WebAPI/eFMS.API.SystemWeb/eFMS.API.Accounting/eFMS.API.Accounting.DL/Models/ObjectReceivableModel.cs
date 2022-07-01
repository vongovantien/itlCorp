using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class ObjectReceivableModel
    {
        public string PartnerId { get; set; }
        public Guid? Office { get; set; }
        public string Service { get; set; }
    }

    public class SurchargeGrpSaleOfficePartner
    {
        public IQueryable<CsShipmentSurcharge> Surcharges { get; set; }
        public string Office { get; set; }
        public string TransactionType { get; set; }
        public string PartnerId { get; set; }
    }

    public class SalesmanSurcharge
    {
        public CsShipmentSurcharge Surcharge { get; set; }
        public string Salesman { get; set; }
        public Guid? Office { get; set; }
        public string TransactionType { get; set; }
        public string PartnerId { get; set; }
    }
}
