using Microsoft.EntityFrameworkCore;
using CinemaTicket.Data;
using CinemaTicketApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Kết nối đến database có sẵn
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<CinemaTicketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔥 Thêm MVC vào container
builder.Services.AddControllersWithViews();

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

app.MapControllerRoute( // 🔥 Map route cho Admin Area
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=AdminPage}/{id?}");//{id?}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");//{id?}

app.Run();
