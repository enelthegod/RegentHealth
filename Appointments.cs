using System;
using RegentHealth.Enums;
namespace RegentHealth.Models;

public enum AppointmentStatus    // later one more enum with doctor category *AppointmentType*
{
    Scheduled,
    Cancelled,
    Completed
}

public class Appointment
{
    public TimeSlot TimeSlot { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public AppointmentType Type { get; set; }
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; }

    public decimal Price { get; set; }
    public bool isAvailible  { get; set; }
	public int PatientId { get; set; }
	public int DoctorId { get; set; }
    public int Id { get; set; }

    public string DisplayTime
    {
        get
        {
            return TimeSlot.ToString()
                .Replace("Slot", "")
                .Replace("_", ":");
        }
    }
    public override string ToString()
    {
        return $"{AppointmentDate:dd MMM yyyy} | {DisplayTime} | {Type} | Status: {Status}";
    }


}
