using RegentHealth.Helpers;
using RegentHealth.Models;

namespace RegentHealth.Services
{
    public class AdminService
    {
 
        private readonly DataService _dataService;

        public AdminService(DataService  dataService)
        {
            _dataService = dataService;
        
        }




        public void CreateDoctor(
            string name,
            string surname,
            string email,
            string password)
        {
            int userId = _dataService.Users.Count + 1;

            var user = new User
            {
                Id = userId,
                Name = name,
                Surname = surname,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                Role = UserRole.Doctor
            };

            _dataService.Users.Add(user);

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

            _dataService.Doctors.Add(doctor);
        }

        public List<Doctor> GetDoctors()
        {
            return _dataService.Doctors;
        }





        // TEMP BUTTON FOR ON/OFF
        public void ToggleDoctor(int userId)
        {
            var doctor = _dataService.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found.");

            doctor.IsActive = !doctor.IsActive;
        }





        // EMERGENCY DOC
        public void SetEmergencyDoctor(int doctorId)
        {
            var existing = _dataService.WeeklyRotations
                .FirstOrDefault(r => r.IsEmergency);

            if (existing != null)
            {
                existing.DoctorId = doctorId;
                return;
            }

            _dataService.WeeklyRotations.Add(new DoctorRotation
            {
                Day = DateTime.Today.DayOfWeek,
                DoctorId = doctorId,
                IsEmergency = true
            });
        }


        // ABOUT ROTATION 
        public List<DoctorRotation> GetWeeklyRotation()
        {
            return _dataService.WeeklyRotations;
        }

        public void SetDoctorRotation(DayOfWeek day, int doctorId)
        {
            var existing = _dataService.WeeklyRotations
                .FirstOrDefault(r => r.Day == day && !r.IsEmergency);

            if (existing != null)
            {
                existing.DoctorId = doctorId;
                return;
            }

            _dataService.WeeklyRotations.Add(new DoctorRotation
            {
                Day = day,
                DoctorId = doctorId,
                IsEmergency = false
            });
        }
    }
}