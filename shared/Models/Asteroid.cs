using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace shared.Models;

public class Asteroid
{
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Position { get; private set; }
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Velocity { get; private set; }
  [JsonConverter(typeof(Vector2Converter))]
  public Vector2 Direction { get; private set; }
  public float Size { get; private set; }
  public float Damage => Size * Velocity.Length();
  public float Health => Size * Size;
  private static readonly Random random = new();

  public Asteroid(int maxX, int maxY)
  {
    int rand = random.Next(1, 11);
    if (rand <= 5)
      Size = 1;
    else if (rand <= 8)
      Size = 2;
    else
      Size = 3;

    bool isVerticalEdge = random.Next(2) == 0;
    int edgePosition = isVerticalEdge ? random.Next(-maxY, maxY) : random.Next(-maxX, maxX);
    Position = isVerticalEdge
        ? new Vector2(maxX * (random.Next(2) * 2 - 1), edgePosition)
        : new Vector2(edgePosition, maxY * (random.Next(2) * 2 - 1));

    Vector2 boardCenter = new(0, 0);
    Direction = Vector2.Normalize(boardCenter - Position);

    float angleDeviation = (float)(random.NextDouble() * Math.PI / 2 - Math.PI / 4);
    Direction = RotateVector(Direction, angleDeviation);

    float baseSpeed = 30 - 5 * Size;
    Velocity = Direction * (float)(random.NextDouble() * 0.5 + 0.1 + baseSpeed);
  }


  [JsonConstructor]
  public Asteroid(Vector2 position, Vector2 velocity, Vector2 direction, float size)
  {
    Position = position;
    Velocity = velocity;
    Direction = direction;
    Size = size;
  }

    public void Update(float timeStep)
    {
        Position += Velocity * (timeStep / 100);
    }

    public void HandleCollision(PlayerShip ship)
    {
        // Calculate the effect factor based on the asteroid's size; larger asteroids are less affected
        float effectFactor = 1 / Size;  // The larger the size, the smaller the effect factor

        // Calculate new direction as a weighted average of the asteroid's and ship's directions
        Vector2 newDirection = Vector2.Normalize((Direction + ship.Direction) / 2);
        Direction = newDirection;

        // Adjust velocity based on the ship's velocity, modified by the effect factor
        // Increasing the ship's influence on smaller asteroids
        Velocity += ship.Velocity * effectFactor;

        // Ensure that the asteroid's velocity does not exceed a reasonable maximum to maintain game balance
        if (Velocity.Length() > 20)
        {
            Velocity = Vector2.Normalize(Velocity) * 20;
        }

        Health -= 1;
    }


  private static Vector2 RotateVector(Vector2 vector, float angle)
  {
    float cosAngle = (float)Math.Cos(angle);
    float sinAngle = (float)Math.Sin(angle);
    return new Vector2(
        vector.X * cosAngle - vector.Y * sinAngle,
        vector.X * sinAngle + vector.Y * cosAngle
    );
  }

  public void TakeDamage(float damage)
  {
    // Size -= damage;
  }
}
