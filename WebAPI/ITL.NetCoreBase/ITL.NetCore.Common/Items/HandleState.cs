using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Net;

namespace ITL.NetCore.Common
{
    public class HandleState
    {
        public bool Success { get; private set; }
        public int Code { get; private set; }
        public Exception Exception { get; private set; }
        public object Message { get; private set; }

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
        public HandleState(bool isSuccess, string errorMessage)
        {
            Success = isSuccess;
            Exception = new Exception(errorMessage);
        }
        public HandleState(bool isSuccess, object message)
        {
            Success = isSuccess;
            Message = message;
        }

        public HandleState(bool isSuccess, LocalizedString message) {
            Success = isSuccess;
            Message = message;
        }

        public HandleState(LocalizedString message)
        {
            Success = false;
            Message = message;
        }
        public HandleState(object message)
        {
            Success = false;
            Message = message;
        }
    }    
}
