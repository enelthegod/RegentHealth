namespace YourProject.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }


        // use timespans cause doctors need hours - not dates 
        public TimeSpan WorkStart { get; set; }

        public TimeSpan WorkEnd { get; set; }

        public TimeSpan? BreakStart { get; set; }
        public TimeSpan? BreakEnd { get; set; }

        public int AppointmentDurationMinutes { get; set; } = 20;
    }
}