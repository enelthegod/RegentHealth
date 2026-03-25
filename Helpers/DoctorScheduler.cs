using RegentHealth.Enums;
using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Linq;

namespace RegentHealth.Helpers
{
    public static class DoctorScheduler
    {
        // Check if doctor has a rotation entry for this exact date
        public static bool IsDoctorScheduledForDate(Doctor doctor, DateTime date)
        {
            return DataService.Instance.WeeklyRotations.Any(r =>
                r.DoctorId == doctor.Id &&
                r.Date.Date == date.Date);  // exact date match, not just weekday
        }

        public static bool IsDoctorWorking(Doctor doctor, DateTime time)
        {
            // Must have a rotation entry for this specific date
            if (!IsDoctorScheduledForDate(doctor, time))
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
                if (a.DoctorId != doctor.UserId) return false;
                if (a.Status != AppointmentStatus.Scheduled) return false;

                int existingDuration =
                    AppointmentRules.GetDurationMinutes(a.Type) +
                    AppointmentRules.GetBreakMinutes(a.Type);

                DateTime existingEnd = a.AppointmentDate.AddMinutes(existingDuration);

                return start < existingEnd && end > a.AppointmentDate;
            });
        }

        public static Doctor FindLeastBusyDoctor(DateTime time, AppointmentType type)
        {
            var data = DataService.Instance;
            var doctors = data.Doctors.Where(d => d.IsActive).ToList();

            if (type == AppointmentType.Emergency)
            {
                // Emergency: need IsEmergencyDoctor flag
                // Check rotation for today specifically
                var emgDoctors = doctors
                    .Where(d => d.IsEmergencyDoctor &&
                                DataService.Instance.WeeklyRotations.Any(r =>
                                    r.DoctorId == d.Id &&
                                    r.Date.Date == time.Date &&
                                    r.IsEmergency == true))
                    .ToList();

                var free = emgDoctors
                    .Where(d => IsDoctorWorking(d, time) && !HasOverlap(d, time, type))
                    .ToList();

                var pool = free.Any() ? free
                    : emgDoctors.Where(d => IsDoctorWorking(d, time)).ToList();

                if (!pool.Any()) return null;

                return pool.OrderBy(d => data.Appointments.Count(a =>
                        a.DoctorId == d.UserId &&
                        a.Status == AppointmentStatus.Scheduled))
                    .First();
            }
            else
            {
                // Regular: any doctor with a rotation entry for this date
                var available = doctors
                    .Where(d => IsDoctorWorking(d, time) && !HasOverlap(d, time, type))
                    .ToList();

                if (!available.Any()) return null;

                return available.OrderBy(d => data.Appointments.Count(a =>
                        a.DoctorId == d.UserId &&
                        a.Status == AppointmentStatus.Scheduled))
                    .First();
            }
        }
    }
}