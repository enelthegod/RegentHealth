using RegentHealth.Enums;

namespace RegentHealth.Helpers
{
    public static class AppointmentRules
    {
        public static int GetDurationMinutes(AppointmentType type)
        {
            switch (type)
            {
                case AppointmentType.BloodTest:
                    return 10;

                case AppointmentType.Checkup:
                    return 20;

                case AppointmentType.Consultation:
                    return 30;

                case AppointmentType.Emergency:
                    return 20;

                default:
                    return 20;
            }
        }

        public static int GetBreakMinutes(AppointmentType type)
        {
            switch (type)
            {
                case AppointmentType.BloodTest:
                    return 5;

                case AppointmentType.Checkup:
                    return 10;

                case AppointmentType.Consultation:
                    return 10;

                default:
                    return 0;
            }
        }

        public static int GetSlotInterval(AppointmentType type)
        {
            return GetDurationMinutes(type) + GetBreakMinutes(type);
        }
    }
}