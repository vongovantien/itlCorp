﻿using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Setting.Resources;

namespace eFMS.API.Setting.Infrastructure.AttributeEx
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
