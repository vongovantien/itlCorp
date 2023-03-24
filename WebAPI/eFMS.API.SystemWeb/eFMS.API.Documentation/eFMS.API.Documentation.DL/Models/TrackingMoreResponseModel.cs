using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class TrackingMoreResponseModel
    {
        public Meta Meta { get; set; }
        public Data Data { get; set; }
    }
    public class LogTrackingResponseModel
    {
        public string Message { get; set; }
        public int Status { get; set; }
        public ICurrentUser User { get; set;}
        public string Action { get; set; }
        public TrackingMoreRequestModel ObjectRequest { get; set; }
        public TrackingMoreResponseModel ObjectsResponse { get; set; }
    }

    public class LogTrackingMoreResponseModel
    {
        public string Message { get; set; }
        public string Status { get; set; }
    }


    public class Meta
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }

    public class FlightInfo
    {
        [JsonProperty("plan_arrival_time")]
        public string PlanArrivalTime { get; set; }
        [JsonProperty("plan_depart_time")]
        public string PlanDepartTime { get; set; }
        [JsonProperty("arrival_station")]
        public string ArrivalStation { get; set; }
        [JsonProperty("depart_station")]
        public string DepartStation { get; set; }
        [JsonProperty("arrival_time")]
        public string ArrivalTime { get; set; }
        [JsonProperty("depart_time")]
        public string DepartTime { get; set; }
    }

    public class TrackInfo
    {
        [JsonProperty("plan_date")]
        public string PlanDate { get; set; }
        [JsonProperty("actual_date")]
        public string ActualDate { get; set; }
        public string Event { get; set; }
        public string Station { get; set; }
        [JsonProperty("flight_number")]
        public string FlightNumber { get; set; }
        public string Status { get; set; }
        public string Piece { get; set; }
        public string Weight { get; set; }
    }

    public class Data
    {
        [JsonProperty("awb_number")]
        public string AwbNumber { get; set; }
        [JsonProperty("status_number")]
        public int StatusNumber { get; set; }
        public string Weight { get; set; }
        public string Piece { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        [JsonProperty("flight_way_station")]
        public List<string> FlightWayStation { get; set; }
        [JsonProperty("last_event")]
        public string LastEvent { get; set; }
        [JsonProperty("flight_info_new")]
        public List<TrackInfo> FlightInfo { get; set; }
        [JsonProperty("track_info")]
        public List<TrackInfo> TrackInfo { get; set; }
    }
}
