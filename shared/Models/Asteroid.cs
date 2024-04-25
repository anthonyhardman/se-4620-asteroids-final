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
    public float Damage => Size * Velocity.Length() * 20;
    public float Health { get; set; }
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

        Health = Size;

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


    [JsonConstructor, Newtonsoft.Json.JsonConstructor]
    public Asteroid(Vector2 position, Vector2 velocity, Vector2 direction, float size, float health = 1)
    {
        Position = position;
        Velocity = velocity;
        Direction = direction;
        Size = size;
        Health = health;
    }

    public void Update(float timeStep)
    {
        Position += Velocity * (timeStep / 100);
    }

    public void HandleCollision()
    {
        Velocity = -Velocity;
        if (Velocity.Length() > 20)
        {
            Velocity = Vector2.Normalize(Velocity) * 20;
        }
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

    public void TakeDamage(float damage = 1)
    {
        Health = Math.Max(0, Health - damage);
    }
}
