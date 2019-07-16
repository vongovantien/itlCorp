using System;
using System.Collections.Generic;

namespace eFMS.API.Operation.Service.Models
{
    public partial class CatCustomerPlace
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string Address { get; set; }
        public Guid PlaceId { get; set; }
        public string Note { get; set; }
        public long? OccurrenceFrequency { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNo { get; set; }
    }
}
