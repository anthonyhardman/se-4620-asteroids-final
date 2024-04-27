using Akka.Actor;
using Akka.TestKit.Xunit2;
using actorSystem;
using Akka.TestKit;
using FluentAssertions;
using shared.Models;
using Moq;
using actorSystem.Services;
using Akka.Actor.Setup;
using Akka.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Akka.Event;

namespace actorSystem.Test;


public class LobbySupervisorTests : TestKit
{
  private IActorRef _lobbySupervisor;
  private IActorRef _raftActor;
  private static readonly Mock<IRaftService> _mockRaftService = new();
  private static readonly Guid oldId = Guid.NewGuid();
  private static LobbyInfo info;

  private static ActorSystem CreateActorSystemWithDI()
  {
    var serviceProvider = SetupMockServiceProvider();
    var resolverSetup = DependencyResolverSetup.Create(serviceProvider);

    var actorSystemSetup = ActorSystemSetup.Create(resolverSetup);

    return ActorSystem.Create("TestSystem", actorSystemSetup);
  }

  public LobbySupervisorTests() : base(CreateActorSystemWithDI())
  {
    this._raftActor = base.CreateTestProbe();

    var resolver = DependencyResolver.For(Sys);
    var raftActorProps = resolver.Props<RaftActor>(_mockRaftService.Object);
    _raftActor = Sys.ActorOf(raftActorProps, "raft-actor");
    var lobbySupervisorProps = resolver.Props<LobbySupervisor>();
    _lobbySupervisor = Sys.ActorOf(lobbySupervisorProps, "lobbySupervisor");
  }

  private static IServiceProvider SetupMockServiceProvider()
  {
    var services = new ServiceCollection();

    services.AddSingleton(new Mock<ICommunicationService>().Object);
    services.AddSingleton(new Mock<ILogger<LobbySupervisor>>().Object);
    services.AddSingleton(new Mock<ILogger<LobbyActor>>().Object);
    services.AddSingleton(new Mock<ILogger<RaftActor>>().Object);
    var lobbyInfo = new LobbyInfo(
      id: oldId,
      createdBy: "preExistingUser",
      maxPlayers: 5,
      players: new Dictionary<string, PlayerShip> { { "preExistingUser", new PlayerShip(100, 100) } },
      state: LobbyState.Joining,
      countdownTime: 0,
      asteroids: new List<Asteroid>(),
      maxAsteroids: 30
    );
    info = lobbyInfo;
    _mockRaftService.Setup(x => x.StrongGet<List<Guid>>("lobbyList"))
                    .ReturnsAsync((new List<Guid> { lobbyInfo.Id }, 0));
    var serializedLobbyInfo = JsonSerializer.Serialize(lobbyInfo);
    _mockRaftService.Setup(x => x.StrongGet<string>(oldId.ToString()))
                    .ReturnsAsync((serializedLobbyInfo, 1));

    services.AddSingleton(_mockRaftService.Object);

    var serviceProvider = services.BuildServiceProvider(validateScopes: true);
    var mockServiceScope = new Mock<IServiceScope>();
    mockServiceScope.Setup(m => m.ServiceProvider).Returns(serviceProvider);

    var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
    mockServiceScopeFactory.Setup(m => m.CreateScope()).Returns(mockServiceScope.Object);
    services.AddSingleton<IServiceScopeFactory>(mockServiceScopeFactory.Object);

    return services.BuildServiceProvider();
  }

  [Fact]
  public void LobbySupervisor_ShouldCreateLobby_WhenCreateLobbyCommandReceived()
  {
    var probe = CreateTestProbe();

    _lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);

