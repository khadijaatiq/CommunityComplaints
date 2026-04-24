using System.ComponentModel.DataAnnotations;

namespace CommunityComplaints.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}