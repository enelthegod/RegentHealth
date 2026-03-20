using System.ComponentModel.DataAnnotations.Schema;

namespace RegentHealth.Models
{
    public enum UserRole
    {
        Admin,
        Patient,
        Doctor
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        [NotMapped]
        public string FullName => $"{Name} {Surname}";
    }
}