    probe.ExpectMsg<LobbyCreated>(lc =>
    {
      lc.Info.CreatedBy.Should().Be("testUser");
      lc.Info.MaxPlayers.Should().Be(5);
      lc.Info.PlayerCount.Should().Be(1);
      lc.Info.State.Should().Be(LobbyState.Joining);
      lc.ActorPath.Should().Contain("lobby_");
    });
  }

  [Fact]
  public void Test_Mock_StrongGet_Returns_Expected_Value()
  {
    var serializedLobbyInfo = JsonSerializer.Serialize(info);

    var result = _mockRaftService.Object.StrongGet<string>(info.Id.ToString()).Result;

    result.Should().NotBeNull(); // Verify that the mock returns the correct data
    result.value.Should().Be(serializedLobbyInfo); // Ensure it returns the correct value
  }


  [Fact]
  public void LobbySupervisor_ShouldRehydrate_Terminated_Actor()
  {
    var probe = CreateTestProbe();

    // Create a lobby and terminate the actor to trigger rehydration
    var createCommand = new CreateLobbyCommand("testUser");
    _lobbySupervisor.Tell(createCommand, probe.Ref);
    var created = probe.ExpectMsg<LobbyCreated>();

    var createdLobbyActor = Sys.ActorSelection(created.ActorPath);
    createdLobbyActor.Tell(PoisonPill.Instance); // Terminate the actor to trigger rehydration

    // Act
    probe.Watch(_lobbySupervisor); // Watch the supervisor for rehydration
    _lobbySupervisor.Tell(new GetLobbiesQuery(), probe.Ref);

    // Assert
    var lobbyList = probe.ExpectMsg<LobbyList>();
    lobbyList.Count.Should().Be(1); // Ensure rehydration occurred

    var rehydratedLobby = lobbyList[0];
    rehydratedLobby.Id.Should().Be(oldId); // Validate correct rehydration
    rehydratedLobby.CreatedBy.Should().Be("preExistingUser"); // Validate consistent data
  }

  [Fact]
  public void LobbySupervisor_ShouldRecreate_PreExisting_Lobby()
  {
    var probe = CreateTestProbe();
    probe.Watch(_lobbySupervisor);
    _lobbySupervisor.Tell(new GetLobbiesQuery(), probe.Ref);

    var lobbyList = probe.ExpectMsg<LobbyList>();

    lobbyList.Count.Should().Be(1); // Verify that one lobby is recreated
    var recreatedLobby = lobbyList[0];
    recreatedLobby.Id.Should().Be(oldId); // Ensure it's the correct lobby
  }

  [Fact]
  public void User_Joins_Created_Lobby()
  {
    var probe = CreateTestProbe();
    _lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);
    var lobbyId = new Guid();

    probe.ExpectMsg<LobbyCreated>(lc =>
    {
      lobbyId = lc.Info.Id;
    });

    _lobbySupervisor.Tell(new JoinLobbyCommand("testUser1", lobbyId), probe.Ref);

    probe.ExpectMsg<UserJoined>(uj =>
    {
      uj.Username.Should().Be("testUser1");
    });

  }

  [Fact]
  public async Task No_lobbies()
  {
    LobbyList list = (LobbyList)await _lobbySupervisor.Ask(new GetLobbiesQuery());

    list.Count.Should().Be(0);
  }

  [Fact]
  public async Task Can_Get_Lobbies()
  {
    var probe = CreateTestProbe();
    _lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);

    probe.ExpectMsg<LobbyCreated>();

    LobbyList list = (LobbyList)await _lobbySupervisor.Ask(new GetLobbiesQuery());

    list.Count.Should().Be(1);
  }

  [Fact]
  public async Task Can_Get_Two_Lobbies()
  {
    var probe = CreateTestProbe();
    _lobbySupervisor.Tell(new CreateLobbyCommand("testUser1"), probe.Ref);
    probe.ExpectMsg<LobbyCreated>();

    _lobbySupervisor.Tell(new CreateLobbyCommand("testUser2"), probe.Ref);
    probe.ExpectMsg<LobbyCreated>();

    LobbyList list = (LobbyList)await _lobbySupervisor.Ask(new GetLobbiesQuery());

    list.Count.Should().Be(2);
  }

  [Fact]
  public void LobbySupervisor_ShouldThrowInvalidOperationException_WhenInvalidOperationOccurs()
  {
    var probe = CreateTestProbe();
    Guid lobbyId = Guid.Empty;

    _lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);
    probe.ExpectMsg<LobbyCreated>(lc =>
    {
      lobbyId = lc.Info.Id;
    });

    for (int i = 0; i < 4; ++i)
    {
      _lobbySupervisor.Tell(new JoinLobbyCommand($"testUser{i}", lobbyId), probe.Ref);
      probe.ExpectMsg<UserJoined>();
    }

    _lobbySupervisor.Tell(new JoinLobbyCommand("testUser6", lobbyId), probe.Ref);
    probe.ExpectMsg<Status.Failure>();
  }

  [Fact]
  public void Attempt_To_Join_Non_Existent_Lobby_Returns_Failure()
  {
    var probe = CreateTestProbe();
    var nonExistentLobbyId = Guid.NewGuid(); // Using a GUID that hasn't been associated with a lobby

    _lobbySupervisor.Tell(new JoinLobbyCommand("randomUser", nonExistentLobbyId), probe.Ref);

    probe.ExpectMsg<Status.Failure>(failure =>
    {
      failure.Cause.Message.Should().Contain($"Lobby {nonExistentLobbyId} not found.");
    });
  }

  [Fact]
  public void Request_Info_For_Non_Existent_Lobby_Returns_Failure()
  {
    var probe = CreateTestProbe();
    var nonExistentLobbyId = Guid.NewGuid(); // A GUID that hasn't been used to create a lobby

    _lobbySupervisor.Tell(nonExistentLobbyId, probe.Ref);

    probe.ExpectMsg<Status.Failure>(failure =>
    {
      failure.Cause.Message.Should().Contain($"Lobby {nonExistentLobbyId} not found.");
    });
  }

  [Fact]
  public void Multiple_Users_Join_Same_Lobby()
  {
    var probe = CreateTestProbe();
    _lobbySupervisor.Tell(new CreateLobbyCommand("hostUser"), probe.Ref);
    var lobbyCreated = probe.ExpectMsg<LobbyCreated>();
    var lobbyId = lobbyCreated.Info.Id;

    _lobbySupervisor.Tell(new JoinLobbyCommand("user1", lobbyId), probe.Ref);
    _lobbySupervisor.Tell(new JoinLobbyCommand("user2", lobbyId), probe.Ref);

    probe.ExpectMsg<UserJoined>(uj => uj.Username.Should().Be("user1"));
    probe.ExpectMsg<UserJoined>(uj => uj.Username.Should().Be("user2"));
  }

  [Fact]
  public void Start_Game_In_Non_Existent_Lobby_Returns_Failure()
  {
    var probe = CreateTestProbe();
    var nonExistentLobbyId = Guid.NewGuid();

    _lobbySupervisor.Tell(new StartGameCommand("no_one", nonExistentLobbyId), probe.Ref);

    probe.ExpectMsg<Status.Failure>(failure =>
    {
      failure.Cause.Message.Should().Contain($"Unable to start game. Lobby {nonExistentLobbyId} not found.");
    });
  }

  [Fact]
  public void LobbySupervisor_Receives_Unexpected_Command()
  {
    var probe = CreateTestProbe();

    _lobbySupervisor.Tell(new object(), probe.Ref);

    probe.ExpectNoMsg(TimeSpan.FromSeconds(1)); // Verifying that the system does not crash or respond unpredictably.
  }

  [Fact]
  public async Task Query_Lobbies_After_Game_Starts()
  {
    var probe = CreateTestProbe();
    _lobbySupervisor.Tell(new CreateLobbyCommand("user1"), probe.Ref);
    var lobby1 = probe.ExpectMsg<LobbyCreated>();
    _lobbySupervisor.Tell(new CreateLobbyCommand("user2"), probe.Ref);
    probe.ExpectMsg<LobbyCreated>();

    _lobbySupervisor.Tell(new StartGameCommand("user1", lobby1.Info.Id), probe.Ref);

    LobbyList list = (LobbyList)await _lobbySupervisor.Ask(new GetLobbiesQuery());
    list.Count.Should().Be(2);
  }

  [Fact]
  public void Lobby_Starts_Successfully()
  {
    var probe = CreateTestProbe();
    _lobbySupervisor.Tell(new CreateLobbyCommand("user1"), probe.Ref);
    var lobby1 = probe.ExpectMsg<LobbyCreated>();

    _lobbySupervisor.Tell(new StartGameCommand("user1", lobby1.Info.Id), probe.Ref);

    probe.ExpectMsg<Status.Success>();
  }
}
