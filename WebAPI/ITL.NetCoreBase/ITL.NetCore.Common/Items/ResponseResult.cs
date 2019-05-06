using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ITL.NetCore.Common.Items
{
    public class ResponseResult<TClass>
    {

        public HttpStatusCode Status { get; private set; }
        public TClass Value { get; private set; }

        public ResponseResult(TClass value)
        {
            Status = HttpStatusCode.OK;
            Value = value;
        }

        public ResponseResult(HttpStatusCode httpStatus, TClass value)
        {
            Status = httpStatus;
            Value = value;
        }
    }

    public class ResponseResult
    {

        public String Message { get; private set; }
        public HttpStatusCode Status { get; private set; }


        public ResponseResult(String message)
        {
            Status = HttpStatusCode.InternalServerError;
            Message = message;
        }

        public ResponseResult(HttpStatusCode httpStatusCode, string message)
        {
            Status = httpStatusCode;
            Message = message;
        }
    }
}
