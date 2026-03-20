using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using RegentHealth.Enums;

namespace RegentHealth.Models
{
    public enum AppointmentStatus
    {
        Scheduled,
        Cancelled,
        Completed
    }

    public class Appointment : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

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

        // Navigation properties — EF loads these automatically
        public User? Patient { get; set; }
        public User? Doctor { get; set; }

        // Computed — not stored in DB
        [NotMapped]
        public string DoctorFullName => Doctor?.FullName ?? "Unknown Doctor";

        [NotMapped]
        public string PatientFullName => Patient?.FullName ?? "Unknown Patient";

        public override string ToString() =>
            $"{AppointmentDate:dd MMM yyyy} | {AppointmentDate:HH:mm} | {Type} | {Status}";

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}