using System.ComponentModel.DataAnnotations;

namespace CommunityComplaints.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string UnitNumber { get; set; }
        public string Phone { get; set; }

        [Required]
        public string Role { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}