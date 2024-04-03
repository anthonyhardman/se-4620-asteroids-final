using signalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:8080") // Add any other origins your client might use
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()); // Allows credentials like cookies, authorization headers, etc.
});

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapHub<AsteroidsHub>("/ws");

app.Run();
