using System;

namespace RegentHealth.Models
{
    public class DoctorRotation
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DayOfWeek Day { get; set; }
        public bool IsEmergency { get; set; }

        // Concrete date so Mon this week ≠ Mon next week
        public DateTime Date { get; set; }

        public Doctor? Doctor { get; set; }
    }
}