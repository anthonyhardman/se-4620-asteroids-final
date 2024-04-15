using System.Numerics;
using System.Text.Json.Serialization;

namespace shared.Models;

public class Bullet
{
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Position { get; private set; }
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Velocity { get; private set; }
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Direction { get; private set; }

  [JsonConstructor]
  public Bullet(Vector2 position, Vector2 direction)
  {
    Position = position;
    Direction = direction;
    Velocity = Direction * 0.6f;
  }

  public void Update(float timeStep)
  {
    Position += Velocity * timeStep;
  }
}