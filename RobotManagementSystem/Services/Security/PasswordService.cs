namespace RobotManagementSystem.Services.Security;
using Microsoft.AspNetCore.Identity;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }
    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return _passwordHasher.VerifyHashedPassword(null!, hashedPassword, password) is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
    
    // Unit test for later
    
    /*[Fact]
    public void HashPassword_ShouldNotReturnPlainTextPassword()
    {
        var service = new PasswordService();

        var hash = service.HashPassword("SimplePasswordToCheck!");
        
        Assert.NotEqual("SimplePasswordToCheck!", hash);
    }*/
}