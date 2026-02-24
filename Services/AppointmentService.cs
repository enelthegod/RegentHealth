using RegentHealth.Enums;
using RegentHealth.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RegentHealth.Services
{
    public class AppointmentService
    {
        private readonly DataService _dataService;
        private readonly AuthService _authService;

        public AppointmentService(DataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        // CREATE APPOINTMENT      
        public Appointment CreateAppointment(
            DateTime date,
            TimeSlot timeSlot,
            AppointmentType type)
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            if (!_authService.IsPatient())
                throw new Exception("Only patients can create appointments.");

            if (date.Date < DateTime.Today)
                throw new Exception("Cannot create appointment in the past.");

            // get all doctors
            var doctors = _dataService.Users
                .Where(u => u.Role == UserRole.Doctor)
                .ToList();

            if (!doctors.Any())
                throw new Exception("No doctors available.");

            // search free doctor
            foreach (var doctor in doctors)
            {
                bool busy = _dataService.Appointments.Any(a =>
                    a.DoctorId == doctor.Id &&
                    a.AppointmentDate.Date == date.Date &&
                    a.TimeSlot == timeSlot &&
                    a.Status == AppointmentStatus.Scheduled);

                if (!busy)
                {
                    // create appointment
                    var appointment = new Appointment
                    {
                        Id = _dataService.Appointments.Count + 1,
                        PatientId = _authService.CurrentUser.Id,
                        DoctorId = doctor.Id,
                        AppointmentDate = date,
                        TimeSlot = timeSlot,
                        Type = type,
                        Status = AppointmentStatus.Scheduled
                    };

                    _dataService.Appointments.Add(appointment);

                    return appointment;
                }
            }

            throw new Exception("No available doctors for this time slot.");
        }

        
        // GET APPOINTMENTS FOR USER       
        public ObservableCollection<Appointment> GetAppointmentsForCurrentUser()
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            if (_authService.IsPatient())
            {
                return new ObservableCollection<Appointment>(
                    _dataService.Appointments
                        .Where(a => a.PatientId == _authService.CurrentUser.Id));
            }

            if (_authService.IsDoctor())
            {
                return new ObservableCollection<Appointment>(
                    _dataService.Appointments
                        .Where(a => a.DoctorId == _authService.CurrentUser.Id));
            }

            if (_authService.IsAdmin())
            {
                return new ObservableCollection<Appointment>(
                    _dataService.Appointments);
            }

            return new ObservableCollection<Appointment>();
        }

        
        // ADMIN — GET ALL
        public ObservableCollection<Appointment> GetAllAppointments()
        {
            if (!_authService.IsAdmin())
                throw new Exception("Access denied.");

            return _dataService.Appointments;
        }

        // CANCEL APPOINTMENT
        public void CancelAppointment(int appointmentId)
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            var appointment = _dataService.Appointments
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found.");

            // patient, doctor or admin can cancel
            if (_authService.IsAdmin() ||
                appointment.PatientId == _authService.CurrentUser.Id ||
                appointment.DoctorId == _authService.CurrentUser.Id)
            {
                appointment.Status = AppointmentStatus.Cancelled;
            }
            else
            {
                throw new Exception("Access denied.");
            }
        }
    }
}