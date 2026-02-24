using System;

public class User
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public required string Surname { get; set; }
	public required string PasswordHash { get; set; }
	public required string Email { get; set; }

	public string FullName => $"{Name} {Surname}";
	public UserRole Role { get; set; }
}

public enum UserRole
{
    Admin,
    Patient,
	Doctor
}
