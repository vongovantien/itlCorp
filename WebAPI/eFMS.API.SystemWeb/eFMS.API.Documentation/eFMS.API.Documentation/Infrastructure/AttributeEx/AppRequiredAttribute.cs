﻿using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.Shipment.Infrastructure.AttributeEx
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class AppRequiredAttribute : RequiredAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var stringLocalizer = validationContext.GetService(typeof(IStringLocalizer<LanguageSub>)) as IStringLocalizer<LanguageSub>;
            this.ErrorMessage = stringLocalizer[LanguageSub.EF_ANNOTATIONS_REQUIRED]?.Value;
            //ErrorMessage = stringLocalizer["The {0} field is required."]?.Value ?? "The {0} field is required.";

            return base.IsValid(value, validationContext);
        }
    }
}
