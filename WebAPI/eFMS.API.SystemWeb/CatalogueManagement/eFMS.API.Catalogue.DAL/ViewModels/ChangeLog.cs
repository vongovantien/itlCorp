using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.Service.ViewModels
{
    public class EntityChangeLog
    {
        public string EntityName { get; set; }
        public ChangeLog ChangeLog { get; set; }
    }
    public class ChangeLog
    {
        public Guid Id { get; set; }
        public string PrimaryKeyValue { get; set; }
        public EntityState ActionType { get; set; }
        public List<PropertyChange> PropertyChanges { get; set; }
        public DateTime DatetimeModified { get; set; }
        public string UserModified { get; set; }
        public object NewObject { get; set; }
    }
    public class PropertyChange
    {
        public string PropertyName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
