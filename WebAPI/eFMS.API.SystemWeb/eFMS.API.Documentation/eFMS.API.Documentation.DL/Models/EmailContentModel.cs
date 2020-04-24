using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class EmailContentModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<string> AttachFiles { get; set; }
    }
}
