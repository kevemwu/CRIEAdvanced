using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CRIEAdvanced.ViewModels
{
    public class ValidateUser
    {
        [Display(Name = "Account")]
        [Required(ErrorMessage = "Account is empty")]
        [StringLength(20, MinimumLength = 5)]
        public String Account { get; set; } = null!;

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is empty")]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; } = null!;
    }

    public class UsersToken
    {
        public string BearerToken { get; set; } = null!;
    }

    public class TempInt
    {
        public int ReturnValues { get; set; }
    }

    public class TempString
    {
        public String Value { get; set; } = null!;
    }

    public class ValidateLinguisticFeaturesAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || value == DBNull.Value)
                return new ValidationResult("Value cannot be null.");

            if (value is List<string?> linguisticFeatures)
            {
                if (linguisticFeatures.Any(string.IsNullOrWhiteSpace))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success!;
        }
    }

    public class ValidateTextAttribute : ValidationAttribute
    {
        private readonly int _minLength;
        private readonly int _maxLength;

        public ValidateTextAttribute(int minLength, int maxLength)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || value == DBNull.Value)
                return new ValidationResult("Value cannot be null.");

            MatchCollection matches = Regex.Matches(value.ToString()!, @"[\u4e00-\u9fa5]");
            if (matches.Any())
            {
                int length = matches.Count;

                if (length < _minLength || length > _maxLength)
                {
                    return new ValidationResult($"The number of characters in the Chinese content must be between {_minLength} and {_maxLength}.");
                }
            }
            else
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success!;
        }
    }

    public class Tokens
    {
        public String AccessToken { get; set; } = null!;
        public String RefreshToken { get; set; } = null!;
    }
}
