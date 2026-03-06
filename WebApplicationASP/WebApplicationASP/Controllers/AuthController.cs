using Microsoft.AspNetCore.Mvc;
using WebApplicationASP.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Signup
   [HttpPost("signup")]
    public IActionResult Signup([FromBody] Signup signup)
    {
        if (_context.Users.Any(u => u.Email == signup.Email))
        {
            return BadRequest("Email already used");
        }

        var user = new User
        {
            Username = signup.Username,
            Email = signup.Email,
            Password = signup.Password
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("Signup successful");
    }

    // ✅ Login
    [HttpPost("login")]
    public IActionResult Login([FromBody] Login login)
    {
        var user = _context.Users.FirstOrDefault(u =>
            u.Email == login.Email && u.Password == login.Password);

        if (user == null)
            return Unauthorized("Invalid email or password");

        return Ok("Login successful");
    }
}