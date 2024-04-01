
using Akka.Actor;
using Akka.DependencyInjection;

namespace actorSystem;


public class AkkaService : IHostedService, IActorBridge
{
  private ActorSystem _actorSystem;
  private readonly IConfiguration _configuration;
  private readonly IServiceProvider _serviceProvider;
  private readonly IHostApplicationLifetime _applicationLifetime;

  public AkkaService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime, IConfiguration configuration)
  {
    _serviceProvider = serviceProvider;
    _applicationLifetime = appLifetime;
    _configuration = configuration;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var bootstrap = BootstrapSetup.Create();

    var diSetup = DependencyResolverSetup.Create(_serviceProvider);

    var actorSystemSetup = bootstrap.And(diSetup);

    _actorSystem = ActorSystem.Create("akka-system", actorSystemSetup);

#pragma warning disable CS4014
    _actorSystem.WhenTerminated.ContinueWith(_ =>
    {
      _applicationLifetime.StopApplication();
    });
#pragma warning restore CS4014 
    await Task.CompletedTask;
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await CoordinatedShutdown.Get(_actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
  }

  public void Tell(object message)
  {
    throw new NotImplementedException();
  }

  public Task<T> Ask<T>(object message)
  {
    throw new NotImplementedException();
  }
}
