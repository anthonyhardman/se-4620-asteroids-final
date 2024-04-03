using actorSystem;
using actorSystem.Services;
using Akka.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ICommunicationService, CommunicationService>();
builder.Services.AddSingleton<IActorBridge, AkkaService>();

builder.Services.AddAkka("asteroid-system", (cb) =>
{
  cb.WithActors((system, registry) =>
     {
       var sessionSupervisorActor = system.ActorOf(ClientSupervisor.Props(), "client-supervisor");
       registry.TryRegister<ClientSupervisor>(sessionSupervisorActor);
       var lobbySupervisorActor = system.ActorOf(LobbySupervisor.Props(), "lobby-supervisor");
       registry.TryRegister<LobbySupervisor>(lobbySupervisorActor);
     });
});

builder.Services.AddHostedService<AkkaService>(
  sp => (AkkaService)sp.GetRequiredService<IActorBridge>()
);
builder.Services.AddHostedService<CommunicationService>(
  sp => (CommunicationService)sp.GetRequiredService<ICommunicationService>()
);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
