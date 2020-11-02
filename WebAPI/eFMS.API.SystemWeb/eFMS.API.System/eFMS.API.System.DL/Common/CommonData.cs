using System.Collections.Generic;

namespace eFMS.API.System.DL.Common
{
    public class CommonData
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }

    public static class SystemConstants
    {
        public static readonly string Owner = "Owner";
        public static readonly string Password = "12345678";
        public static readonly short SpecialGroup = 11;
        public static readonly string DeptTypeAccountant = "ACCOUNTANT";
        public static readonly string NOTIFICATION_STATUS_NEW = "New";
        public static readonly string NOTIFICATION_STATUS_READ = "Read";
        public static readonly string NOTIFICATION_STATUS_CLOSED = "Closed";
    }
}
