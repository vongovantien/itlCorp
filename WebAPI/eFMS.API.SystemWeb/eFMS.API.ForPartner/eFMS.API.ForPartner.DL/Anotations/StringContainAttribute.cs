using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;

namespace eFMS.API.ForPartner.DL.Anotations
{
    public class StringContainAttribute: ValidationAttribute
    {
        public string[] AllowableValues { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isExisted = false;

            AllowableValues.ToList().ForEach(i =>
            {
                if (value?.ToString().ToLower().Contains(i.ToLower()) == true)
                {
                    isExisted = true;
                }
            });
            if (isExisted)
            {
                return ValidationResult.Success;
            }
            var msg = $"Please enter value contain: {string.Join(", ", (AllowableValues ?? new string[] { "Not allowable values found" }))}.";

            return new ValidationResult(msg); ;
        }
    }
}
