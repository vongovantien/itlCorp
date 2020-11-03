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

    public class RequestGuidTypeListModel
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public ACTION Action { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class RequestIntTypeListModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public ACTION Action { get; set; }
        public string PaymentMethod { get; set; }
    }
    
    public enum ACTION
    {
        ADD,
        UPDATE
    }
}
