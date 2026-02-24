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

        private Appointment _selectedAppointment;

        // SELECTED ITEM FROM LISTBOX
        public Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                _selectedAppointment = value;
                OnPropertyChanged(nameof(SelectedAppointment));
                OnPropertyChanged(nameof(CanCancel)); 
            }
        }

        // BUTTON ENABLE LOGIC
        public bool CanCancel =>
            SelectedAppointment != null &&
            SelectedAppointment.Status == AppointmentStatus.Scheduled;

        public event PropertyChangedEventHandler PropertyChanged;

        public AppointmentsViewModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;

            var appointments =
                _appointmentService.GetAppointmentsForCurrentUser();

            Appointments = new ObservableCollection<Appointment>(appointments);
        }

        // CREATE
        public void CreateAppointment(
            DateTime date,
            TimeSlot slot,
            AppointmentType type)
        {
            var appointment =
                _appointmentService.CreateAppointment(date, slot, type);

            Appointments.Add(appointment);
        }

        // CANCEL
        public void CancelAppointment()
        {
            if (!CanCancel)
                return;

            _appointmentService.CancelAppointment(SelectedAppointment.Id);

            SelectedAppointment.Status = AppointmentStatus.Cancelled;

            OnPropertyChanged(nameof(Appointments));
            OnPropertyChanged(nameof(CanCancel));
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}