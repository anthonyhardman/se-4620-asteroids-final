using System.Data;
using System.Numerics;

namespace shared.Models;

public class PlayerShip
{
  Vector2 Position { get; set; }
  Vector2 Velocity { get; set; }
  Vector2 Direction { get; set; }
  public InputState InputState { get; set; }

  public void Update(float timeStep)
  {
    if (InputState.RotationDirection == RotationDirection.Left)
    {
      Direction = Vector2.Transform(Direction, Matrix3x2.CreateRotation(-0.1f));
      Direction = Vector2.Normalize(Direction);
    }
    else if (InputState.RotationDirection == RotationDirection.Right)
    {
      Direction = Vector2.Transform(Direction, Matrix3x2.CreateRotation(0.1f));
      Direction = Vector2.Normalize(Direction);
    }

    if (InputState.Thrusting)
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