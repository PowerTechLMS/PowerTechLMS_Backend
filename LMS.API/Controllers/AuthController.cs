using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            return Ok(await _authService.LoginAsync(request));
        } catch(UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            return Ok(await _authService.RegisterAsync(request));
        } catch(InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
