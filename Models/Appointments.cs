using System;
using System.ComponentModel;
using RegentHealth.Enums;

namespace RegentHealth.Models;

public enum AppointmentStatus
{
    Scheduled,
    Cancelled,
    Completed
}

public class Appointment : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private AppointmentStatus _status;

    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }
    public TimeSlot TimeSlot { get; set; }
    public AppointmentType Type { get; set; }

    public decimal Price { get; set; }

    public AppointmentStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public string DisplayTime =>
        TimeSlot.ToString()
            .Replace("Slot", "")
            .Replace("_", ":");

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(name));
    }

    public override string ToString()
    {
        return $"{AppointmentDate:dd MMM yyyy} | {DisplayTime} | {Type} | Status: {Status}";
    }
}
