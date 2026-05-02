using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CommunityComplaints.Utilities
{
    public class StrictEmailAttribute : ValidationAttribute
    {
        /*
         Rules:
         - Username before @ must be at least 3 characters
         - Domain name must be at least 2 characters
         - TLD must be at least 2 letters
        */

        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+\-]{3,}@[a-zA-Z0-9\-]{2,}\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(
                    ErrorMessage ?? "Email is required.");
            }

            string email = value.ToString()!.Trim();

            if (!EmailRegex.IsMatch(email))
            {
                return new ValidationResult(
                    ErrorMessage ?? "Please enter a valid email address.");
            }

            return ValidationResult.Success;
        }
    }
}