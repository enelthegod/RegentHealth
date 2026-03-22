using Microsoft.EntityFrameworkCore;
using RegentHealth.Data;
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
                bool alreadyHasEmergency =
                    _dataService.Appointments.Any(a =>
                        a.PatientId == _authService.CurrentUser.Id &&
                        a.Type == AppointmentType.Emergency &&
                        a.Status == AppointmentStatus.Scheduled)
                    || _dataService.EmergencyQueue.Any(e =>
                        e.PatientId == _authService.CurrentUser.Id);

                if (alreadyHasEmergency)
                    throw new Exception("You already have an active emergency request.");

                var emergencyDoctor = DoctorScheduler.FindLeastBusyDoctor(dateTime, type);

                if (emergencyDoctor != null)
                {
                    var appointment = new Appointment
                    {
                        PatientId = _authService.CurrentUser.Id,
                        DoctorId = emergencyDoctor.UserId,
                        AppointmentDate = dateTime,
                        Type = type,
                        Status = AppointmentStatus.Scheduled
                    };

                    SaveAppointment(appointment);
                    return appointment;
                }
                else
                {
                    var queued = new Appointment
                    {
                        PatientId = _authService.CurrentUser.Id,
                        DoctorId = 0,
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



            // ── REGULAR ──────────────────────────────────────────────
            var doctor = DoctorScheduler.FindLeastBusyDoctor(dateTime, type);

            if (doctor == null)
                throw new Exception("No available doctors for this time slot.");

            var regular = new Appointment
            {
                PatientId = _authService.CurrentUser.Id,
                DoctorId = doctor.UserId,
                AppointmentDate = dateTime,
                Type = type,
                Status = AppointmentStatus.Scheduled
            };

            SaveAppointment(regular);
            return regular;
        }




        // Saves to DB and adds to in-memory list
        private void SaveAppointment(Appointment appointment)
        {
            using (var db = new AppDbContext())
            {
                db.Appointments.Add(appointment);
                db.SaveChanges(); // SQLite assigns appointment.Id

                // Reload with navigation properties so FullName works in UI
                var saved = db.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .First(a => a.Id == appointment.Id);

                _dataService.Appointments.Add(saved);
            }
        }



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



        public ObservableCollection<Appointment> GetAllAppointments()
        {
            if (!_authService.IsAdmin())
                throw new Exception("Access denied.");

            return _dataService.Appointments;
        }



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
            }
            else if (_authService.IsPatient())
            {
                if (appointment.PatientId != _authService.CurrentUser.Id)
                    throw new Exception("You can only cancel your own appointments.");
                appointment.Status = AppointmentStatus.Cancelled;
            }
            else if (_authService.IsDoctor())
            {
                if (appointment.DoctorId != _authService.CurrentUser.Id)
                    throw new Exception("You can only cancel your own appointments.");
                appointment.Status = AppointmentStatus.Cancelled;
            }
            else
            {
                throw new Exception("Access denied.");
            }

            // Save updated status to DB
            UpdateAppointmentStatus(appointment);
        }



        public void CompleteAppointment(int appointmentId)
        {
            var appointment = _dataService.Appointments
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found.");

            if (!_authService.IsDoctor())
                throw new Exception("Only a doctor can complete an appointment.");

            appointment.Status = AppointmentStatus.Completed;

            // Save updated status to DB
            UpdateAppointmentStatus(appointment);

            ProcessEmergencyQueue(appointment.DoctorId);
        }



        // Updates only the Status field of an existing appointment in DB
        private void UpdateAppointmentStatus(Appointment appointment)
        {
            using (var db = new AppDbContext())
            {
                // Attach tells EF "this object already exists in DB, just update it"
                db.Appointments.Attach(appointment);
                db.Entry(appointment).Property(a => a.Status).IsModified = true;
                db.SaveChanges();
            }
        }



        public void ProcessEmergencyQueue(int completingDoctorId = 0)
        {
            if (!_dataService.EmergencyQueue.Any())
                return;

            Doctor doctor = null;

            if (completingDoctorId > 0)
                doctor = _dataService.Doctors
                    .FirstOrDefault(d => d.UserId == completingDoctorId
                                     && d.IsActive && d.IsOnShift);

            if (doctor == null)
                doctor = DoctorScheduler.FindLeastBusyDoctor(
                    DateTime.Now, AppointmentType.Emergency);

            if (doctor == null)
                return;

            var next = _dataService.EmergencyQueue.Dequeue();
            next.DoctorId = doctor.UserId;
            next.AppointmentDate = DateTime.Now;
            next.Status = AppointmentStatus.Scheduled;

            SaveAppointment(next);
        }



        public List<DateTime> GetAvailableTimeSlots(int doctorId, DateTime date)
        {
            var doctor = _dataService.Doctors
                .FirstOrDefault(d => d.UserId == doctorId);

            if (doctor == null) return new List<DateTime>();

            var result = new List<DateTime>();
            var start = date.Date + doctor.WorkStart;
            var end = date.Date + doctor.WorkEnd;
            var current = start;

            while (current < end)
            {
                if (current.Hour != 12)
                {
                    bool isBooked = _dataService.Appointments.Any(a =>
                        a.DoctorId == doctorId &&
                        a.AppointmentDate == current &&
                        a.Status == AppointmentStatus.Scheduled);

                    if (!isBooked)
                        result.Add(current);
                }
                current = current.AddMinutes(30);
            }

            return result;
        }
    }
}