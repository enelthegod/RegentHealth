using RegentHealth.Enums;
using RegentHealth.Helpers;
using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Windows;

namespace RegentHealth
{
    public partial class PatientWindow : Window
    {
        private readonly AppointmentService _appointmentService;
        private readonly AuthService _authService;

        public PatientWindow(
            AppointmentService appointmentService,
            AuthService authService)
        {
            InitializeComponent();

            _appointmentService = appointmentService;
            _authService = authService;

            // fill appointment types from enum
            AppointmentTypeComboBox.ItemsSource =
    Enum.GetValues(typeof(AppointmentType));

            TimeSlotComboBox.ItemsSource =
    EnumDisplayHelper.GetTimeSlots();



            LoadAppointments();
        }



        private void CreateAppointment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                if (AppointmentDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Please select a date");
                    return;
                }

                if (AppointmentTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Select appointment type");
                    return;
                }

                if (TimeSlotComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Select time slot");
                    return;
                }

                DateTime date = AppointmentDatePicker.SelectedDate.Value;

                AppointmentType type =
                    (AppointmentType)AppointmentTypeComboBox.SelectedItem;

                                                                                 // delete "Slot" before time in combobox
                var selectedTime = TimeSlotComboBox.SelectedItem;
                if (selectedTime == null) { }                                           // attention ! 

                string enumName = "Slot" + selectedTime.ToString().Replace(":", "_");

                TimeSlot slot =
                        (TimeSlot)Enum.Parse(
                        typeof(TimeSlot),
                        enumName);


                _appointmentService.CreateAppointment(
                                date,
                                slot,
                                type);



                MessageBox.Show("Appointment created!");

                LoadAppointments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            var selectedAppointment = AppointmentsListBox.SelectedItem as Appointment;

            if (selectedAppointment == null)
            {
                MessageBox.Show("Please select an appointment to cancel.");
                return;
            }

            DataService.CancelAppointment(selectedAppointment);

            LoadAppointments();
        }



        private void Back_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard =
                new DashboardWindow(_authService);

            dashboard.Show();
            Close();
        }




        private void LoadAppointments()
        {
            var appointments =
                _appointmentService.GetAppointmentsForCurrentUser();

            AppointmentsListBox.ItemsSource = DataService.Instance.Appointments;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) // temp leave then will delete
        {
            LoadAppointments();
        }
    }
}

