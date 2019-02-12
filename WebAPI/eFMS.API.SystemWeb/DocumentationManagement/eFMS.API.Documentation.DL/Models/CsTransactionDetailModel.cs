﻿using eFMS.API.Documentation.Service.Models;
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
        public string NotifyParty { get; set; }
        public string PODName { get; set; }
        public string ContainerNames { get; set; }
        public string PackageTypes { get; set; }
        public decimal? CBM { get; set; }
    }
}
