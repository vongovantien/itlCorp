using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Accounting
{
    public class BravoAttachDoc
    {
        public string AttachDocRowId { get; set; }
        public DateTime? AttachDocDate { get; set; }
        public string AttachDocName { get; set; }
        public string AttachDocPath { get; set; }
    }
}
