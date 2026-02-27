using System;
using System.Collections.Generic;
using System.Text;

namespace RegentHealth.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        // connection with User
        public int UserId { get; set; }

        public string Specialization { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
