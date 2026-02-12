using System;
namespace RegentHealth.Models;

public enum AppointmentStatus    // later one more enum with doctor category *AppointmentType*
{
    Scheduled,
    Cancelled,
    Completed
}
public class Appointment
{
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; }

    public decimal Price { get; set; }
    public bool isAvailible  { get; set; }
	public int PatientId { get; set; }
	public int DoctorId { get; set; }
    public int Id { get; set; }



}
