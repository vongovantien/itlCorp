using eFMS.API.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.ForPartner.Infrastructure.Common
{
    public class ValidationError
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; set; }
        public string Message { get; set; }


        public ValidationError(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;

        }
    }

    public class ValidationResultModel : ResultHandle
    {
        public string Message { get; }
        public List<ValidationError> Errors { get; }
        public ValidationResultModel(ModelStateDictionary modelState)
        {
            Message = "Validation";
            Status = false;
            Data = null;
            Errors = modelState.Keys
                    .SelectMany(key => modelState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage)))
                    .ToList();
        }
    }
}
