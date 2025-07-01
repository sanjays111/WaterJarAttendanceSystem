using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WaterJarAttendanceSystem.Data;
using WaterJarAttendanceSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load configuration
var configuration = builder.Configuration;

// Read environment variables (for production)
var defaultConnection = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
if (!string.IsNullOrWhiteSpace(defaultConnection))
{
    configuration["ConnectionStrings:DefaultConnection"] = defaultConnection;
}

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
if (!string.IsNullOrWhiteSpace(jwtKey)) configuration["Jwt:Key"] = jwtKey;
if (!string.IsNullOrWhiteSpace(jwtIssuer)) configuration["Jwt:Issuer"] = jwtIssuer;
if (!string.IsNullOrWhiteSpace(jwtAudience)) configuration["Jwt:Audience"] = jwtAudience;

// ✅ Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// ✅ Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ✅ JWT Auth
var jwtSettings = configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero
    };
});

// ✅ CORS
var frontendUrl = configuration["Frontend:Url"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(frontendUrl!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ✅ Controllers
builder.Services.AddControllers();

var app = builder.Build();

// ✅ Support for Heroku dynamic ports
if (!app.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    app.Urls.Add($"http://*:{port}");
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ✅ Simple default endpoint
app.MapGet("/", () => "✅ Water Jar Attendance API is running.");

app.Run();
