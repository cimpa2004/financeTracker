using backend.apis;
using System.Reflection;
using System.Linq;
using QuestPDF;
using backend.Models;
using backend.services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

// Load .env file (backend/.env) into environment variables for local development
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
  var lines = await File.ReadAllLinesAsync(envPath);
  foreach (var line in lines)
  {
    var trimmed = line.Trim();
    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
      continue;
    var idx = trimmed.IndexOf('=');
    if (idx <= 0)
      continue;
    var key = trimmed.Substring(0, idx).Trim();
    var val = trimmed.Substring(idx + 1).Trim();
    // remove optional surrounding quotes
    if ((val.StartsWith('"') && val.EndsWith('"')) || (val.StartsWith('\'') && val.EndsWith('\'')))
      val = val.Substring(1, val.Length - 2);
    Environment.SetEnvironmentVariable(key, val);
  }
}

var builder = WebApplication.CreateBuilder(args);

#region Add DbContext
builder.Services.AddDbContext<FinancetrackerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
                         "Server=(localdb)\\MSSQLLocalDB;Database=financetracker;Trusted_Connection=True;MultipleActiveResultSets=true;"));
#endregion

// register JwtService
builder.Services.AddSingleton<JwtService>();
// register Mailjet email service. Configure Email:Mailjet:ApiKey, Email:Mailjet:ApiSecret and Email:From
builder.Services.AddHttpClient("mailjet");
builder.Services.AddSingleton<backend.services.IEmailService, backend.services.MailjetEmailService>();

// register report service
builder.Services.AddScoped<backend.services.ReportService>();

// background service to auto-renew budget periods (weekly/monthly/yearly)
builder.Services.AddHostedService<backend.services.BudgetPeriodRenewalService>();

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

// Configure QuestPDF license (set to Community) using reflection so this works across QuestPDF versions.
try
{
  var settingsType = Type.GetType("QuestPDF.Settings, QuestPDF") ?? (typeof(object).Assembly.GetType("QuestPDF.Settings") ?? typeof(object));
  if (settingsType != null && settingsType.FullName == "QuestPDF.Settings")
  {
    var asm = settingsType.Assembly;
    var enumType = asm.GetTypes().FirstOrDefault(t => t.IsEnum && t.GetFields(BindingFlags.Public | BindingFlags.Static).Any(f => f.Name == "Community"));
    if (enumType != null)
    {
      var community = Enum.Parse(enumType, "Community");
      var licenseProp = settingsType.GetProperty("License", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      if (licenseProp != null && licenseProp.PropertyType == enumType)
      {
        licenseProp.SetValue(null, community);
      }
      else
      {
        var licenseField = settingsType.GetField("License", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (licenseField != null && licenseField.FieldType == enumType)
        {
          licenseField.SetValue(null, community);
        }
      }
    }
  }
}
catch
{
  // If we can't set the license here, the QuestPDF library will raise a clear exception at PDF generation time.
}

app.MapGet("/", () => "OK");
app.MapRegister();
app.MapLogin();
app.MapLogout();
app.MapUser();
app.MapTransactions();
app.MapNewToken();
app.MapSubscriptions();
app.MapCategories();
app.MapBudgets();
app.MapStatistics();

app.MapSpentThisMonth();
app.MapReports();

await app.RunAsync();
