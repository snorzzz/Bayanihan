using System;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace ResponderApp.Models
{
    [Table("finishedmissionhistory")]
    public class FinishedMissionHistory : BaseModel
    {
        [PrimaryKey("finishedmissionid")]
        public Guid finishedmissionid { get; set; }

        [Column("missionid")]
        public Guid missionid { get; set; }

        [Column("affectedindividualname")]
        public string affectedindividualname { get; set; }

        [Column("missionlocation")]
        public string missionlocation { get; set; }

        [Column("missiontype")]
        public string missiontype { get; set; }

        [Column("Missionurgency")]
        public int Missionurgency { get; set; }

        [Column("missionproximity")]
        public decimal? missionproximity { get; set; }

        [Column("missiondifficulty")]
        public int missiondifficulty { get; set; }

        [Column("missionbursttime")]
        public decimal? missionbursttime { get; set; }

        [Column("responderid")]
        public Guid? responderID { get; set; }

        [Column("dispatcherid")]
        public Guid dispatcherid { get; set; }

        [Column("missioncreatedat")]
        public DateTime missioncreatedat { get; set; }

        [Column("missionfinishedat")]
        public DateTime missionfinishedat { get; set; }
    }
}
