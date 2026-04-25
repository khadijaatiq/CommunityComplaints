using CommunityComplaints.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services
builder.Services.AddControllersWithViews();

// ✅ Add DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ ADD SESSION
// FIX 6: Configure session with timeout and security options.
// Default AddSession() had no expiry and no HttpOnly flag.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;       // not accessible via JavaScript
    options.Cookie.IsEssential = true;    // required for GDPR compliance
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
});

var app = builder.Build();

// pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ✅ ADD THIS (missing)

app.UseRouting();

// ✅ ENABLE SESSION HERE
app.UseSession();

app.UseAuthorization();

// ✅ SET DEFAULT ROUTE TO ACCOUNT REGISTER
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Register}/{id?}");

app.Run();