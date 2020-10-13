using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Accounting
{
    public class RequestGuidListModel
    {
        public Guid Id { get; set; }
        public ACTION Action { get; set; }
    }

    public class RequestIntListModel
    {
        public int Id { get; set; }
        public ACTION Action { get; set; }
    }

    public enum ACTION
    {
        ADD,
        UPDATE
    }
}
