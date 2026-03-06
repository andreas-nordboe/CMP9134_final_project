using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Services.Security;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GenerateJWTToken(AuthenticationTokenDTO generateTokenRequest)
    {
        try
        {
            // Gets the secret key from AppSettings.json
            string? secretKey = _configuration["JWTConfiguration:SecretKey"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            // Create Claims
            var claimUserName = new Claim(ClaimTypes.Email, generateTokenRequest.UserId);
            var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, generateTokenRequest.UserId);

            // Create claimsIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimUserName, claimNameIdentifier, }, "JWTAuth");

            // Generate token that is valid for 15 minutes
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                // TODO Improve stability in case AppSettings field is changed or file is non-existent 
                Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JWTConfiguration:AccessTokenExpiryTimeInMins")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
            };

            // Create a  token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            Console.WriteLine(token);

            // Return the token back to the caller
            return tokenHandler.WriteToken(token);
        }
        catch (Exception e)
        {
            return string.Empty; // Returning an empty string for now so it can be checked by the caller
        }
    }
}