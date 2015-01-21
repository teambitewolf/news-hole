using System.ComponentModel.DataAnnotations;

namespace NewsHole.Web.Models.Account
{
    public class NewPasswordModel
    {
        public string Token { get; set; }

        [MinLength(6)]
        public string NewPassword { get; set; }

        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}