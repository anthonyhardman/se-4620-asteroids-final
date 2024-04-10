using System.Numerics;
using shared.Models;

namespace actorSystem.Test;

public class AsteroidTests
{
  readonly int maxX = 1200;
  readonly int maxY = 900;

  [Fact]
  public void Constructor_SetsPropertiesCorrectly()
  {
    var asteroid = new Asteroid(maxX, maxY);
    bool isOnEdgeX = asteroid.Position.X == maxX || asteroid.Position.X == -maxX;
    bool isOnEdgeY = asteroid.Position.Y == maxY || asteroid.Position.Y == -maxY;
    Assert.True(isOnEdgeX || isOnEdgeY);
    Assert.InRange(asteroid.Size, 1, 2);
    Assert.InRange(asteroid.Velocity.Length(), 15.1f, 25.6f); // Check if velocity is within the expected range
  }

  [Fact]
  public void Constructor_InitializesDirectionTowardsCenter()
  {
    var asteroid = new Asteroid(maxX, maxY);
    var center = new Vector2(0, 0);
    var directionToCenter = Vector2.Normalize(center - asteroid.Position);
    var dotProduct = Vector2.Dot(Vector2.Normalize(asteroid.Direction), directionToCenter);

    Assert.True(dotProduct > 0.707);
  }

  [Fact]
  public void Update_ChangesPositionBasedOnVelocity()
  {
    var initialPosition = new Vector2(0, 0);
    var velocity = new Vector2(5, 0);
    var asteroid = new Asteroid(initialPosition, velocity, new Vector2(0, 1), 2);

    asteroid.Update(1000);
    Assert.Equal(new Vector2(50, 0), asteroid.Position);
  }

  [Fact]
  public void Health_IsCalculatedAsSquareOfSize()
  {
    var asteroid = new Asteroid(new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1), 2);
    Assert.Equal(4, asteroid.Health);
  }

  [Fact]
  public void Damage_IsCalculatedAsSizeTimesVelocityLength()
  {
    var asteroid = new Asteroid(new Vector2(0, 0), new Vector2(3, 4), new Vector2(0, 1), 2); // Velocity length = 5
    Assert.Equal(10, asteroid.Damage);
  }


}