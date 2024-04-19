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

namespace actorSystem.Test;
public class LobbySupervisorTests : TestKit
{
  private readonly Mock<ICommunicationService> _mockCommunicationService;
  private readonly Mock<IServiceProvider> _mockLobbySupervisorServiceProvider;
  private readonly Mock<ILogger<LobbyActor>> _mockLobbyActorLogger;
  private IActorRef _lobbySupervisor;
  private TestProbe _mockRaftActor;  // Use TestProbe instead of Mock<IActorRef>

  public LobbySupervisorTests()
      : base(ActorSystemSetup.Create()
            .And(DependencyResolverSetup.Create(SetupMockServiceProvider())))
  {
    _mockCommunicationService = new Mock<ICommunicationService>();
    _mockLobbySupervisorServiceProvider = new Mock<IServiceProvider>();
    _mockLobbyActorLogger = new Mock<ILogger<LobbyActor>>();
    _mockRaftActor = CreateTestProbe();  // This replaces the Mock<RaftActor>
    _lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(_mockLobbySupervisorServiceProvider.Object, _mockRaftActor.Ref));
  }

  private static IServiceProvider SetupMockServiceProvider()
  {
    var mockServiceProvider = new Mock<IServiceProvider>();
    mockServiceProvider.Setup(x => x.GetService(typeof(ICommunicationService)))
        .Returns(new Mock<ICommunicationService>().Object);
    mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<LobbySupervisor>)))
        .Returns(new Mock<ILogger<LobbySupervisor>>().Object);
    mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<LobbyActor>)))
        .Returns(new Mock<ILogger<LobbyActor>>().Object);
    mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<RaftActor>)))
        .Returns(new Mock<ILogger<RaftActor>>().Object);

    return mockServiceProvider.Object;
  }

  [Fact]
  public void RehydrateLobby_ShouldReplaceDeadLobbyWithNewOne()
  {
    // Arrange
    var oldLobbyInfo = new LobbyInfo("testUser");
    var props = LobbyActor.Props(oldLobbyInfo, _mockCommunicationService.Object, _mockLobbyActorLogger.Object, _mockRaftActor.Ref);
    var oldLobbyActor = ActorOfAsTestActorRef<LobbyActor>(props, $"lobby_{oldLobbyInfo.Id}");
    _lobbySupervisor.Tell(new KeyValuePair<Guid, IActorRef>(oldLobbyInfo.Id, oldLobbyActor));
    Watch(oldLobbyActor);

    var newLobbyInfo = new LobbyInfo("rehydratedUser");

    _mockRaftActor.SetAutoPilot(new DelegateAutoPilot((sender, message) =>
    {
      if (message is GetLobbyCommand)
      {
        sender.Tell(newLobbyInfo, sender);  // Make sure to simulate the response correctly
        return AutoPilot.KeepRunning;
      }
      return AutoPilot.NoAutoPilot;
    }));

    // Act - simulate termination of the old lobby actor
    Sys.Stop(oldLobbyActor);
    ExpectTerminated(oldLobbyActor);

    // Simulate the process that would normally trigger rehydration
    _lobbySupervisor.Tell(new Terminated(oldLobbyActor, existenceConfirmed: true, addressTerminated: false));

    // Assert - check that a new actor is created and the dictionary is updated
    // AwaitAssert(async () =>
    // {
    //   var lobbyList = await _lobbySupervisor.Ask<LobbyList>(new GetLobbiesQuery());
    //   Assert.NotEmpty(lobbyList);
    //   Assert.Contains(lobbyList, l => l.Id == newLobbyInfo.Id);
    // }, TimeSpan.FromSeconds(3));
  }




  [Fact]
  public void LobbySupervisor_ShouldCreateLobby_WhenCreateLobbyCommandReceived()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");

    lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);

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
  public void User_Joins_Created_Lobby()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);
    var lobbyId = new Guid();

    probe.ExpectMsg<LobbyCreated>(lc =>
    {
      lobbyId = lc.Info.Id;
    });

    lobbySupervisor.Tell(new JoinLobbyCommand("testUser1", lobbyId), probe.Ref);

    probe.ExpectMsg<UserJoined>(uj =>
    {
      uj.Username.Should().Be("testUser1");
    });

  }

  [Fact]
  public async Task No_lobbies()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");

    LobbyList list = (LobbyList)await lobbySupervisor.Ask(new GetLobbiesQuery());

    list.Count.Should().Be(0);
  }

  [Fact]
  public async Task Can_Get_Lobbies()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);

    probe.ExpectMsg<LobbyCreated>();

    LobbyList list = (LobbyList)await lobbySupervisor.Ask(new GetLobbiesQuery());

    list.Count.Should().Be(1);
  }

  [Fact]
  public async Task Can_Get_Two_Lobbies()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("testUser1"), probe.Ref);
    probe.ExpectMsg<LobbyCreated>();

    lobbySupervisor.Tell(new CreateLobbyCommand("testUser2"), probe.Ref);
    probe.ExpectMsg<LobbyCreated>();

    LobbyList list = (LobbyList)await lobbySupervisor.Ask(new GetLobbiesQuery());

    list.Count.Should().Be(2);
  }

  [Fact]
  public void LobbySupervisor_ShouldThrowInvalidOperationException_WhenInvalidOperationOccurs()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    Guid lobbyId = Guid.Empty;

    lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);
    probe.ExpectMsg<LobbyCreated>(lc =>
    {
      lobbyId = lc.Info.Id;
    });

    for (int i = 0; i < 4; ++i)
    {
      lobbySupervisor.Tell(new JoinLobbyCommand($"testUser{i}", lobbyId), probe.Ref);
      probe.ExpectMsg<UserJoined>();
    }

    lobbySupervisor.Tell(new JoinLobbyCommand("testUser6", lobbyId), probe.Ref);
    probe.ExpectMsg<Status.Failure>();
  }

  [Fact]
  public void Attempt_To_Join_Non_Existent_Lobby_Returns_Failure()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    var nonExistentLobbyId = Guid.NewGuid(); // Using a GUID that hasn't been associated with a lobby

    lobbySupervisor.Tell(new JoinLobbyCommand("randomUser", nonExistentLobbyId), probe.Ref);

    probe.ExpectMsg<Status.Failure>(failure =>
    {
      failure.Cause.Message.Should().Contain($"Lobby {nonExistentLobbyId} not found.");
    });
  }

  [Fact]
  public void Request_Info_For_Non_Existent_Lobby_Returns_Failure()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    var nonExistentLobbyId = Guid.NewGuid(); // A GUID that hasn't been used to create a lobby

    lobbySupervisor.Tell(nonExistentLobbyId, probe.Ref);

    probe.ExpectMsg<Status.Failure>(failure =>
    {
      failure.Cause.Message.Should().Contain($"Lobby {nonExistentLobbyId} not found.");
    });
  }

  [Fact]
  public void Multiple_Users_Join_Same_Lobby()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("hostUser"), probe.Ref);
    var lobbyCreated = probe.ExpectMsg<LobbyCreated>();
    var lobbyId = lobbyCreated.Info.Id;

    lobbySupervisor.Tell(new JoinLobbyCommand("user1", lobbyId), probe.Ref);
    lobbySupervisor.Tell(new JoinLobbyCommand("user2", lobbyId), probe.Ref);

    probe.ExpectMsg<UserJoined>(uj => uj.Username.Should().Be("user1"));
    probe.ExpectMsg<UserJoined>(uj => uj.Username.Should().Be("user2"));
  }

  [Fact]
  public void Start_Game_In_Non_Existent_Lobby_Returns_Failure()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    var nonExistentLobbyId = Guid.NewGuid();

    lobbySupervisor.Tell(new StartGameCommand("no_one", nonExistentLobbyId), probe.Ref);

    probe.ExpectMsg<Status.Failure>(failure =>
    {
      failure.Cause.Message.Should().Contain($"Unable to start game. Lobby {nonExistentLobbyId} not found.");
    });
  }

  [Fact]
  public void LobbySupervisor_Receives_Unexpected_Command()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");

    lobbySupervisor.Tell(new object(), probe.Ref);

    probe.ExpectNoMsg(TimeSpan.FromSeconds(1)); // Verifying that the system does not crash or respond unpredictably.
  }

  [Fact]
  public async Task Query_Lobbies_After_Game_Starts()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("user1"), probe.Ref);
    var lobby1 = probe.ExpectMsg<LobbyCreated>();
    lobbySupervisor.Tell(new CreateLobbyCommand("user2"), probe.Ref);
    probe.ExpectMsg<LobbyCreated>();

    lobbySupervisor.Tell(new StartGameCommand("user1", lobby1.Info.Id), probe.Ref);

    LobbyList list = (LobbyList)await lobbySupervisor.Ask(new GetLobbiesQuery());
    list.Count.Should().Be(2);
  }

  [Fact]
  public void Lobby_Starts_Successfully()
  {
    var probe = CreateTestProbe();
    var raftProbe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(DependencyResolver.For(Sys).Props<LobbySupervisor>(raftProbe.Ref), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("user1"), probe.Ref);
    var lobby1 = probe.ExpectMsg<LobbyCreated>();

    lobbySupervisor.Tell(new StartGameCommand("user1", lobby1.Info.Id), probe.Ref);

    probe.ExpectMsg<Status.Success>();
  }

  [Fact]
  public void LobbyActorRehydrates()
  {

  }
}
