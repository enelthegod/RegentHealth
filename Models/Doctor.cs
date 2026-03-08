using System;
using System.Collections.Generic;

namespace RegentHealth.Models
{
    public class Doctor
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public bool IsActive { get; set; }

        public bool IsEmergencyDoctor { get; set; }

        // Working days
        public List<DayOfWeek> WorkingDays { get; set; } = new List<DayOfWeek>();

        public TimeSpan WorkStart { get; set; }

        public TimeSpan WorkEnd { get; set; }
    }
}