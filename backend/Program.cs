using backend.apis;
using backend.Models;
using backend.services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Add DbContext
builder.Services.AddDbContext<FinancetrackerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
                         "Server=(localdb)\\MSSQLLocalDB;Database=financetracker;Trusted_Connection=True;MultipleActiveResultSets=true;"));
#endregion

// register JwtService
builder.Services.AddSingleton<JwtService>();

// -- ADD: configure JWT authentication --
// ensure you have a secret in configuration: "Jwt:Key"
var jwtKey = builder.Configuration["Jwt:Key"] ?? "very_secure_key_for_development_purposes_only";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      options.RequireHttpsMetadata = false;
      options.SaveToken = true;
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
      };
    });

// register authorization services required by app.UseAuthorization()
builder.Services.AddAuthorization();

// allow CORS for frontend
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontend", policy =>
  {
    policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
  });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// -- ADD: authentication and authorization middleware (order matters) --
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "OK");
app.MapRegister();
app.MapLogin();
app.MapLogout();
app.MapTransactions();
app.MapNewToken();
app.MapSubscriptions();
app.MapCategories();

app.MapSpentThisMonth();

app.Run();
