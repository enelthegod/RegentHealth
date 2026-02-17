using System;
using System.Collections.Generic;
using System.Linq;
using RegentHealth.Enums;
using RegentHealth.Models;

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

        public Appointment CreateAppointment(
            int doctorId,
            DateTime date,
            TimeSlot timeSlot,
            AppointmentType type)
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            if (!_authService.IsPatient())
                throw new Exception("Only patients can create appointments.");

            if (date.Date < DateTime.Now.Date)
                throw new Exception("Cannot create appointment in the past.");

            var appointment = new Appointment
            {
                Id = _dataService.Appointments.Count + 1,
                PatientId = _authService.CurrentUser.Id,
                DoctorId = doctorId,
                AppointmentDate = date.Date,
                TimeSlot = timeSlot,
                Type = type,
                Status = AppointmentStatus.Scheduled
            };

            _dataService.Appointments.Add(appointment);

            return appointment;
        }


        public List<Appointment> GetAppointmentsForCurrentUser()
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            if (_authService.IsPatient())
            {
                return _dataService.Appointments
                    .Where(a => a.PatientId == _authService.CurrentUser.Id)
                    .ToList();
            }

            if (_authService.IsDoctor())
            {
                return _dataService.Appointments
                    .Where(a => a.DoctorId == _authService.CurrentUser.Id)
                    .ToList();
            }

            if (_authService.IsAdmin())
            {
                return _dataService.Appointments;
            }

            return new List<Appointment>();
        }

                                                               // future admin dashboard usage window
        public List<Appointment> GetAllAppointments()
        {
            if (!_authService.IsAdmin())
                throw new Exception("Access denied.");

            return _dataService.Appointments;
        }

        public void CancelAppointment(int appointmentId)                  // not delete => change to cancel 
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            var appointment = _dataService.Appointments
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found.");

            if (_authService.IsAdmin() ||                                   //only patient with his appoint or his doctor or admin can cancel
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
