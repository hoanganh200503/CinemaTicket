using CinemaTicketApp.Data;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.Data;
using CinemaTicket.Services;
using CinemaTicketApp.Service;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Kết nối đến database có sẵn
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<BookingService>();
builder.Services.AddDbContext<CinemaTicketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// khai báo dataconection trong admin area
builder.Services.AddDbContext<CinemaTicketAdmin.Data.CinemaTicketAdminDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// khai báo dataconection trong admin area



// 🔥 Thêm MVC vào container
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<EmailService>();
var app = builder.Build();

// 🔥 Middleware pipeline    
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.UseSession();
// 🔥Thêm Map route cho Admin Area
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=AdminPage}/{id?}");//{id?}
// 🔥Thêm Map route cho Admin Area

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
