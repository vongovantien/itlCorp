using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysSentEmailHistory
    {
        public int Id { get; set; }
        public string SentUser { get; set; }
        public string Receivers { get; set; }
        public string Ccs { get; set; }
        public string Bccs { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public bool? Sent { get; set; }
        public DateTime? SentDateTime { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
    }
}
