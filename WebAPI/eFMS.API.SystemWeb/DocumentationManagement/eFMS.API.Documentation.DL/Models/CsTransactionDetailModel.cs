using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsTransactionDetailModel: CsTransactionDetail
    {
        public List<CsMawbcontainerModel> CsMawbcontainers { get; set; }
        public string CustomerName { get; set; }
        public string SaleManName { get; set; }
        public string CustomerNameVn { get; set; }
        public string SaleManNameVn { get; set; }
        public string ForwardingAgentName { get; set; }
        public string NotifyParty { get; set; }
        public string PODName { get; set; }
        public string ContainerNames { get; set; }
        public string PackageTypes { get; set; }
        public decimal? CBM { get; set; }
        public decimal? CW { get; set; }
        public decimal? GW { get; set; }
        public string Packages { get; set; }
        public string Containers { get; set; }
    }
}
