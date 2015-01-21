using System.ComponentModel.DataAnnotations;

namespace NewsHole.Web.Models.Account
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Please provide your email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please provide your password.")]
        public string Password { get; set; }

        public bool RememberUser { get; set; }
    }
}