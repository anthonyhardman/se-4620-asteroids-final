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
        Size = random.Next(1, 3);

        // Position the asteroid at the edge of the board
        bool isVerticalEdge = random.Next(2) == 0;
        int edgePosition = isVerticalEdge ? random.Next(-maxY, maxY) : random.Next(-maxX, maxX);
        Position = isVerticalEdge
            ? new Vector2(maxX * (random.Next(2) * 2 - 1), edgePosition) // Left or right edge
            : new Vector2(edgePosition, maxY * (random.Next(2) * 2 - 1)); // Top or bottom edge

        // Calculate a direction vector pointed towards the center of the board
        Vector2 boardCenter = new(0, 0);
        Direction = Vector2.Normalize(boardCenter - Position);

        // Adjust the direction to aim for the middle half of the board by applying a random deviation
        float angleDeviation = (float)(random.NextDouble() * Math.PI / 2 - Math.PI / 4); // Deviate up to 45 degrees
        Direction = RotateVector(Direction, angleDeviation);

        // Random velocity
        Velocity = Direction * (float)(random.NextDouble() * 0.5 + 0.1 + 20);  // Speed corrected
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

    private static Vector2 RotateVector(Vector2 vector, float angle)
    {
        float cosAngle = (float)Math.Cos(angle);
        float sinAngle = (float)Math.Sin(angle);
        return new Vector2(
            vector.X * cosAngle - vector.Y * sinAngle,
            vector.X * sinAngle + vector.Y * cosAngle
        );
    }
}
