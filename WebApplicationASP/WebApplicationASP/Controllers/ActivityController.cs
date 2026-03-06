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

        public IActionResult Index()
        {
            var activities = _context.Activities.ToList();
            return View(activities);
        }
    }
}