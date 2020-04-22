using Newtonsoft.Json;

namespace eFMS.API.Catalogue.Infrastructure.Common
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
