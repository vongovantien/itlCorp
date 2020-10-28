
using eFMS.API.System.Service.Models;

namespace eFMS.API.System.DL.Models
{
    public class SysUserNotificationModel: SysUserNotification
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public string ActionLink { get; set; }
        public string Title { get; set; }
    }
}
