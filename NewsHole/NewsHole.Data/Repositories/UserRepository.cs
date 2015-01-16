using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NewsHole.Data.Entities;
using NHibernate;

namespace NewsHole.Data.Repositories
{
    public interface IUserRepository : IRepository<User, int>
    {
        bool UserExists(string email);
        User GetByEmailAddress(string email);
    }

    public class UserRepository : Repository<User, int>, IUserRepository
    {
        ISession _session;

        public UserRepository(ISession session)
            : base(session)
        {
            _session = session;
        }

        public bool UserExists(string email)
        {
            var results = _session.QueryOver<User>()
                                .Where(u => u.Email == email);

            return results.RowCount() > 0;
        }

        public User GetByEmailAddress(string email)
        {
            var results = _session.QueryOver<User>()
                                .Where(u => u.Email == email).List();

            return results[0];
        }
    }
}
