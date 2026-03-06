using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class UserController : Controller
{
    public IActionResult Menu()
    {
        return View();
    }

    public IActionResult Profile()
    {
        return Content("Profile page coming soon");
    }
}