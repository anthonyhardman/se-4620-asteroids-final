using actorSystem;
using actorSystem.Services;
using Akka.DependencyInjection;
using Akka.Hosting;

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
