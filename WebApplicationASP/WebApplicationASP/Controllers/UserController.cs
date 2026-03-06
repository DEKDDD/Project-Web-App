// using Microsoft.AspNetCore.Mvc;
// using WebApplicationASP.Models;

// public class UserController : Controller
// {
//     private readonly AppDbContext _context;

//     public UserController(AppDbContext context)
//     {
//         _context = context;
//     }

//     public IActionResult CreateTestUser()
//     {
//         var user = new User
//         {
//             Username = "tester01",
//             Email = "tester01@gmail.com",
//             Password = "tester01"
//         };

//         _context.Users.Add(user);
//         _context.SaveChanges();

//         return Content("User created!");
//     }
// }