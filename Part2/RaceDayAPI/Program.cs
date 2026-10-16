
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// REGISTER CONTROLLERS
// =====================================
builder.Services.AddControllers();

// =====================================
// CONNECT TO SQL SERVER DATABASE
// =====================================
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "RaceDayConnection")));

// =====================================
// CONFIGURE SESSION MANAGEMENT
// =====================================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;
    options.Cookie.SameSite =
        SameSiteMode.Lax;
});

// =====================================
// CONFIGURE AUTHENTICATION
// =====================================
builder.Services
    .AddAuthentication("RaceDayCookie")
    .AddCookie("RaceDayCookie", options =>
    {
        options.LoginPath = "/api/Auth/login";
        options.AccessDeniedPath = "/api/Auth/denied";

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// =====================================
// CONFIGURE SWAGGER
// =====================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================================
// BUILD APPLICATION
// =====================================
var app = builder.Build();

// =====================================
// ENABLE SWAGGER
// =====================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =====================================
// MIDDLEWARE
// =====================================
app.UseHttpsRedirection();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
