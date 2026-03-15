using RegentHealth.Models;
using RegentHealth.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace RegentHealth.ViewModels
{
    public class DoctorViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _appointmentService;

        public ObservableCollection<Appointment> Appointments { get; private set; }

        private Appointment _selectedAppointment;
        public Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                _selectedAppointment = value;
                OnPropertyChanged(nameof(SelectedAppointment));
            }
        }

        public DoctorViewModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
            LoadAppointments();
        }

        // Reload list fresh from DataService (picks up new emergency from queue)
        public void LoadAppointments()
        {
            var fresh = _appointmentService.GetAppointmentsForCurrentUser();

            if (Appointments == null)
            {
                Appointments = fresh;
                return;
            }

            // Sync in place so UI binding stays alive
            Appointments.Clear();
            foreach (var a in fresh)
                Appointments.Add(a);

            OnPropertyChanged(nameof(Appointments));
        }

        public void CompleteAppointment()
        {
            if (SelectedAppointment == null)
                return;

            // 1. Mark as completed + auto-process emergency queue
            _appointmentService.CompleteAppointment(SelectedAppointment.Id);

            // 2. Reload list — new emergency appointment (if any) will appear
            LoadAppointments();

            SelectedAppointment = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}