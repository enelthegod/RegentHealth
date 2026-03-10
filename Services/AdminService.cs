using RegentHealth.Helpers;
using RegentHealth.Models;

namespace RegentHealth.Services
{
    public class AdminService
    {
        private readonly DataService _data;

        public AdminService(DataService data)
        {
            _data = data;
        }

        public void CreateDoctor(
            string name,
            string surname,
            string email,
            string password)
        {
            int userId = _data.Users.Count + 1;

            var user = new User
            {
                Id = userId,
                Name = name,
                Surname = surname,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                Role = UserRole.Doctor
            };

            _data.Users.Add(user);

            var doctor = new Doctor
            {
                UserId = user.Id,
                WorkStart = new TimeSpan(9, 0, 0),
                WorkEnd = new TimeSpan(17, 0, 0),
                IsActive = true,

                WorkingDays = new List<DayOfWeek>
                {
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday
                }
            };

            _data.Doctors.Add(doctor);
        }

        public List<Doctor> GetDoctors()
        {
            return _data.Doctors;
        }

        // TEMP BUTTON FOR ON/OFF
        public void ToggleDoctor(int userId)
        {
            var doctor = _data.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found.");

            doctor.IsActive = !doctor.IsActive;
        }
        // EMERGENCY DOC
        public void SetEmergencyDoctor(int userId)
        {
            var doctor = _data.Doctors.FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return;

            if (doctor.IsEmergencyDoctor)
            {
                doctor.IsEmergencyDoctor = false;
            }
            else
            {
                foreach (var doc in _data.Doctors)
                    doc.IsEmergencyDoctor = false;

                doctor.IsEmergencyDoctor = true;
            }
        }
    }
}