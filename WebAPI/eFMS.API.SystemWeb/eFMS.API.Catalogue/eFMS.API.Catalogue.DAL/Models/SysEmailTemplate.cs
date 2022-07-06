using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class SysEmailTemplate
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Footer { get; set; }
        public string Page { get; set; }
        public Guid? UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public Guid? UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string Content { get; set; }
    }
}
