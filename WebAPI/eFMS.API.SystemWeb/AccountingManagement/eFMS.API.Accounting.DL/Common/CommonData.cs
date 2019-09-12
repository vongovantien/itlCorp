using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Common
{
    public class CommonData
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }


    public static class CustomData
    {
        public static readonly List<CommonData> PaymentMethod = new List<CommonData>
        {
            new CommonData { Value = "Cash", DisplayName = "Cash" },
            new CommonData { Value = "Bank", DisplayName = "Bank Transfer" }
        };

        //Define list status approve advance
        public static readonly List<CommonData> StatusApproveAdvance = new List<CommonData>
        {
            new CommonData { Value = "New", DisplayName = "New" },
            //new CommonData { Value = "RequestApproval", DisplayName = "Request Approval" },
            new CommonData { Value = "LeaderApproved", DisplayName = "Leader Approved" },
            new CommonData { Value = "DepartmentManagerApproved", DisplayName = "Department Manager Approved" },
            new CommonData { Value = "AccountantManagerApproved", DisplayName = "Accountant Manager Approved" },
            new CommonData { Value = "Done", DisplayName = "Done" },
            new CommonData { Value = "Denied", DisplayName = "Denied" }
        };

    }
}
