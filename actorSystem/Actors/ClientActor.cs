using Akka.Actor;

namespace actorSystem;

// This actor is in charge of managing the correlation between
// websocket connection ID and username
// We'll need that for the lobby, notification actor, etc.
public class ClientActor : ReceiveActor
{
  
}