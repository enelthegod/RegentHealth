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

        public ObservableCollection<Appointment> Appointments { get; }

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

            Appointments =
                _appointmentService.GetAppointmentsForCurrentUser();
        }

        public void CompleteAppointment()
        {
            if (SelectedAppointment == null)
                return;

            _appointmentService.CompleteAppointment(SelectedAppointment.Id);

            SelectedAppointment.Status = AppointmentStatus.Completed;

            OnPropertyChanged(nameof(Appointments));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));
        }
    }
}
