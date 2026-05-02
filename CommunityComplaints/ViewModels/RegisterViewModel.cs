using CommunityComplaints.Utilities;
using System.ComponentModel.DataAnnotations;

namespace CommunityComplaints.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [StrictEmail(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "Unit number cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\-\/]*$", ErrorMessage = "Unit number can only contain letters, numbers, hyphens, or slashes")]
        [Display(Name = "Unit Number")]
        public string? UnitNumber { get; set; }  // FIX 8: nullable — field is optional

        [Phone(ErrorMessage = "Enter a valid phone number")]
        [RegularExpression(@"^\+?[\d\s\-\(\)]{7,20}$", ErrorMessage = "Enter a valid phone number (7-20 digits)")]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }  // FIX 8: nullable — field is optional

        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}