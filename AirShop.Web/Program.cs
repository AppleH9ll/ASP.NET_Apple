using AirShop.Web.Data;
using AirShop.Web.Repositories;
using AirShop.Web.Services;
using AirShop.Web.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;

// Решение проблемы с датами PostgreSQL (Npgsql)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Console.OutputEncoding = System.Text.Encoding.UTF8;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Подключение к базе данных
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// === РЕГИСТРАЦИЯ СЛОЁВ (Scoped) ===
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// === 1. CORS ===
var allowedOrigin = builder.Configuration["CorsSettings:AllowedOrigin"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigin ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// === 2. RATE LIMITING (Защита от брутфорса) ===
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, // 5 попыток
                Window = TimeSpan.FromMinutes(1), // в минуту
                QueueLimit = 0
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// === 3. АУТЕНТИФИКАЦИЯ (JWT для API + Cookies для MVC) ===
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
    };
});

// === 4. AUTHORIZATION (Регистрация Resource Handler) ===
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, ReservationOwnerHandler>();

builder.Services.AddMemoryCache();

// === 5. SWAGGER С ПОДДЕРЖКОЙ JWT ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите JWT токен",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Новый синтаксис для Swashbuckle 10+ и Microsoft.OpenApi 2+
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Порядок Middleware строго важен!
app.UseRouting();

// CORS должен идти после UseRouting, но до UseAuthentication
app.UseCors("ReactPolicy");

app.UseStaticFiles();

// Rate Limiter
app.UseRateLimiter();

// Аутентификация и Авторизация
app.UseAuthentication();
app.UseAuthorization();

// Маппинг маршрутов
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();