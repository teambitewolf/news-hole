using System.ComponentModel.DataAnnotations;

namespace NewsHole.Web.Models.Account
{
    public class CreateUserModel
    {
        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please provide an email address.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please provide your first name.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please provide your last name.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please provide a password.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }
    }
}