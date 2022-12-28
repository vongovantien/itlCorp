﻿using eFMS.API.Common.Models;
using eFMS.API.Documentation.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class OpsTransactionModel : OpsTransaction
    {
        public List<CsMawbcontainerModel> CsMawbcontainers { get; set; }
        public string PODName { get; set; }
        public string POLName { get; set; }
        public int CurrentStageId { get; set; }
        public string CurentStageCode { get; set; }
        public string AgentName { get; set; }
        public string SupplierName { get; set; }
        public string ClearanceNo { get; set; }
        public string CustomerName { get; set; }
        public PermissionAllowBase Permission { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        public string ProductService { get; set; }
        public bool IsLinkJob { get; set; }
        public bool IsReplicate { get; set; }
        public string ReplicateJobNo { get; set; }
        public string UserCreatedNameLinkJob { get; set; }
        public bool IsAllowChangeSaleman { get; set; }
        public string SalesmanName { get; set; }
        public string DepartmentName { get; set; }
        public string GroupName { get; set; }
    }
}
