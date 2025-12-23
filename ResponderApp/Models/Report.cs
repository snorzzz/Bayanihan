using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ResponderApp.Models
{
    [Table("reports")]
    public class Report : BaseModel
    {
        [PrimaryKey("report_id", false)]
        [Column("report_id")]
        public string ReportId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }
}
