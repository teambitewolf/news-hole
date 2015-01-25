using System.ComponentModel.DataAnnotations;

namespace NewsHole.Web.Models.Account
{
    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}