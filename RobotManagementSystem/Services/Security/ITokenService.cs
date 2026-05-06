using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Services.Security;

// The system incorporates token provisioning as a service to increase modularity
// for example by changing token provisioning algorithm later and also more easily fix security vulnerabilities
public interface ITokenService
{
    public string GenerateJWTToken(AuthenticationTokenDTO generateTokenRequest);
    public DateTime GetAccessTokenExpiryTime();
}