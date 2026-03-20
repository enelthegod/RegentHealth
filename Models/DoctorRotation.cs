using System;

namespace RegentHealth.Models
{
    public class DoctorRotation
    {
        public int Id { get; set; }  // PK
        public int DoctorId { get; set; }  // FK → Doctor
        public DayOfWeek Day { get; set; }
        public bool IsEmergency { get; set; }

        // Navigation property
        public Doctor? Doctor { get; set; }
    }
}