using System.Collections.Generic;
using Newtonsoft.Json;

namespace SystemManagementAPI.Models.Emploees
{
    public class WhereRequestModel
    {
        public IDictionary<string, string> Criterias { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
