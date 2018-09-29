using System;
using System.Collections.Generic;
using System.Text;

namespace SystemManagement.DL.Models.Views
{
    public class vw_catHub
    {
        public Guid HubID { get; set; }

        public int IncreasingID { get; set; }

        public string Code { get; set; }

        public string Name_VN { get; set; }

        public string Name_EN { get; set; }

        public string PlaceTypeID { get; set; }

        public string Note { get; set; }

        public string ContactPerson { get; set; }

        public string Email { get; set; }

        public string ContactNo { get; set; }

        public string Address { get; set; }

        public Guid? DistrictID { get; set; }

        public short BUID { get; set; }

        public string BUCode { get; set; }

        public Guid? ProvinceID { get; set; }

        public short? CountryID { get; set; }

        public string PostalCode { get; set; }

        public string UserCreated { get; set; }

        public DateTime? DatetimeCreated { get; set; }

        public string UserModified { get; set; }

        public DateTime? DatetimeModified { get; set; }

        public bool? Inactive { get; set; }

        public DateTime? InactiveOn { get; set; }

    }

}
