using System;
using System.Collections.Generic;
using System.Text;

namespace ITL.NetCore.Common.Items
{
    public class AuditLog
    {
        public string EntityName { get; set; }
        public ItemLog ChangeLog { get; set; }
    }
    public class ItemLog
    {
        public Guid Id { get; set; }
        public PropertyCommon PropertyCommon { get; set; }
        public object NewObject { get; set; }
    }
    public class PropertyCommon
    {
        public string PrimaryKeyValue { get; set; }
        public object ActionType { get; set; }
        public PropertyChange PropertyChange { get; set; }
        public DateTime DatetimeModified { get; set; }
        public string UserModified { get; set; }
    }
    public class PropertyChange
    {
        public string PropertyName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
