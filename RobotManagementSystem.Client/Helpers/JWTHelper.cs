using System.IdentityModel.Tokens.Jwt;

namespace RobotManagementSystem.Client.Helpers;

public static class JWTHelper
{
    public static bool IsAccessTokenExpired(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return true;
        
        
        return GetTokenExpiryTime(accessToken) < DateTime.UtcNow; 
    }
    
    public static DateTime GetTokenExpiryTime(string accessToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        return jwt.ValidTo;
    }
}