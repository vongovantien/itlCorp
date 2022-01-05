using System;

namespace eFMS.API.SystemFileManagement.DL.Models.Accounting
{
    public class RequestGuidListModel
    {
        public Guid Id { get; set; }
        public ACTION Action { get; set; }
    }

    public class RequestStringListModel
    {
        public string Id { get; set; }
        public ACTION Action { get; set; }
    }

    public class RequestGuidTypeListModel
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public ACTION Action { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class RequestStringTypeListModel
    {
        public string Id { get; set; }
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
