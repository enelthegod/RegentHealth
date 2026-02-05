using System;

public class Appointments
{
	public DateTime dateTime { get; set; }
    public required string Type  { get; set; } // Dentist / Therapist / Surgery
    public decimal Price { get; set; }
    public bool isAvailible  { get; set; }
	public int? idClient { get; set; }
	public int? idDoctor { get; set; }
    public int Id { get; set; }



}
