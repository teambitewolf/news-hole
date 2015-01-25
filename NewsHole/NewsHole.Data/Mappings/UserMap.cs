using FluentNHibernate.Mapping;
using NewsHole.Data.Entities;

namespace NewsHole.Data.Mappings
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.Id);
            Map(x => x.Email).Unique();
            Map(x => x.FirstName);
            Map(x => x.LastName);
            Map(x => x.PasswordHash);
        }
    }
}
