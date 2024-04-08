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

namespace actorSystem.Test;
public class LobbySupervisorTests : TestKit
{
  private static IServiceProvider SetupMockServiceProvider()
  {
    var mockServiceProvider = new Mock<IServiceProvider>();
    mockServiceProvider.Setup(x => x.GetService(typeof(ICommunicationService)))
      .Returns(new Mock<ICommunicationService>().Object);

    return mockServiceProvider.Object;
  }

  public LobbySupervisorTests()
    : base(ActorSystemSetup.Create()
      .And(DependencyResolverSetup.Create(SetupMockServiceProvider())))
  {

  }

  [Fact]
  public void LobbySupervisor_ShouldCreateLobby_WhenCreateLobbyCommandReceived()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");

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
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
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
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");

    LobbyList list = (LobbyList)await lobbySupervisor.Ask(new GetLobbiesQuery());

    list.Count.Should().Be(0);
  }

  [Fact]
  public async Task Can_Get_Lobbies()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);

    probe.ExpectMsg<LobbyCreated>();

    LobbyList list = (LobbyList)await lobbySupervisor.Ask(new GetLobbiesQuery());

    list.Count.Should().Be(1);
  }

  [Fact]
  public async Task Can_Get_Two_Lobbies()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
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
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
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
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
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
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
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
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
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
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
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
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");

    lobbySupervisor.Tell(new object(), probe.Ref);

    probe.ExpectNoMsg(TimeSpan.FromSeconds(1)); // Verifying that the system does not crash or respond unpredictably.
  }

  [Fact]
  public async Task Query_Lobbies_After_Game_Starts()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
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
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("user1"), probe.Ref);
    var lobby1 = probe.ExpectMsg<LobbyCreated>();

    lobbySupervisor.Tell(new StartGameCommand("user1", lobby1.Info.Id), probe.Ref);

    probe.ExpectMsg<Status.Success>();
  }

  [Fact]
  public void Lobby_Stops_Successfully()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("user1"), probe.Ref);
    var lobby1 = probe.ExpectMsg<LobbyCreated>();
    lobbySupervisor.Tell(new StartGameCommand("user1", lobby1.Info.Id), probe.Ref);
    probe.ExpectMsg<Status.Success>();

    lobbySupervisor.Tell(new StopGameCommand("user1", lobby1.Info.Id), probe.Ref);

    probe.ExpectMsg<Status.Success>();
  }

  [Fact]
  public void Cannot_Stop_Non_Existent_Game()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");

    lobbySupervisor.Tell(new StopGameCommand("user1", Guid.Empty), probe.Ref);

    probe.ExpectMsg<Status.Failure>();
  }
}
