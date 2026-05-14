// AuthController.cs
using Microsoft.AspNetCore.Mvc;
using SmartLeave.Application.DTOs.Auth;
using SmartLeave.Application.Interfaces;

namespace SmartLeave.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {

        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {

        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }
}