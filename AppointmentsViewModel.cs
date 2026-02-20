using RegentHealth.Enums;
using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RegentHealth.ViewModels
{
    public class AppointmentsViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _appointmentService;

        public ObservableCollection<Appointment> Appointments { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppointmentsViewModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;

            LoadAppointments();
        }


        private void LoadAppointments()
        {
            Appointments =
                _appointmentService.GetAppointmentsForCurrentUser();

            OnPropertyChanged(nameof(Appointments));
        }

        public void Refresh()
        {
            LoadAppointments();
        }

        public void CreateAppointment(
            DateTime date,
            TimeSlot slot,
            AppointmentType type)
        {
            _appointmentService.CreateAppointment(date, slot, type);

            LoadAppointments();
        }

        public void CancelAppointment(int appointmentId)
        {
            _appointmentService.CancelAppointment(appointmentId);

            LoadAppointments();
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}
