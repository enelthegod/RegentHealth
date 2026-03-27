using RegentHealth.Models;

namespace RegentHealth.Services
{
    public class AdminService
    {
        private readonly DataService _dataService;

        public AdminService(DataService dataService)
        {
            _dataService = dataService;
        }

        // CreateDoctor and SetEmergencyDoctor used to live here
        // but they duplicate AuthService.RegisterDoctor and RotationPage logic
        // Kept only methods that are actually used elsewhere in the project

        public List<Doctor> GetDoctors()
        {
            return _dataService.Doctors;
        }

        public List<DoctorRotation> GetWeeklyRotation()
        {
            return _dataService.WeeklyRotations;
        }

        // Toggle doctor active status and persist to DB
        public void ToggleDoctor(int userId)
        {
            var doctor = _dataService.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found.");

            doctor.IsActive = !doctor.IsActive;

            // Save the updated IsActive flag to DB
            _dataService.SaveDoctors();
        }
    }
}