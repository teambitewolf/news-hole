using System;

namespace NewsHole.Data.Entities
{
    public class ResetPasswordEntry
    {
        public virtual string Token { get; set; }
        public virtual DateTime EntryDateTime { get; set; }
        public virtual User User { get; set; }
    }
}
