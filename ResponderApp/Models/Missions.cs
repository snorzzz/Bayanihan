using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ResponderApp.Models
{
    [Table("missions")]
    public class Missions : BaseModel
    {
        [PrimaryKey("missionID", false)]
        [Column("missionID")]
        public Guid MissionID { get; set; }

        [Column("rescueeName")]
        public string? RescueeName { get; set; }

        [Column("rescueeContact")]
        public string? RescueeContact { get; set; }

        [Column("missionLocation")]
        public string? MissionLocation { get; set; }

        [Column("missionIncident")]
        public string? MissionIncident { get; set; }

        [Column("missionSeverity")]
        public int? MissionSeverity { get; set; }

        [Column("missionProximity")]
        public double? MissionProximity { get; set; }

        [Column("missionDifficulty")]
        public int? MissionDifficulty { get; set; }

        [Column("missionBurstScore")]
        public double MissionBurstScore { get; set; }

        [Column("needBackUp")]
        public bool NeedBackUp { get; set; }

        [Column("missionStatus")]
        public string MissionStatus { get; set; }

        [Column("assignedResponderID")]
        public Guid[] AssignedResponderID { get; set; } = Array.Empty<Guid>();

        [Column("missionCreatedAt")]
        public DateTime missionCreatedAt { get; set; }

        [Column("missionUpdatedAt")]
        public DateTime MissionUpdatedAt { get; set; }

        [Column("backupNum")]
        public int? BackupNum { get; set; }

        [Column("missionLat")]
        public float? MissionLat { get; set; }

        [Column("missionLng")]
        public float? MissionLng { get; set; }

        [Column("missionAssessment")]
        public string? MissionAssessment { get; set; }

        [Column("numOfCasualties")]
        public int? NumOfCasualties { get; set; }

        [Column("actionTaken")]
        public string ActionTaken { get; set; }

        [Column("missionNeeds")]
        public string? MissionNeeds { get; set; }
    }
}
