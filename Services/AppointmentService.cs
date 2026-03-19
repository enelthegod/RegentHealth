using RegentHealth.Enums;
using RegentHealth.Helpers;
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

        // ── CREATE APPOINTMENT ───────────────────────────────────────
        public Appointment CreateAppointment(DateTime dateTime, AppointmentType type)
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            if (!_authService.IsPatient())
                throw new Exception("Only patients can create appointments.");

            if (dateTime < DateTime.Now)
                throw new Exception("Cannot create appointment in the past.");

            if (dateTime.Date > DateTime.Today.AddDays(14))
                throw new Exception("Appointments can only be booked up to 14 days ahead.");

            DayOfWeek day = dateTime.DayOfWeek;
            bool isWeekend = day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;

            if (isWeekend && type != AppointmentType.Emergency)
                throw new Exception("Only emergency appointments allowed on weekends.");

            TimeSpan time = dateTime.TimeOfDay;
            if (time >= new TimeSpan(12, 0, 0) && time < new TimeSpan(13, 0, 0))
                throw new Exception("Doctor is on lunch break.");

            // ── EMERGENCY ────────────────────────────────────────────
            if (type == AppointmentType.Emergency)
            {
                // Anti-spam: one active emergency per patient
                bool alreadyHasEmergency =
                    _dataService.Appointments.Any(a =>
                        a.PatientId == _authService.CurrentUser.Id &&
                        a.Type == AppointmentType.Emergency &&
                        a.Status == AppointmentStatus.Scheduled)
                    || _dataService.EmergencyQueue.Any(e =>
                        e.PatientId == _authService.CurrentUser.Id);

                if (alreadyHasEmergency)
                    throw new Exception("You already have an active emergency request.");

                // FIX: look for a free emergency doctor RIGHT NOW
                var emergencyDoctor = DoctorScheduler.FindLeastBusyDoctor(dateTime, type);

                if (emergencyDoctor != null)
                {
                    // Doctor is free → book immediately
                    var appointment = new Appointment
                    {
                        Id = _dataService.Appointments.Count + 1,
                        PatientId = _authService.CurrentUser.Id,
                        DoctorId = emergencyDoctor.UserId,
                        AppointmentDate = dateTime,
                        Type = type,
                        Status = AppointmentStatus.Scheduled
                    };

                    _dataService.Appointments.Add(appointment);
                    return appointment;
                }
                else
                {
                    // All emergency doctors are busy → add to queue
                    var queued = new Appointment
                    {
                        Id = _dataService.Appointments.Count + 1,
                        PatientId = _authService.CurrentUser.Id,
                        DoctorId = 0, // assigned when dequeued
                        AppointmentDate = dateTime,
                        Type = type,
                        Status = AppointmentStatus.Scheduled
                    };

                    _dataService.EmergencyQueue.Enqueue(queued);
                    int position = _dataService.EmergencyQueue.Count;

                    throw new Exception(
                        $"All emergency doctors are busy.\n" +
                        $"You have been added to the queue. Position: {position}.");
                }
            }

            // ── REGULAR APPOINTMENT ──────────────────────────────────
            var doctor = DoctorScheduler.FindLeastBusyDoctor(dateTime, type);

            if (doctor == null)
                throw new Exception("No available doctors for this time slot.");

            var regular = new Appointment
            {
                Id = _dataService.Appointments.Count + 1,
                PatientId = _authService.CurrentUser.Id,
                DoctorId = doctor.UserId,
                AppointmentDate = dateTime,
                Type = type,
                Status = AppointmentStatus.Scheduled
            };

            _dataService.Appointments.Add(regular);
            return regular;
        }

        // ── GET APPOINTMENTS FOR CURRENT USER ────────────────────────
        public ObservableCollection<Appointment> GetAppointmentsForCurrentUser()
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            if (_authService.IsPatient())
                return new ObservableCollection<Appointment>(
                    _dataService.Appointments
                        .Where(a => a.PatientId == _authService.CurrentUser.Id));

            if (_authService.IsDoctor())
                return new ObservableCollection<Appointment>(
                    _dataService.Appointments
                        .Where(a => a.DoctorId == _authService.CurrentUser.Id));

            if (_authService.IsAdmin())
                return new ObservableCollection<Appointment>(_dataService.Appointments);

            return new ObservableCollection<Appointment>();
        }

        // ── ADMIN — GET ALL ──────────────────────────────────────────
        public ObservableCollection<Appointment> GetAllAppointments()
        {
            if (!_authService.IsAdmin())
                throw new Exception("Access denied.");

            return _dataService.Appointments;
        }

        // ── CANCEL APPOINTMENT ───────────────────────────────────────
        public void CancelAppointment(int appointmentId)
        {
            if (_authService.CurrentUser == null)
                throw new Exception("User not logged in.");

            var appointment = _dataService.Appointments
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found.");

            if (_authService.IsAdmin())
            {
                appointment.Status = AppointmentStatus.Cancelled;
                return;
            }

            if (_authService.IsPatient())
            {
                if (appointment.PatientId != _authService.CurrentUser.Id)
                    throw new Exception("You can only cancel your own appointments.");

                appointment.Status = AppointmentStatus.Cancelled;
                return;
            }

            if (_authService.IsDoctor())
            {
                if (appointment.DoctorId != _authService.CurrentUser.Id)
                    throw new Exception("You can only cancel your own appointments.");

                appointment.Status = AppointmentStatus.Cancelled;
                return;
            }

            throw new Exception("Access denied.");
        }

        // ── COMPLETE APPOINTMENT (doctor action) ─────────────────────
        public void CompleteAppointment(int appointmentId)
        {
            var appointment = _dataService.Appointments
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found.");

            if (!_authService.IsDoctor())
                throw new Exception("Only a doctor can complete an appointment.");

            appointment.Status = AppointmentStatus.Completed;

            // Pass the completing doctor's ID so they get the next queued patient
            ProcessEmergencyQueue(appointment.DoctorId);
        }


        // ── PROCESS EMERGENCY QUEUE ──────────────────────────────────

        public void ProcessEmergencyQueue(int completingDoctorId = 0)
        {
            if (!_dataService.EmergencyQueue.Any())
                return;

            // Prefer the doctor who just finished (they are free right now).
            // Fall back to any other available emergency doctor.
            Doctor doctor = null;

            if (completingDoctorId > 0)
                doctor = _dataService.Doctors
                    .FirstOrDefault(d => d.UserId == completingDoctorId
                                     && d.IsActive && d.IsOnShift);

            if (doctor == null)
                doctor = DoctorScheduler.FindLeastBusyDoctor(
                    DateTime.Now, AppointmentType.Emergency);

            if (doctor == null)
                return; // no available emergency doctor at all

            var next = _dataService.EmergencyQueue.Dequeue();
            next.DoctorId = doctor.UserId;
            next.AppointmentDate = DateTime.Now;
            next.Status = AppointmentStatus.Scheduled;

            _dataService.Appointments.Add(next);
        }
    }
}