using System.IdentityModel.Tokens.Jwt;

namespace RobotManagementSystem.Client.Helpers;

public static class JWTHelper
{
    public static bool IsAccessTokenExpired(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return true;
        
        // 1 minute buffer to log user out early so requests won't fail
        return GetTokenExpiryTime(accessToken) < DateTime.UtcNow.AddMinutes(1); 
    }
    
    public static DateTime GetTokenExpiryTime(string accessToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        return jwt.ValidTo;
    }
}