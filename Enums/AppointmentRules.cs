using RegentHealth.Enums;

namespace RegentHealth.Helpers
{
    public static class AppointmentRules
    {
        public static int GetSlotInterval(AppointmentType type)
        {
            switch (type)
            {
                case AppointmentType.BloodTest:
                    return 15;

                case AppointmentType.Checkup:
                    return 30;

                case AppointmentType.Consultation:
                    return 40;

                default:
                    return 30;
            }
        }

        public static bool IsEmergency(AppointmentType type)
        {
            return type == AppointmentType.Emergency;
        }
    }
}