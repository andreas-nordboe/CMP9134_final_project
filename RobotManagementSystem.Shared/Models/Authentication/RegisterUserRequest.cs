using System.ComponentModel.DataAnnotations;

namespace RobotManagementSystem.Shared.Models.Authentication;

public class RegisterUserRequest
{
    [Required]
    [MinLength(5)]
    public string Username { get; set; }
    
    [Required]
    [MinLength(2)]
    public string FirstName { get; set; }
    
    [Required]
    [MinLength(10)]
    public string LastName { get; set; }
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; }
    
    // Password validation could possibly occur client-side instead of being sent to the server twice
    [Required]
    [MinLength(8)]
    public string ConfirmPassword { get; set; }
}