using DevOne.Security.Cryptography.BCrypt;

namespace NewsHole.Account.Infrastructure
{
    public interface ICrypt
    {
        bool CheckPassword(string plaintext, string hashed);
        string GenerateSalt();
        string GenerateSalt(int logRounds);
        string HashPassword(string password, string salt);
    }

    public class BCryptWrapper : ICrypt
    {
        public bool CheckPassword(string plaintext, string hashed)
        {
            return BCryptHelper.CheckPassword(plaintext, hashed);
        }

        public string GenerateSalt()
        {
            return BCryptHelper.GenerateSalt();
        }

        public string GenerateSalt(int logRounds)
        {
            return BCryptHelper.GenerateSalt(logRounds);
        }

        public string HashPassword(string password, string salt)
        {
            return BCryptHelper.HashPassword(password, salt);
        }
    }
}
