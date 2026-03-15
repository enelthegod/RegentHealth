using RegentHealth.Enums;
using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Linq;

namespace RegentHealth.Helpers
{
    public static class DoctorScheduler
    {
        public static bool IsDoctorWorking(Doctor doctor, DateTime time)
        {
            if (!doctor.WorkingDays.Contains(time.DayOfWeek))
                return false;

            if (time.TimeOfDay < doctor.WorkStart)
                return false;

            if (time.TimeOfDay >= doctor.WorkEnd)
                return false;

            return true;
        }

        public static bool HasOverlap(Doctor doctor, DateTime start, AppointmentType type)
        {
            var data = DataService.Instance;

            int duration =
                AppointmentRules.GetDurationMinutes(type) +
                AppointmentRules.GetBreakMinutes(type);

            DateTime end = start.AddMinutes(duration);

            return data.Appointments.Any(a =>
            {
                if (a.DoctorId != doctor.UserId)
                    return false;

                if (a.Status != AppointmentStatus.Scheduled)
                    return false;

                int existingDuration =
                    AppointmentRules.GetDurationMinutes(a.Type) +
                    AppointmentRules.GetBreakMinutes(a.Type);

                DateTime existingStart = a.AppointmentDate;
                DateTime existingEnd = a.AppointmentDate.AddMinutes(existingDuration);

                return start < existingEnd && end > existingStart;
            });
        }

        public static Doctor FindLeastBusyDoctor(DateTime time, AppointmentType type)
        {
            var data = DataService.Instance;

            // Start with all active on-shift doctors
            var doctors = data.Doctors
                .Where(d => d.IsActive && d.IsOnShift)
                .ToList();

            if (type == AppointmentType.Emergency)
            {
                // FIX: Emergency needs a doctor with IsEmergencyDoctor = true
                // but we do NOT filter by overlap for emergency —
                // an emergency doctor can always take an emergency call
                // (they may interrupt a less critical task)
                var emergencyDoctors = doctors
                    .Where(d => d.IsEmergencyDoctor)
                    .ToList();

                // Among emergency doctors, prefer ones with no overlap,
                // but if all have overlap still pick the least busy one
                var free = emergencyDoctors
                    .Where(d => IsDoctorWorking(d, time) && !HasOverlap(d, time, type))
                    .ToList();

                var pool = free.Any() ? free : emergencyDoctors
                    .Where(d => IsDoctorWorking(d, time))
                    .ToList();

                if (!pool.Any())
                    return null;

                return pool
                    .OrderBy(d => data.Appointments.Count(a =>
                        a.DoctorId == d.UserId &&
                        a.Status == AppointmentStatus.Scheduled))
                    .First();
            }
            else
            {
                // FIX: For regular appointments, include ALL on-shift doctors
                // regardless of IsEmergencyDoctor flag.
                // A doctor with both Work+Emergency checked can still
                // take regular appointments when not handling emergencies.
                var available = doctors
                    .Where(d => IsDoctorWorking(d, time) && !HasOverlap(d, time, type))
                    .ToList();

                if (!available.Any())
                    return null;

                return available
                    .OrderBy(d => data.Appointments.Count(a =>
                        a.DoctorId == d.UserId &&
                        a.Status == AppointmentStatus.Scheduled))
                    .First();
            }
        }
    }
}