using FluentNHibernate.Mapping;
using NewsHole.Data.Entities;

namespace NewsHole.Data.Mappings
{
    public class ResetPasswordEntryMap : ClassMap<ResetPasswordEntry>
    {
        public ResetPasswordEntryMap()
        {
            Id(x => x.Token);
            Map(x => x.EntryDateTime);
            References(x => x.User, "UserId");
        }
    }
}
