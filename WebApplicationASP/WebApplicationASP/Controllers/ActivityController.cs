using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplicationASP.Models;

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
        public async Task<IActionResult> HandleAddActivity(Activity activity, IFormFile ImageFile)
        {
            var new_activity = activity;
            new_activity.Status = "open";
            new_activity.Member = [];
            
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

            return RedirectToAction("Index","Home");
        }
    }
}