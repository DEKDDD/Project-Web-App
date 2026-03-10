using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApplicationASP.Models; // อย่าลืมใส่ Namespace ของ Models และ DbContext
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

[Authorize]
public class UserController : Controller
{
    private readonly AppDbContext _context;

    // Constructor เพื่อดึง Database Context มาใช้งาน
    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Profile() // เปลี่ยนเป็น async เพื่อประสิทธิภาพ
    {
        var currentUserName = User.Identity?.Name;

        // 1. ดึงข้อมูล User (รวมถึงพวก List ของ ID ที่เราสร้างไว้)
        var user = _context.Users.FirstOrDefault(u => u.Username == currentUserName);

        if (user == null)
        {
            return NotFound("ไม่พบข้อมูลผู้ใช้งาน");
        }

        // 2. ดึงข้อมูล "กิจกรรมที่ฉันสร้าง" (ดึงข้อมูลเต็มๆ จากตาราง Activities)
        var createdActivities = _context.Activities
            .Where(a => user.CreatedActivityIds.Contains(a.Id))
            .ToList();

        // 3. ดึงข้อมูล "กิจกรรมที่ฉันเข้าร่วม"
        var joinedActivities = _context.Activities
            .Where(a => user.JoinedActivityIds.Contains(a.Id))
            .ToList();

        // 4. ส่งข้อมูลกิจกรรมไปที่หน้า View ผ่าน ViewBag
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
            // 1. ดึง UserId จาก Cookie (Claims)
            // .Value จะได้ค่า string ออกมา แล้วเราค่อยแปลงเป็น int
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
            if (string.IsNullOrEmpty(userIdString)) {
                return Json(new { success = false, message = "ไม่พบข้อมูลการล็อกอินจาก Cookie" });
            }

            int userId = int.Parse(userIdString);

            // 2. บันทึกรูป (โค้ดเดิมที่คุณทำได้แล้ว)
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/profiles");
        
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            var uploadPath = Path.Combine(uploadFolder, fileName);
            using (var stream = new FileStream(uploadPath, FileMode.Create)) {
                await profileImage.CopyToAsync(stream);
            }

            // 3. อัปเดตฐานข้อมูล
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
                    new AuthenticationProperties { IsPersistent = true } // ให้จำสถานะล็อกอินไว้เหมือนเดิม
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