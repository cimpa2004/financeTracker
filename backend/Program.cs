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
var jwtKey = builder.Configuration["Jwt:Key"] ?? "change_this_to_a_secure_key";
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
////log all category IDs to console (for testing)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinancetrackerContext>();
    var categoryIds = db.Categories.Select(c => c.CategoryId).ToList();

    Console.WriteLine("Category IDs (count: {0}):", categoryIds.Count);
    if (categoryIds.Count == 0)
    {
        Console.WriteLine("(no categories found)");
    }
    else
    {
        foreach (var id in categoryIds)
        {
            Console.WriteLine(id);
        }
    }
}

////

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
app.MapTransactions();

app.Run();
