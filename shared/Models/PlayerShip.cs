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
  public float Health { get; private set; }
  public float MaxHealth { get; } = 100;
  public string Color { get; private set; }
  private const float acceleration = 0.0005f;
  private const float rotationAmount = 0.05f;
  public float VelocityCap { get; init; } = 0.2f;
  private static readonly Random random = new();
  private static readonly string[] colors = ["blue", "red", "green", "yellow", "purple", "orange"];
  public int MaxX { get; private set; }
  public int MaxY { get; private set; }
  public int Points { get; set; } = 0;
  public float CollisionCooldown { get; private set; } = 0f;
  public List<Bullet> Bullets { get; init; } = [];
  public float FireCooldown { get; private set; } = 0f;
  private const float FireCooldownDuration = 500f;

  public PlayerShip(int maxX, int maxY)
  {
    Position = new Vector2(
        random.Next(-maxX, maxX),
        random.Next(-maxY, maxY)
    );
    Velocity = new Vector2(0.0f, 0);
    Direction = new Vector2(0, -1);
    Health = MaxHealth;
    Color = colors[random.Next(colors.Length)];
    MaxX = maxX;
    MaxY = maxY;
  }

  [JsonConstructor]
  public PlayerShip(Vector2 position, Vector2 velocity, Vector2 direction, List<Bullet> bullets, float health, string color, int maxX, int maxY, int points = 0)
  {
    Position = position;
    Velocity = velocity;
    Direction = direction;
    Health = health;
    Color = color;
    MaxX = maxX;
    MaxY = maxY;
    Points = points;
    Bullets = bullets;
  }

  public void Update(float timeStep)
  {
    if (CollisionCooldown > 0)
    {
      CollisionCooldown -= timeStep;
    }
    if (FireCooldown > 0)
    {
      FireCooldown -= timeStep;
    }
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

    if (InputState != null && InputState.ShootPressed)
    {
      Fire();
    }

    if (InputState != null && InputState.Thrusting)
    {
      Vector2 oldPosition = Position;
      Position += Velocity * timeStep + 0.5f * Direction * acceleration * timeStep * timeStep;
      Velocity = (Position - oldPosition) / timeStep;

      if (Velocity.Length() > VelocityCap)
      {
        Velocity = Vector2.Normalize(Velocity) * VelocityCap;
      }
    }
    else
    {
      Position += Velocity * timeStep;
    }
    Position = new Vector2(
        WrapValue(Position.X, -MaxX, MaxX),
        WrapValue(Position.Y, -MaxY, MaxY)
    );
    if (Health > 0)
    {
      Points += 1;
    }
  }

  public void UpdateBullets(float timeStep)
  {
    foreach (var bullet in Bullets)
    {
      bullet.Update(timeStep);
    }
  }

  public void HandleCollision(Asteroid asteroid)
  {
    CollisionCooldown = 1000.0f;
    Direction = Vector2.Reflect(Direction, asteroid.Direction);
    Velocity += asteroid.Velocity;

    if (Velocity.Length() > VelocityCap)
    {
      Velocity = Vector2.Normalize(Velocity) * VelocityCap;
    }
  }


  private float WrapValue(float value, float min, float max)
  {
    float range = max - min;
    while (value < min) value += range;
    while (value > max) value -= range;
    return value;
  }

  public void TakeDamage(float damagePerSecond, float timeStep)
  {
    var damage = damagePerSecond * timeStep / 1000;
    Health = Math.Max(0, Health - damage);
  }

  public void Fire()
  {
    if (FireCooldown <= 0)
    {
      var bullet = new Bullet(Position, Direction);
      Bullets.Add(bullet);
      FireCooldown = FireCooldownDuration;
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
  public bool ShootPressed { get; set; }
}