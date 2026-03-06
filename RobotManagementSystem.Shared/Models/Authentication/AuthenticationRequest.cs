using System.ComponentModel.DataAnnotations;

namespace RobotManagementSystem.Shared.Models.Authentication;

// Note: Validation decorators may be removed and be replaced by a custom validation class later for greater modularity
public class AuthenticationRequest
{
    //[Required]
    //[MinLength(5)]
    public string Username { get; set; }

    //[Required]
    //[DataType(DataType.Password)]
    //[MinLength(8)]
    public string Password { get; set; }
}