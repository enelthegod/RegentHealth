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

            var doctors = data.Doctors
                .Where(d => d.IsActive && d.IsOnShift)
                .ToList();

            if (type == AppointmentType.Emergency)
            {
                doctors = doctors
                    .Where(d => d.IsEmergencyDoctor)
                    .ToList();
            }
            else
            {
                doctors = doctors
                    .Where(d => !d.IsEmergencyDoctor)
                    .ToList();
            }

            var availableDoctors = doctors
                .Where(d =>
                    IsDoctorWorking(d, time) &&
                    !HasOverlap(d, time, type))
                .ToList();

            if (!availableDoctors.Any())
                return null;

            return availableDoctors
                .OrderBy(d =>
                    data.Appointments.Count(a =>
                        a.DoctorId == d.UserId &&
                        a.Status == AppointmentStatus.Scheduled))
                .First();
        }
    }
}