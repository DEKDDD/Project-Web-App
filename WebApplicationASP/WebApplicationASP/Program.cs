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
            new User { Username = "นักเตะแข้งทอง", Email = "1@gmail.com", Password = "123" },
            new User { Username = "นักร้องเสียง...", Email = "2@gmail.com", Password = "123" },
            new User { Username = "นักกินมืออาชีพ", Email = "3@gmail.com", Password = "123" },
            new User { Username = "แค่คนเหงาๆ", Email = "4@gmail.com", Password = "123" }
        );
        context.SaveChanges();
    }
}

app.Run();