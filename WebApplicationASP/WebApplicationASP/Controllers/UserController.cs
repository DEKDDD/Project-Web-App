using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApplicationASP.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

[Authorize]
public class UserController : Controller
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var currentUserName = User.Identity?.Name;

        var user = _context.Users.FirstOrDefault(u => u.Username == currentUserName);

        if (user == null)
        {
            return NotFound("ไม่พบข้อมูลผู้ใช้งาน");
        }

        var createdActivities = _context.Activities
            .Where(a => user.CreatedActivityIds.Contains(a.Id))
            .ToList();

        var joinedActivities = _context.Activities
            .Where(a => user.JoinedActivityIds.Contains(a.Id))
            .ToList();

        ViewBag.CreatedActivities = createdActivities;
        ViewBag.JoinedActivities = joinedActivities;

        return View(user);
    }
    [HttpPost]
    public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
    {
        if (profileImage == null || profileImage.Length == 0)
            return Json(new { success = false, message = "กรุณาเลือกรูปภาพ" });

        try {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
            if (string.IsNullOrEmpty(userIdString)) {
                return Json(new { success = false, message = "ไม่พบข้อมูลการล็อกอินจาก Cookie" });
            }

            int userId = int.Parse(userIdString);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/profiles");
        
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            var uploadPath = Path.Combine(uploadFolder, fileName);
            using (var stream = new FileStream(uploadPath, FileMode.Create)) {
                await profileImage.CopyToAsync(stream);
            }

            var user = _context.Users.Find(userId);
            if (user != null) {
                user.ProfileImage = "/uploads/profiles/" + fileName;
                _context.Update(user);
                await _context.SaveChangesAsync();
                var identity = (ClaimsIdentity)User.Identity;
                var existingClaim = identity.FindFirst("ProfileImage");
                if (existingClaim != null)
                {
                    identity.RemoveClaim(existingClaim);
                }
                identity.AddClaim(new Claim("ProfileImage", user.ProfileImage));
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties { IsPersistent = true }
                );
                return Json(new { success = true, newImageUrl = user.ProfileImage });
            }

            return Json(new { success = false, message = "ไม่พบผู้ใช้ในฐานข้อมูล" });
        }
        catch (Exception ex) {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }
    public IActionResult Menu()
    {
        return View();
    }
}