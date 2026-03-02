using System;

namespace RegentHealth.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        // connection with  User
        public int UserId { get; set; }

        // working schedule
        public TimeSpan WorkStart { get; set; }
        public TimeSpan WorkEnd { get; set; }

        // lunch break
        public TimeSpan LunchStart { get; set; }
        public TimeSpan LunchEnd { get; set; }

        // appointment duration
        public int AppointmentDurationMinutes { get; set; } = 20;

        public bool IsActive { get; set; } = true;
    }
}