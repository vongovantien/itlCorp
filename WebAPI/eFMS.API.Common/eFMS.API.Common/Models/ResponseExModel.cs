using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Common.Models
{
    public class ResponseExModel
    {
        public string Body { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public bool Success { get; set; }
        public string Path { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
