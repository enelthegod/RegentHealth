using RegentHealth.Enums;
using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace RegentHealth.ViewModels
{
    public class AppointmentsViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _appointmentService;

        public ObservableCollection<Appointment> Appointments { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppointmentsViewModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;

            // load 1 time
            var appointments =
                _appointmentService.GetAppointmentsForCurrentUser();

            Appointments = new ObservableCollection<Appointment>(appointments);
        }

        public void CreateAppointment(
            DateTime date,
            TimeSlot slot,
            AppointmentType type)
        {
            var appointment =
                _appointmentService.CreateAppointment(date, slot, type);

            // ui auto-update
            Appointments.Add(appointment);
        }

        public void CancelAppointment(int appointmentId)
        {
            _appointmentService.CancelAppointment(appointmentId);

            var appointment = Appointments
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment != null)
            {
                // Меняем статус вместо удаления
                appointment.Status = AppointmentStatus.Cancelled;

                // Уведомляем UI о том, что объект изменился
                OnPropertyChanged(nameof(Appointments));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}