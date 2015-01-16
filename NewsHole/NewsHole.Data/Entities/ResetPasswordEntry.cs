using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsHole.Data.Entities
{
    public class ResetPasswordEntry
    {
        public virtual string Token { get; set; }
        public virtual DateTime EntryDateTime { get; set; }
        public virtual User User { get; set; }
    }
}
