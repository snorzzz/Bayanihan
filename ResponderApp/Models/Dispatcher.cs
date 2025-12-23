using System;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace ResponderApp.Models
{
    [Table("dispatcher")]
    public class Dispatcher : BaseModel
    {
        [PrimaryKey("dispatcherid")]
        public Guid DispatcherID { get; set; }

        [Column("firstname")]
        public string firstname { get; set; }

        [Column("lastname")]
        public string lastname { get; set; }

        [Column("email")]
        public string email { get; set; }

        [Column("createdat")]
        public DateTime createdat { get; set; }
    }
}
