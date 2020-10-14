using eFMS.API.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace eFMS.API.ForPartner.Infrastructure.Extensions
{
    public class CustomUnauthorizedResult : JsonResult
    {
        public CustomUnauthorizedResult(string message) : base(new ResultHandle { Message = message })
        {
            StatusCode = StatusCodes.Status401Unauthorized;
        }

        public CustomUnauthorizedResult(string message, bool status) : base(new ResultHandle { Message = message, Status = status })
        {
            StatusCode = StatusCodes.Status401Unauthorized;
        }

        public CustomUnauthorizedResult(string message, bool status, object data) : base(new ResultHandle { Message = message, Status = status })
        {
            StatusCode = StatusCodes.Status401Unauthorized;
        }

        public CustomUnauthorizedResult(ResultHandle result) : base(new ResultHandle { Message = result.Message, Status = result.Status, Data = result.Data })
        {
            StatusCode = StatusCodes.Status401Unauthorized;
        }
    }

    
}
