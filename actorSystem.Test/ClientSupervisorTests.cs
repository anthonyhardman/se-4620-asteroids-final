using Akka.TestKit.Xunit2;
using actorSystem;
using Xunit;
using Akka.TestKit;

public class ClientSupervisorTests : TestKit
{
  public readonly TestProbe probe;

  public ClientSupervisorTests()
  {
    probe = CreateTestProbe();
  }

  [Fact]
  public void ClientSupervisor_Should_RegisterClient_Successfully()
  {
    var supervisor = Sys.ActorOf(ClientSupervisor.Props(), "clientSupervisor");
    var registerCommand = new RegisterClientCommand("conn123", "JohnDoe");

    supervisor.Tell(registerCommand, probe.Ref);

    probe.ExpectMsg<ClientRegistered>(msg =>
        msg.ConnectionId == "conn123" &&
        msg.ClientActorPath.Contains("client_johndoe"));
  }
}
