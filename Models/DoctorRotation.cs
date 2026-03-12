using System;

namespace RegentHealth.Models
{
    public class DoctorRotation
    {
        public DayOfWeek Day { get; set; }

        public int DoctorId { get; set; }

        public bool IsEmergency { get; set; }
    }
}
