using System.Data;
using System.Numerics;
using System.Text.Json.Serialization;

namespace shared.Models;

public class PlayerShip
{
  public Vector2 Position { get; private set; }
  public Vector2 Velocity { get; private set; }
  public Vector2 Direction { get; private set; }
  public InputState? InputState { get; set; }

  public void Update(float timeStep)
  {
    if (InputState != null && InputState.RotationDirection == RotationDirection.Left)
    {
      Direction = Vector2.Transform(Direction, Matrix3x2.CreateRotation(-0.1f));
      Direction = Vector2.Normalize(Direction);
    }
    else if (InputState != null && InputState.RotationDirection == RotationDirection.Right)
    {
      Direction = Vector2.Transform(Direction, Matrix3x2.CreateRotation(0.1f));
      Direction = Vector2.Normalize(Direction);
    }

    if (InputState != null && InputState.Thrusting)
    {
      Vector2 oldPosition = Position;
      Position += Velocity + 0.5f * Direction * timeStep * timeStep;
      Velocity = (Position - oldPosition) / timeStep;
    }
    else
    {
      Position += Velocity * timeStep;
    }
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