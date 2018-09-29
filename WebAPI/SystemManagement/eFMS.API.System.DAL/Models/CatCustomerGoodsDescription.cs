using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatCustomerGoodsDescription
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Weight { get; set; }
        public int? Sku { get; set; }
    }
}
