using System.Data;
using System.Numerics;
using System.Text.Json.Serialization;

namespace shared.Models;

public class PlayerShip
{
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Position { get; private set; }
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Velocity { get; private set; }
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Direction { get; private set; }
  public InputState? InputState { get; set; }
  private const float acceleration = 0.0005f;
  private const float rotationAmount = 0.05f;
  private const float velocityCap = 0.3f;
  private const int maxX = 400 * 3;
  private const int maxY = 300 * 3;

  public PlayerShip()
  {
    Position = new Vector2(0, 0);
    Velocity = new Vector2(0.0f, 0);
    Direction = new Vector2(0, -1);
  }

  [JsonConstructor]
  public PlayerShip(Vector2 position, Vector2 velocity, Vector2 direction)
  {
    Position = position;
    Velocity = velocity;
    Direction = direction;
  }

  public void Update(float timeStep)
  {
    if (InputState != null && InputState.RotationDirection == RotationDirection.Left)
    {
      Direction = Vector2.Transform(Direction, Matrix3x2.CreateRotation(rotationAmount));
      Direction = Vector2.Normalize(Direction);
    }
    else if (InputState != null && InputState.RotationDirection == RotationDirection.Right)
    {
      Direction = Vector2.Transform(Direction, Matrix3x2.CreateRotation(-rotationAmount));
      Direction = Vector2.Normalize(Direction);
    }

    if (InputState != null && InputState.Thrusting)
    {
      Vector2 oldPosition = Position;
      Position += Velocity * timeStep + 0.5f * Direction * acceleration * timeStep * timeStep;
      Velocity = (Position - oldPosition) / timeStep;

      if (Velocity.Length() > velocityCap)
      {
        Velocity = Vector2.Normalize(Velocity) * velocityCap;
      }
    }
    else
    {
      Position += Velocity * timeStep;
    }
    Position = new Vector2(
        WrapValue(Position.X, -maxX, maxX),
        WrapValue(Position.Y, -maxY, maxY)
    );
  }

  private float WrapValue(float value, float min, float max)
  {
      float range = max - min;
      while (value < min) value += range;
      while (value > max) value -= range;
      return value;
  }
}

public enum RotationDirection
{
  None,
  Left,
  Right
}

public class InputState
{
  public bool Thrusting { get; set; }
  public RotationDirection RotationDirection { get; set; }
  public int ShootPressed { get; set; }
}