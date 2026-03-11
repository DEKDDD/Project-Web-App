using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApplicationASP.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
    });

builder.Services.AddAuthorization();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

Console.WriteLine(Directory.GetCurrentDirectory());

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated(); 

    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new User { Username = "Admin01", Email = "admin@mail.com", Password = "123" },
            new User { Username = "นักเตะแข้งทอง", Email = "1@mail.com", Password = "123" },
            new User { Username = "นักร้องเสียง...", Email = "2@mail.com", Password = "123" },
            new User { Username = "นักกินมืออาชีพ", Email = "3@mail.com", Password = "123" },
            new User { Username = "แค่คนเหงาๆ", Email = "4@mail.com", Password = "123" }
        );
        context.SaveChanges();
    }

    if (!context.Activities.Any())
    {
        var host1 = context.Users.FirstOrDefault(u => u.Username == "Admin01");
        var user1 = context.Users.FirstOrDefault(u => u.Username == "นักเตะแข้งทอง");
        var user2 = context.Users.FirstOrDefault(u => u.Username == "นักร้องเสียง...");
        var user3 = context.Users.FirstOrDefault(u => u.Username == "นักกินมืออาชีพ");
        var user4 = context.Users.FirstOrDefault(u => u.Username == "แค่คนเหงาๆ");

        if (host1 != null && user1 != null && user2 != null && user3 != null && user4 != null)
        {
            context.Activities.AddRange(
                new Activity { 
                    Title = "To much To fun", 
                    Host = host1.Username, 
                    Description = "ทดสอบคนเยอะ", 
                    ExpireDate = DateTime.Now.AddDays(14), 
                    Number = 999, 
                    Category = ["sport"], 
                    Status = "open", 
                    ImageUrl = "/images/default.jpg", 
                    Member = [host1.Username, user1.Username, user2.Username, user3.Username, user4.Username]
                },
                new Activity { 
                    Title = "หาคนเตะบอลหลังเลิกเรียน", 
                    Host = user1.Username, 
                    Description = "ขาดโกลตึงๆ 1 ตำแหน่งครับ ตำแหน่งอื่นก็มาจอยเอาสนุกได้ครับ \nเตะที่สนามหญ้าเทียมข้างม. พรุ่งนี้เย็น", 
                    ExpireDate = DateTime.Now.AddDays(1), 
                    Number = 10, 
                    Category = ["sport"], 
                    Status = "open", 
                    ImageUrl = "https://img5.pic.in.th/file/secure-sv1/footballfcefd13ad3c12055.jpg",
                    Member = []
                },
                new Activity { 
                    Title = "หาเพื่อนร้องเกะะะ", 
                    Host = user2.Username, 
                    Description = "ที่ห้องสมุดม. มาจอยกันๆๆ", 
                    ExpireDate = DateTime.Now.AddDays(7), 
                    Number = 8, 
                    Category = ["music"], 
                    Status = "open",
                    ImageUrl = "/images/default.jpg",
                    Member = [user1.Username, user3.Username, user4.Username] 
                },
                new Activity { 
                    Title = "เจอกัน!! ชาบูเปิดใหม่หน้าม.", 
                    Host = user3.Username,
                    Description = "[โปรโมชั่นมา 4 จ่าย 3 !!] \nตอนนี้มีแล้ว 2 คน ขาดอีก 2 คนครับ \nไปกินเย็นนี้เลย หิวมากๆๆๆ", 
                    ExpireDate = DateTime.Now.AddHours(1),
                    Number = 2, 
                    Category = ["food"], 
                    Status = "open", 
                    ImageUrl = "https://img2.pic.in.th/shabu.jpg", 
                    Member = [user4.Username] 
                },
                new Activity { 
                    Title = "หาเพื่อนเดินตลาดนัด", 
                    Host = user4.Username,
                    Description = "เดินเล่นหาของกินชิลๆ เย็นนี้ขอสักคน🙏", 
                    ExpireDate = DateTime.Now.AddHours(4),
                    Number = 1,
                    Category = ["food"],
                    Status = "open", 
                    ImageUrl = "https://img2.pic.in.th/market.jpg",
                    Member = []
                }
            );
            context.SaveChanges();
        }
    }
}

app.Run();