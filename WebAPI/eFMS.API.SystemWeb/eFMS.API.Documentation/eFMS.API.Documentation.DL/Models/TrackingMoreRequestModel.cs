using Newtonsoft.Json;

namespace eFMS.API.Documentation.DL.Models
{
    public class TrackingMoreRequestModel
    {
        [JsonProperty("awb_number")]
        public string AwbNumber { get; set; }
    }
}
