using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegentHealth.Models
{
    public class Doctor
    {
        public int Id { get; set; }  // PK
        public int UserId { get; set; }  // FK → User

        public bool IsActive { get; set; }
        public bool IsEmergencyDoctor { get; set; }
        public bool IsOnShift { get; set; }
        public DateTime? LastLogin { get; set; }

        public TimeSpan WorkStart { get; set; }
        public TimeSpan WorkEnd { get; set; }

        // EF Core cannot store List<DayOfWeek> directly
        // We store as "Monday,Tuesday,Wednesday" string in DB
        public string WorkingDaysRaw { get; set; } = string.Empty;

        // Navigation property — EF loads User automatically
        public User? User { get; set; }

        // Computed — not stored in DB
        [NotMapped]
        public string FullName =>
            User != null ? $"{User.Name} {User.Surname}" : "Unknown Doctor";

        // Convenience wrapper — converts string ↔ List in code
        [NotMapped]
        public List<DayOfWeek> WorkingDays
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WorkingDaysRaw))
                    return new List<DayOfWeek>();

                var result = new List<DayOfWeek>();
                foreach (var part in WorkingDaysRaw.Split(','))
                    if (Enum.TryParse<DayOfWeek>(part.Trim(), out var day))
                        result.Add(day);
                return result;
            }
            set
            {
                WorkingDaysRaw = string.Join(",", value);
            }
        }
    }
}