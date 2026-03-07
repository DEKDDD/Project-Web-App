using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApplicationASP.Models; // อย่าลืมใส่ Namespace ของ Models และ DbContext
using System.Linq;

[Authorize]
public class UserController : Controller
{
    private readonly AppDbContext _context;

    // Constructor เพื่อดึง Database Context มาใช้งาน
    public UserController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Profile()
    {
        // 1. ดึงชื่อ Username จากคนที่ Login อยู่
        var currentUserName = User.Identity?.Name;

        // 2. ไปหาใน Database ว่า User คนนี้มีข้อมูลอะไรบ้าง
        // สมมติว่าใน AppDbContext คุณมีตาราง Users หรือ Users Profile
        var user = _context.Users.FirstOrDefault(u => u.Username == currentUserName);

        if (user == null)
        {
            return NotFound("ไม่พบข้อมูลผู้ใช้งาน");
        }

        // 3. ส่งข้อมูล User ไปยัง View
        return View(user);
    }

    public IActionResult Menu()
    {
        return View();
    }
}