using E_Commerce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers
builder.Services.AddControllers();

// 2. CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. MySQL Connection
var connectionString =
    builder.Configuration.GetConnectionString("MyConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "MyConnection is missing from appsettings.json.");
}

builder.Services.AddDbContext<ECommerceDBcontext>(options =>
    options.UseMySQL(connectionString)
);

// 4. JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                ),

                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

// 5. Authorization
builder.Services.AddAuthorization();

// 6. Swagger and OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// IMPORTANT: Build AFTER all service registrations
var app = builder.Build();

// 7. Development Environment
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 8. Middleware
app.UseHttpsRedirection();

app.UseStaticFiles();

// CORS before Authentication and Authorization
app.UseCors("AllowAngular");

app.UseAuthentication();

app.UseAuthorization();

// 9. Map Controllers
app.MapControllers();

app.Run();