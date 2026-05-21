using System.ComponentModel.DataAnnotations;

namespace RobotManagementSystem.Shared.Models.Authentication;

// Note: Validation decorators may be removed and be replaced by a custom validation class later for greater modularity
public class AuthenticationRequest
{
    [Required]
    //[MinLength(5)] TODO refactor later
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    //[MinLength(8)] TODO refactor later
    public string Password { get; set; } = string.Empty;
}