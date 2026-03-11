using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplicationASP.Models;
using System.Security.Claims;

namespace WebApplicationASP.Controllers
{
    public class ActivityController : Controller
    {
        private readonly AppDbContext _context;

        public ActivityController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult AddActivity()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> HandleAddActivity(Activity activity, IFormFile ImageFile)
        {
            // ✅ เช็คเวลา
            if (activity.ExpireDate <= DateTime.Now)
            {
                TempData["Error"] = "ไม่สามารถสร้างกิจกรรมได้: เวลาปิดรับต้องมากกว่าปัจจุบัน";
                return RedirectToAction("AddActivity");
            }

            var new_activity = activity;

            new_activity.Status = "open";
            new_activity.Member = [];

            new_activity.Host = User.Identity?.Name ?? "ไม่ระบุชื่อ";

            if (ImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);

                var uploadPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images",
                    fileName
                );

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                activity.ImageUrl = "/images/" + fileName;
            }
            else
            {
                activity.ImageUrl = "/images/default.jpg";
            }

            _context.Activities.Add(new_activity);
            await _context.SaveChangesAsync();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim != null)
            {
                var userId = int.Parse(userIdClaim);
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    user.CreatedActivityIds.Add(new_activity.Id);
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult GetActivityDetails(int id)
        {
            var activity = _context.Activities.FirstOrDefault(a => a.Id == id);
            if (activity == null) return NotFound(new { success = false, message = "ไม่พบข้อมูลกิจกรรม" });

            // ===============================================
            // เพิ่มการเช็คเวลาตรงนี้ ถ้าหมดเวลาแล้วให้เปลี่ยนเป็น Close เลย
            if (activity.Status == "open" && DateTime.Now >= activity.ExpireDate)
            {
                activity.Status = "close";
                _context.Activities.Update(activity);
                _context.SaveChanges(); // อัปเดตลง Database
            }
            // ===============================================

            // คำนวณจำนวนคนและที่ว่าง
            int currentMembers = activity.Member != null ? activity.Member.Count : 0;
            int emptySeats = activity.Number - currentMembers;

            // ดึงชื่อคนที่ล็อกอินอยู่
            var currentUsername = User.Identity?.Name;
            bool isHost = (activity.Host == currentUsername); // เช็คว่าเป็นเจ้าของหรือไม่

            // เพิ่มการเช็คว่าคนที่ล็อกอินอยู่สมัครหรือยัง
            bool hasJoined = false;
            if (!string.IsNullOrEmpty(currentUsername) && activity.Member != null)
            {
                hasJoined = activity.Member.Contains(currentUsername);
            }

            return Json(new
            {
                id = activity.Id, // ส่ง ID กลับไป
                isHost = isHost,  // ส่งสถานะความเป็นเจ้าของ
                hasJoined = hasJoined, // ส่งสถานะการสมัคร
                title = activity.Title,
                host = activity.Host ?? "ไม่ระบุชื่อ",
                description = activity.Description,

                // แก้ไขบรรทัดนี้: ปรับ format ให้แสดงวันที่และเวลาในหน้าป๊อปอัป
                expireDate = activity.ExpireDate.ToString("dd/MM/yyyy HH:mm น."),

                category = activity.Category != null ? string.Join(", ", activity.Category) : "ไม่มีหมวดหมู่",
                status = activity.Status,
                memberCount = currentMembers,
                emptySeats = emptySeats,
                maxParticipants = activity.Number,
                imageUrl = activity.ImageUrl,
                members = activity.Member ?? new List<string>()
            });
        }

        // 1. ฟังก์ชันรับ Request สำหรับ Join
        [HttpPost]
        [Authorize] // บังคับว่าต้อง Login ก่อนถึงจะกด Join ได้
        public async Task<IActionResult> JoinActivity([FromBody] JoinRequest request)
        {
            // ดึงชื่อคนที่ล็อกอินอยู่
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUsername))
            {
                return Json(new { success = false, message = "กรุณาล็อกอินก่อนเข้าร่วมกิจกรรม" });
            }

            // หากิจกรรมจาก Database
            var activity = await _context.Activities.FindAsync(request.Id);
            if (activity == null)
            {
                return Json(new { success = false, message = "ไม่พบกิจกรรมนี้" });
            }

            // ==========================================
            // เพิ่มใหม่: เช็คว่า ณ วินาทีที่กด Join หมดเวลาหรือยัง?
            // ==========================================
            if (DateTime.Now >= activity.ExpireDate || activity.Status.ToLower() == "close")
            {
                // ถ้าหมดเวลาแล้ว ให้เปลี่ยน Status เป็น close ในฐานข้อมูลทันที
                if (activity.Status != "close")
                {
                    activity.Status = "close";
                    _context.Activities.Update(activity);
                    await _context.SaveChangesAsync();
                }

                // ส่งข้อความกลับไปบอกหน้าเว็บว่าหมดเวลาแล้ว
                return Json(new { success = false, message = "เนื่องจากกิจกรรมนี้หมดเวลาหรือปิดรับสมัครแล้ว" });
            }
            // ==========================================

            // ถ้า Member ยังเป็น null ให้สร้าง List เปล่ามารองรับ
            if (activity.Member == null) activity.Member = new List<string>();

            // เช็คว่าผู้ใช้กด Join ไปแล้วหรือยัง
            if (activity.Member.Contains(currentUsername))
            {
                return Json(new { success = false, message = "คุณเข้าร่วมกิจกรรมนี้ไปแล้ว" });
            }

            // เช็คว่าที่นั่งเต็มหรือยัง
            if (activity.Member.Count >= activity.Number)
            {
                return Json(new { success = false, message = "กิจกรรมนี้เต็มแล้ว" });
            }

            // เพิ่มชื่อผู้ใช้ลงใน List รายชื่อคนสมัคร
            activity.Member.Add(currentUsername);

            if (activity.Member.Count >= activity.Number)
            {
                activity.Status = "close"; // เปลี่ยนสถานะเป็นปิดรับสมัคร
            }

            _context.Activities.Update(activity);
            var userId = int.Parse(userIdClaim);
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                if (!user.JoinedActivityIds.Contains(activity.Id))
                {
                    user.JoinedActivityIds.Add(activity.Id);
                    _context.Users.Update(user);
                }
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // 2. Class สำหรับรับข้อมูล JSON จาก Javascript
        public class JoinRequest
        {
            public int Id { get; set; }
        }

        // ฟังก์ชันดึงหน้าจอแก้ไข
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return NotFound();

            // เช็คว่าคนที่ล็อกอินอยู่ คือ Host ของกิจกรรมนี้หรือไม่
            if (activity.Host != User.Identity?.Name)
            {
                return RedirectToAction("Index", "Home"); // ถ้าไม่ใช่ให้เด้งกลับหน้าแรก
            }

            return View(activity);
        }

        // ฟังก์ชันบันทึกข้อมูลที่แก้ไข
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditActivity(Activity model, IFormFile? ImageFile)
        {
            var existingActivity = await _context.Activities.FindAsync(model.Id);
            if (existingActivity == null) return NotFound();

            // เช็คความปลอดภัยอีกรอบ ป้องกันคนแฮ็กยิง API ตรงๆ
            if (existingActivity.Host != User.Identity?.Name)
            {
                return RedirectToAction("Index", "Home");
            }

            // อัปเดตข้อมูล
            existingActivity.Title = model.Title;
            existingActivity.Description = model.Description;
            existingActivity.ExpireDate = model.ExpireDate;
            existingActivity.Number = model.Number;
            existingActivity.Category = model.Category;
            existingActivity.Status = model.Status;

            // ถ้ามีการอัปโหลดรูปภาพใหม่
            if (ImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                existingActivity.ImageUrl = "/images/" + fileName;
            }

            _context.Activities.Update(existingActivity);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}