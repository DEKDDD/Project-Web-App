using Microsoft.AspNetCore.Mvc;
using WebApplicationASP.Models;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

[Route("auth")]
public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("login")]
    public IActionResult LoginPage()
    {
        return View("login");
    }

    [HttpGet("signup")]
    public IActionResult Signup()
    {
        return View();
    }


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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login login)
    {
        var user = _context.Users.FirstOrDefault(u =>
            u.Email == login.Email && u.Password == login.Password);

        if (user == null)
            return Unauthorized("Invalid email or password");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("ProfileImage", user.ProfileImage ?? "/images/default.jpg"),
            new Claim("UserId", user.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok("Login successful");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}