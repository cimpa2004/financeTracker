var builder = WebApplication.CreateBuilder(args);

// Minimal services: keep Swagger for API exploration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Simple health/root endpoint (optional)
app.MapGet("/", () => "OK");

app.Run();
