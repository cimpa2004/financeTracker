using backend.apis;
using backend.Models; // use the generated FinancetrackerContext
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Add DbContext
// register the generated FinancetrackerContext with DI so it can be injected
builder.Services.AddDbContext<FinancetrackerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
                         "Server=(localdb)\\MSSQLLocalDB;Database=financetracker;Trusted_Connection=True;MultipleActiveResultSets=true;"));
#endregion

// allow requests from your Vite dev server
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

app.MapGet("/", () => "OK");
app.MapRegister();

app.Run();
