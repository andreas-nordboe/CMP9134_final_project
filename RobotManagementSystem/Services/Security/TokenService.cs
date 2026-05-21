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
            // Gets the secret key from AppSettings.jsonTje
            string secretKey = _configuration["JWTConfiguration:SecretKey"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            // Create Claims
            var claimUserName = new Claim(ClaimTypes.Name, generateTokenRequest.Username);
            var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, generateTokenRequest.UserId.ToString());
            var claimRole = new Claim(ClaimTypes.Role, generateTokenRequest.Role.ToString());

            // Create claimsIdentity
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                claimUserName, 
                claimNameIdentifier,
                claimRole
            });

            // Generate an access token that is valid for 15 minutes
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                // TODO Improve stability in case AppSettings field is changed or file is non-existent 
                Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JWTConfiguration:AccessTokenExpiryTimeInMins")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JWTConfiguration:Issuer"],
                Audience = _configuration["JWTConfiguration:Audience"]
            };

            // Create a token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the token back to the caller
            return tokenHandler.WriteToken(token);
        }
        catch (Exception e)
        {
            return string.Empty; // Returning an empty string for now so it can be checked by the caller
        }
    }

    public DateTime GetAccessTokenExpiryTime()
    {
        int expiryTimeInMins = _configuration.GetValue<int>("JWTConfiguration:AccessTokenExpiryTimeInMins");
        
        return DateTime.UtcNow.AddMinutes(expiryTimeInMins);
    }
}