using System.Collections.Generic;

namespace eFMS.API.Common.Models
{
    public class MessageCardWebHook
    {
        public string Type { get; set; }
        public string ThemeColor { get; set; }
        public string Summary { get; set; }
        public List<MessageCardActivitySection> Sections { get; set; }
    }

    public class MessageCardActivitySection
    {
        public string ActivityTitle { get; set; }
        public string ActivitySubtitle { get; set; }
        public string ActivityImage { get; set; }
        public List<ValueName> Facts { get; set; }
    }
}
