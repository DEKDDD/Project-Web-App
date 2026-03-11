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

            if (activity.Status == "open" && DateTime.Now >= activity.ExpireDate)
            {
                activity.Status = "close";
                _context.Activities.Update(activity);
                _context.SaveChanges();
            }

            int currentMembers = activity.Member != null ? activity.Member.Count : 0;
            int emptySeats = activity.Number - currentMembers;

            var currentUsername = User.Identity?.Name;
            bool isHost = (activity.Host == currentUsername);

            bool hasJoined = false;
            if (!string.IsNullOrEmpty(currentUsername) && activity.Member != null)
            {
                hasJoined = activity.Member.Contains(currentUsername);
            }

            return Json(new
            {
                id = activity.Id,
                isHost = isHost,
                hasJoined = hasJoined,
                title = activity.Title,
                host = activity.Host ?? "ไม่ระบุชื่อ",
                description = activity.Description,

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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> JoinActivity([FromBody] JoinRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUsername))
            {
                return Json(new { success = false, message = "กรุณาล็อกอินก่อนเข้าร่วมกิจกรรม" });
            }

            var activity = await _context.Activities.FindAsync(request.Id);
            if (activity == null)
            {
                return Json(new { success = false, message = "ไม่พบกิจกรรมนี้" });
            }

            if (DateTime.Now >= activity.ExpireDate || activity.Status.ToLower() == "close")
            {
                if (activity.Status != "close")
                {
                    activity.Status = "close";
                    _context.Activities.Update(activity);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = false, message = "เนื่องจากกิจกรรมนี้หมดเวลาหรือปิดรับสมัครแล้ว" });
            }

            if (activity.Member == null) activity.Member = new List<string>();

            if (activity.Member.Contains(currentUsername))
            {
                return Json(new { success = false, message = "คุณเข้าร่วมกิจกรรมนี้ไปแล้ว" });
            }

            if (activity.Member.Count >= activity.Number)
            {
                return Json(new { success = false, message = "กิจกรรมนี้เต็มแล้ว" });
            }

            activity.Member.Add(currentUsername);

            if (activity.Member.Count >= activity.Number)
            {
                activity.Status = "close";
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

        public class JoinRequest
        {
            public int Id { get; set; }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return NotFound();

            if (activity.Host != User.Identity?.Name)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(activity);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditActivity(Activity model, IFormFile? ImageFile)
        {
            var existingActivity = await _context.Activities.FindAsync(model.Id);
            if (existingActivity == null) return NotFound();

            if (existingActivity.Host != User.Identity?.Name)
            {
                return RedirectToAction("Index", "Home");
            }

            existingActivity.Title = model.Title;
            existingActivity.Description = model.Description;
            existingActivity.ExpireDate = model.ExpireDate;
            existingActivity.Number = model.Number;
            existingActivity.Category = model.Category;
            existingActivity.Status = model.Status;

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