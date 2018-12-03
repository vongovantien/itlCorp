using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.Service.Models
{
    public class CatCurrencyExchange
    {
        public Guid Id { get; set; }
        public PropertyCommon PropertyCommon { get; set; }
        public CatCurrencyExchangeEntity NewObject { get; set; }
    }
    public class CatCurrencyExchangeEntity
    {
        public int Id { get; set; }
        public string CurrencyFromId { get; set; }
        public string CurrencyToId { get; set; }
        public decimal Rate { get; set; }
        public DateTime? EffectiveOn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
