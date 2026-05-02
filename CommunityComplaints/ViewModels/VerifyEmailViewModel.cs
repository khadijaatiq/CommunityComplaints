using System.ComponentModel.DataAnnotations;
using CommunityComplaints.Utilities;

namespace CommunityComplaints.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [StrictEmail(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
    }
}