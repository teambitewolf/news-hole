using System.Web.Security;

namespace NewsHole.Web.Infrastructure
{
    public interface IAuthenticationHelper
    {
        void Authenticate(string userName, bool rememberUser);
        void SignOut();
    }

    public class FormsAuthenticationWrapper : IAuthenticationHelper
    {
        public void Authenticate(string userName, bool rememberUser)
        {
            FormsAuthentication.SetAuthCookie(userName, rememberUser);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}