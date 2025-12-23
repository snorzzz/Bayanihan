using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace ResponderApp.Models
{
    [Table("Email")]
    public class Email : BaseModel
    {
        [Column("email")]
        public string email { get; set; }

        [Column("fullname")]
        public string fullname { get; set; }

        [Column("position")]
        public string position { get; set; }
    }
}
