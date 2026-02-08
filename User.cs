using System;

public class User
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public required string Surname { get; set; }
	public required string Password { get; set; }
	public required string Email { get; set; }

	public UserRole Role { get; set; }
}

public enum UserRole
{
    Admin,
    Patient,
	Doctor
}
