using System;
using System.Collections.Generic;
using System.Text;

namespace SystemManagement.DL.Models
{
    public class CustomerBookingInfoModel
    {
        public List<CatCustomerGoodsDescriptionModel> catCustomerGoodsDescriptionModels { get; set; }
        public List<CatCustomerPlaceModel> catCustomerPlaceModels { get; set; }
        public List<CatCustomerShipmentNoteModel> catCustomerShipmentNoteModels { get; set; }
    }
}
