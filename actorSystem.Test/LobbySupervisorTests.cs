using Akka.Actor;
using Akka.TestKit.Xunit2;
using actorSystem;
using Akka.TestKit;
using FluentAssertions;
using shared.Models;

public class LobbySupervisorTests : TestKit
{
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
      lc.Info.PlayerCount.Should().Be(0);
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

    list.LobbyCount.Should().Be(0);
  }

  [Fact]
  public async Task Can_Get_Lobbies()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
    lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);

    probe.ExpectMsg<LobbyCreated>();

    LobbyList list = (LobbyList)await lobbySupervisor.Ask(new GetLobbiesQuery());

    list.LobbyCount.Should().Be(1);
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

    list.LobbyCount.Should().Be(2);
  }

  // [Fact]
  // public void LobbySupervisor_ShouldThrowInvalidOperationException_WhenInvalidOperationOccurs()
  // {
  //   // Arrange
  //   var lobbySupervisor = Sys.ActorOf(LobbySupervisor.Props(), "lobbySupervisor");
  //   Guid lobbyId = Guid.Empty;

  //   // Act
  //   lobbySupervisor.Tell(new CreateLobbyCommand("testUser"), probe.Ref);
  //   probe.ExpectMsg<LobbyCreated>(lc =>
  //   {
  //     lobbyId = lc.Info.Id;
  //   });

  //   probe.ExpectMsg<UserJoined>();

  //   for (int i = 0; i < 4; ++i)
  //   {
  //     lobbySupervisor.Tell(new JoinLobbyCommand($"testUser{i}", lobbyId), probe.Ref);
  //     probe.ExpectMsg<UserJoined>();
  //   }

  //   // Assert
  //   lobbySupervisor.Tell(new JoinLobbyCommand("testUser6", lobbyId), probe.Ref);

  //   EventFilter.Exception<InvalidOperationException>().ExpectOne(() =>
  //   {
  //   });
  // }
}
