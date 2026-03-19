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
    public event PropertyChangedEventHandler PropertyChanged ;

    private AppointmentStatus _status;

    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }
    public AppointmentType Type { get; set; }

    public AppointmentStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }


    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(name));
    }

    public override string ToString()
    {
        return $"{AppointmentDate:dd MMM yyyy} | {AppointmentDate:HH:mm} | {Type} | Status: {Status}";
    }

    public string DoctorFullName
    {
        get
        {
            var doctorUser = DataService.Instance.Users
                .FirstOrDefault(u => u.Id == DoctorId);

            return doctorUser != null
                ? $"{doctorUser.FullName}"
                : "Unknown Doctor";
        }
    }

    public string PatientFullName
    {
        get
        {
            var patientUser = DataService.Instance.Users
                .FirstOrDefault(u => u.Id == PatientId);

            return patientUser != null
                ? $"{patientUser.FullName}"
                : "Unknown Patient";
        }
    }
}
