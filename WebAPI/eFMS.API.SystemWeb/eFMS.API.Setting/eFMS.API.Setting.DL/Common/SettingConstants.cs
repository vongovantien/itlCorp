using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Common
{
    public static class SettingConstants
    {
        #region -- STATUS APPROVAL --
        public static readonly string STATUS_APPROVAL_NEW = "New";
        public static readonly string STATUS_APPROVAL_DENIED = "Denied";
        public static readonly string STATUS_APPROVAL_DONE = "Done";
        public static readonly string STATUS_APPROVAL_LEADERAPPROVED = "Leader Approved";
        public static readonly string STATUS_APPROVAL_MANAGERAPPROVED = "Manager Approved";
        public static readonly string STATUS_APPROVAL_ACCOUNTANTAPPRVOVED = "Accountant Approved";
        public static readonly string STATUS_APPROVAL_REQUESTAPPROVAL = "Request Approval";
        #endregion -- STATUS APPROVAL --

        public static readonly short SpecialGroup = 11;
        public static readonly string PositionManager = "Manager-Leader";
        public static readonly string DeptTypeAccountant = "ACCOUNTANT";

        public static readonly string ROLE_NONE = "None";
        public static readonly string ROLE_APPROVAL = "Approval";
        public static readonly string ROLE_AUTO = "Auto";
        public static readonly string ROLE_SPECIAL = "Special";

        public static readonly string LEVEL_LEADER = "Leader";
        public static readonly string LEVEL_MANAGER = "Manager";
        public static readonly string LEVEL_ACCOUNTANT = "Accountant";
        public static readonly string LEVEL_BOD = "BOD";
        
    }
}
