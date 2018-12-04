using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.Service.Models
{
    public class ChangeLog
    {
        public Guid Id { get; set; }
        public string PrimaryKeyValue { get; set; }
        public EntityState ActionType { get; set; }
        public List<PropertyChange> PropertyChanges { get; set; }
        public DateTime DatetimeModified { get; set; }
        public string UserModified { get; set; }
        public CatCurrencyEntity NewObject { get; set; }
    }
    public class PropertyChange
    {
        public string PropertyName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
    public class CatCurrencyEntity
    {
        public string Id { get; set; }
        public string CurrencyName { get; set; }
        public bool IsDefault { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
