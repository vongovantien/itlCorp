using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace eFMS.API.ForPartner.DL.Anotations
{
    public class StringContainAttribute : ValidationAttribute
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
            var msg = $"Vui lòng nhập giá trị chứa: {string.Join(", ", (AllowableValues ?? new string[] { "Không tìm thấy giá trị cho phép" }))}.";

            return new ValidationResult(msg); ;
        }
    }
}
