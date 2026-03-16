using Microsoft.AspNetCore.Mvc;
using RobotManagementSystem.Shared.Models;

namespace RobotManagementSystem.Controllers;

// POST /auth/login
// POST /auth/register
// GET /auth/account

// Sends robot actions by forwarding requests to the commands service


[ApiController]
[Route("auth/")]
public class AuthenticationController : ControllerBase
{
    [HttpGet("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login()
    {

        var response = new AuthenticationResponse()
        {
            Token = "Todo_Token",
            Role =  "Admin"
        };

        return Ok(response);
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponse>> Register()
    {

        var response = new AuthenticationResponse()
        {
            Token = "Todo_Token",
            Role =  "Admin"
        };

        return Ok(response);
    }
}
