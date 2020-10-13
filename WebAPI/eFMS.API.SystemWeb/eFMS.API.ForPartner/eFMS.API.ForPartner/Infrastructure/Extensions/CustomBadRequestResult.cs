using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace eFMS.API.ForPartner.Infrastructure.Extensions
{
    public class CustomBadRequestResult : JsonResult
    {
        public CustomBadRequestResult(string message)
       : base(new CustomError(message))
        {
            StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
