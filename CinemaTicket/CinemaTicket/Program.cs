using Microsoft.EntityFrameworkCore;
using CinemaTicket.Data;
using CinemaTicket.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Kết nối đến database có sẵn
builder.Services.AddScoped<BookingService>();
builder.Services.AddDbContext<CinemaTicketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
