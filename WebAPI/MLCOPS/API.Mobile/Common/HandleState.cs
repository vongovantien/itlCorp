using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Mobile.Common
{
    public class HandleState
    {
        public bool Success { get; private set; }
        public int Code { get; private set; }
        public Exception Exception { get; private set; }

        public HandleState()
        {
            Code = 200;
            Success = true;
        }

        public HandleState(string errMessage)
        {
            Code = 400;
            Success = false;
            Exception = new Exception(errMessage);
        }

        public HandleState(Exception exception)
        {
            Code = 500;
            Success = false;
            Exception = exception;
        }

        public HandleState(int code, string errorMessage)
        {
            Code = code;
            Success = false;
            Exception = new Exception(errorMessage);
        }
    }
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
