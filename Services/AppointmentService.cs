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

        public User CurrentUser => _authService.CurrentUser;

        public AppointmentService(DataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        // CREATE APPOINTMENT      
        public Appointment CreateAppointment(
                                   DateTime dateTime,  
                                   AppointmentType type)
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            if (!_authService.IsPatient())
                throw new Exception("Only patients can create appointments.");

            if (dateTime < DateTime.Now)
                throw new Exception("Cannot create appointment in the past.");
            if (dateTime.Date > DateTime.Today.AddDays(14))
                throw new Exception("Appointments can only be booked up to 14 days ahead.");

            // get all doctors
            var doctors = _dataService.Doctors
                            .Where(d => d.IsActive)
                            .ToList();

            if (!doctors.Any())
                throw new Exception("No doctors available.");

            // searching free doctors for timeslot
            foreach (var doctor in doctors)
            {
                bool busy = _dataService.Appointments.Any(a =>
                    a.DoctorId == doctor.UserId &&
                    a.AppointmentDate == dateTime && 
                    a.Status == AppointmentStatus.Scheduled);

                if (!busy)
                {
                    
                    var appointment = new Appointment
                    {
                        Id = _dataService.Appointments.Count + 1,
                        PatientId = _authService.CurrentUser.Id,
                        DoctorId = doctor.UserId,
                        AppointmentDate = dateTime,
                        Type = type,
                        Status = AppointmentStatus.Scheduled
                    };

                    _dataService.Appointments.Add(appointment);

                    return appointment;
                }
            }

            throw new Exception("No available doctors for this time.");
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

            var currentUser = _authService.CurrentUser;

            // ADMIN
            if (_authService.IsAdmin())
            {
                appointment.Status = AppointmentStatus.Cancelled;
                return;
            }

            // PATIENT
            if (_authService.IsPatient())
            {
                if (appointment.PatientId != currentUser.Id)
                    throw new Exception("You can cancel only your own appointments.");

                appointment.Status = AppointmentStatus.Cancelled;
                return;
            }

            // DOCTOR
            if (_authService.IsDoctor())
            {
                if (appointment.DoctorId != currentUser.Id)
                    throw new Exception("You can cancel only your own appointments.");

                appointment.Status = AppointmentStatus.Cancelled;
                return;
            }

            throw new Exception("Access denied.");
        }

        // COMPLETE APPOINTMENT - FOR DOCTORS
        public void CompleteAppointment(int appointmentId)
        {
            var appointment = _dataService.Appointments
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found.");

            if (!_authService.IsDoctor())
                throw new Exception("Only doctor can complete appointment.");

            appointment.Status = AppointmentStatus.Completed;
        }

        // DOCTORS WORKTIME WINDOWS
        public List<DateTime> GetAvailableTimeSlots(int doctorId, DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            var schedule = _dataService.DoctorSchedules
                .FirstOrDefault(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek);

            if (schedule == null)
                return new List<DateTime>();

            var result = new List<DateTime>();

            var current = schedule.WorkStart;
            var slotInterval = TimeSpan.FromMinutes(schedule.SlotIntervalMinutes);
            var appointmentDuration = TimeSpan.FromMinutes(schedule.AppointmentDurationMinutes);

            while (current + appointmentDuration <= schedule.WorkEnd)
            {
                bool isInBreak = schedule.BreakStart.HasValue && schedule.BreakEnd.HasValue &&
                                 current >= schedule.BreakStart.Value &&
                                 current < schedule.BreakEnd.Value;

                if (!isInBreak)
                {
                    var slotDateTime = date.Date + current;

                    bool isBooked = _dataService.Appointments.Any(a =>
                        a.DoctorId == doctorId &&
                        a.AppointmentDate == slotDateTime);

                    if (!isBooked)
                        result.Add(slotDateTime);
                }

                current = current.Add(slotInterval);
            }

            return result;
        }


    }
}