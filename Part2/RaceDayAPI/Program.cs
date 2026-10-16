
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using System;
using System.IO;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// REGISTER API CONTROLLERS
// =====================================
builder.Services.AddControllers();

// =====================================
// SQL SERVER DATABASE CONNECTION
// =====================================
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "RaceDayConnection")));

// =====================================
// CONFIGURE SERVER-SIDE SESSIONS
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
// CONFIGURE COOKIE AUTHENTICATION
// =====================================
builder.Services
    .AddAuthentication("RaceDayCookie")
    .AddCookie("RaceDayCookie", options =>
    {
        options.LoginPath = "/api/Auth/login";

        options.AccessDeniedPath = "/api/Auth/denied";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(30);

        options.SlidingExpiration = false;

        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        // Return 401 for unauthenticated API requests
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        };

        // Return 403 for forbidden API requests
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return Task.CompletedTask;
        };
    });

// =====================================
// ENABLE ROLE-BASED AUTHORIZATION
// =====================================
builder.Services.AddAuthorization();

// =====================================
// CONFIGURE SWAGGER AND XML DOCUMENTATION
// =====================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Locate generated API documentation
    var xmlFile =
        $"{typeof(Program).Assembly.GetName().Name}.xml";

    var xmlPath = Path.Combine(
        AppContext.BaseDirectory,
        xmlFile);

    // Include XML comments in Swagger
    options.IncludeXmlComments(xmlPath);
});

// =====================================
// BUILD APPLICATION
// =====================================
var app = builder.Build();

// =====================================
// ENABLE SWAGGER IN DEVELOPMENT
// =====================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =====================================
// CONFIGURE REQUEST MIDDLEWARE
// =====================================
app.UseHttpsRedirection();

// Enable sessions before controllers
app.UseSession();

// Authenticate the logged-in user
app.UseAuthentication();

// Enforce access permissions
app.UseAuthorization();

// Register API endpoints
app.MapControllers();

// =====================================
// START RACEDAY API
// =====================================
app.Run();
