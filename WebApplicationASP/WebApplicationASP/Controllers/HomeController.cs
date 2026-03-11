using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplicationASP.Models;

namespace WebApplicationASP.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    public HomeController(AppDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        var activities = _context.Activities.ToList();
        var expired = activities
            .Where(a => DateTime.Now >= a.ExpireDate && a.Status != "close");
        foreach (var activity in expired)
        {
            activity.Status = "close";
        }
        _context.SaveChanges();

        return View(activities);
    }

    public IActionResult Privacy()
    {
        return View();
    }

}
