using actorSystem;
using actorSystem.Services;
using Akka.DependencyInjection;
using Akka.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ICommunicationService, CommunicationService>();
builder.Services.AddSingleton<IActorBridge, AkkaService>();
builder.Services.AddControllers();

builder.Services.AddHostedService<AkkaService>(
  sp => (AkkaService)sp.GetRequiredService<IActorBridge>()
);
builder.Services.AddHostedService<CommunicationService>(
  sp => (CommunicationService)sp.GetRequiredService<ICommunicationService>()
);

var raftGatewayUrl = Environment.GetEnvironmentVariable("RAFT_GATEWAY_URL") ?? "http://raft-gateway:8080";
builder.Services.AddSingleton<HttpClient>(new HttpClient { BaseAddress = new Uri(raftGatewayUrl) });

builder.Services.AddSingleton<IRaftService, RaftService>();

var serviceName = "actorSystem";

builder.Logging.AddOpenTelemetry(options =>
{
  options.AddOtlpExporter(options =>
  {
    options.Endpoint = new Uri("http://asteroids_otel-collector:4317");
  }).SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
});

builder.Services.AddOpenTelemetry().WithMetrics(metrics =>
{
  metrics.AddMeter("Microsoft.AspNetCore.Hosting");
  metrics.AddMeter("Microsoft.AspNetCore.Http");
  metrics.AddMeter(LobbySupervisor.meter.Name);
  metrics.AddMeter(PlayerShip.meter.Name);
  metrics.AddOtlpExporter(options =>
  {
    options.Endpoint = new Uri("http://asteroids_otel-collector:4317");
  });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
