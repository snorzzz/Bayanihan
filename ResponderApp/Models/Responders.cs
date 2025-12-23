using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ResponderApp.Models
{
    [Table("responders")]
    public class Responder : BaseModel
    {
        [PrimaryKey("responderID", false)]
        [Column("responderID")]
        public Guid ResponderID { get; set; }

        [Column("responderEmail")]
        public string? ResponderEmail { get; set; }

        [Column("responderLocation")]
        public string? ResponderLocation { get; set; }

        [Column("responderPassword")]
        public string? ResponderPassword { get; set; }

        [Column("responderStatus")]
        public string ResponderStatus { get; set; }

        [Column("responderFirstName")]
        public string? ResponderFirstName { get; set; }

        [Column("responderLastName")]
        public string? ResponderLastName { get; set; }

        [Column("lastFinishedAt")]
        public DateTime lastFinishedAt { get; set; }

        [Column("assignedMission")]
        public Guid? AssignedMission { get; set; }

        [Column("responderLat")]
        public float? ResponderLat { get; set; }

        [Column("responderLng")]
        public float? ResponderLng { get; set; }
    }
}
