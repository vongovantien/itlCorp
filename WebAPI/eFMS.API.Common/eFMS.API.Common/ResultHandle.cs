using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Common
{
    public class ResultHandle
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
