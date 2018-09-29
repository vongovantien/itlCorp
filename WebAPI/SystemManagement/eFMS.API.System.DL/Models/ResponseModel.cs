using Newtonsoft.Json;

namespace Turnmeup.DL.Models
{
    public class ResponseModel
    {

        public int Code { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public bool Success { get; set; }

        // other fields

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}