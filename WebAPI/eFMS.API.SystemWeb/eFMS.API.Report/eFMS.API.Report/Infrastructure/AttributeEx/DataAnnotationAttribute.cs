using eFMS.API.Common.Globals;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.Report.Infrastructure.AttributeEx
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class AppRequiredAttribute : RequiredAttribute
    {
        public string DisplayName { get; set; }
        public AppRequiredAttribute(string errorMessage, string displayName)
        {
            this.ErrorMessage = errorMessage;
            this.DisplayName = displayName;
        }

        public AppRequiredAttribute() : base()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var stringLocalizer = validationContext.GetService(typeof(IStringLocalizer<LanguageSub>)) as IStringLocalizer<LanguageSub>;
            //this.ErrorMessage = stringLocalizer[AccountingLanguageSub.EF_ANNOTATIONS_REQUIRED]?.Value;
            if (!string.IsNullOrEmpty(this.DisplayName))
            {
                validationContext.DisplayName = stringLocalizer[this.DisplayName]?.Value;
            }
            return base.IsValid(value, validationContext);
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class AppStringLengthAttribute : StringLengthAttribute
    {
        public string DisplayName { get; set; }
        public AppStringLengthAttribute(int maximumLength) : base(maximumLength)
        {
        }
        public AppStringLengthAttribute(string displayName, string errorMessage, int maximumLength, int minimumLength) : base(maximumLength)
        {
            ErrorMessage = errorMessage;
            DisplayName = displayName;
            MinimumLength = minimumLength;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var stringLocalizer = validationContext.GetService(typeof(IStringLocalizer<LanguageSub>)) as IStringLocalizer<LanguageSub>;
            this.ErrorMessage = stringLocalizer[LanguageSub.EF_ANNOTATIONS_STRING_LENGTH]?.Value;
            if (!string.IsNullOrEmpty(this.DisplayName))
            {
                validationContext.DisplayName = stringLocalizer[this.DisplayName]?.Value;
            }
            return base.IsValid(value, validationContext);
        }
    }
}
