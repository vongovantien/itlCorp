﻿using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatPlaceCriteria
    {
        public string All { get; set; }
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? ProvinceId { get; set; }
        public short? CountryId { get; set; }
        public string AreaId { get; set; }
        public string AreaNameEN { get; set; }
        public string AreaNameVN { get; set; }
        public string LocalAreaId { get; set; }
        public string ModeOfTransport { get; set; }
        public string GeoCode { get; set; }
        public CatPlaceTypeEnum PlaceType { get; set; }
        public string CountryNameVN { get; set; }
        public string CountryNameEN { get; set; }
        public string DistrictNameEN { get; set; }
        public string DistrictNameVN { get; set; }
        public string ProvinceNameEN { get; set; }
        public string ProvinceNAmeVN { get; set; }
        public bool? Active { get; set; }
        public string FlightVesselNo { get; set; }
    }
}
