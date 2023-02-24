using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class ApiResponse
    {
        public Dictionary<string, object> Meta { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}
