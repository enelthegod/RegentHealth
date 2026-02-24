using System;
using System.Collections.Generic;
using System.Linq;
using RegentHealth.Enums;
namespace RegentHealth.Helpers
{
    public static class EnumDisplayHelper
    {
        public static List<string> GetTimeSlots()
        {
            return Enum.GetNames(typeof(TimeSlot))
                .Select(name =>
                    name.Replace("Slot", "")
                        .Replace("_", ":"))
                .ToList();
        }
    }
}

