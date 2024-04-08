using System.Numerics;
using System.Text.Json;
using shared.Models;

namespace actorSystem.Test;

public class PlayerShipTests
{
  [Fact]
  public void Constructor_InitializesWithRandomPosition_WithinBounds()
  {
    var ship = new PlayerShip();
    Assert.InRange(ship.Position.X, -1200, 1200);
    Assert.InRange(ship.Position.Y, -900, 900);
  }

  [Fact]
  public void Update_WithThrust_IncreasesVelocity()
  {
    var ship = new PlayerShip(new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, -1), 100, "blue")
    {
      InputState = new InputState { Thrusting = true }
    };

    ship.Update(1.0f);

    Assert.NotEqual(new Vector2(0, 0), ship.Velocity);
  }

  [Theory]
  [InlineData(RotationDirection.Left)]
  [InlineData(RotationDirection.Right)]
  public void Update_WithRotation_ChangesDirection(RotationDirection direction)
  {
    var initialDirection = new Vector2(0, -1);
    var ship = new PlayerShip(new Vector2(0, 0), new Vector2(0, 0), initialDirection, 100, "blue")
    {
      InputState = new InputState { RotationDirection = direction }
    };

    ship.Update(1.0f);

    Assert.NotEqual(initialDirection, ship.Direction);
  }

  [Fact]
  public void Update_WithWrapping_HandlesBoundaryCross()
  {
    var ship = new PlayerShip(new Vector2(1201, 0), new Vector2(0, 0), new Vector2(0, -1), 100, "blue");

    ship.Update(1.0f);

    Assert.InRange(ship.Position.X, -1200, 1200);
  }

  [Fact]
  public void JsonSerialization_Deserialization_RetainsProperties()
  {
    var initialShip = new PlayerShip(new Vector2(100, 200), new Vector2(1, -1), new Vector2(0, -1), 100, "blue");

    var json = JsonSerializer.Serialize(initialShip);
    var deserializedShip = JsonSerializer.Deserialize<PlayerShip>(json);

    Assert.Equal(initialShip.Position, deserializedShip?.Position);
    Assert.Equal(initialShip.Velocity, deserializedShip?.Velocity);
    Assert.Equal(initialShip.Direction, deserializedShip?.Direction);
  }

  [Fact]
  public void Update_WithThrust_ExceedingMaxVelocity_CapsAtMaxVelocity()
  {
    var ship = new PlayerShip(Vector2.Zero, Vector2.Zero, new Vector2(0, -1), 100, "blue")
    {
      InputState = new InputState { Thrusting = true }
    };

    for (int i = 0; i < 100; i++)
    {
      ship.Update(1.0f);
    }

    Assert.True(ship.Velocity.Length() <= ship.VelocityCap);
  }

  [Fact]
  public void Update_RotationResultsInNormalizedDirection()
  {
    var ship = new PlayerShip(Vector2.Zero, Vector2.Zero, new Vector2(1, 1), 100, "blue")
    {
      InputState = new InputState { RotationDirection = RotationDirection.Right }
    };

    ship.Update(1.0f);

    var directionLength = ship.Direction.Length();
    Assert.Equal(1, directionLength, precision: 5);
  }

  [Theory]
  [InlineData(1201, 0)]
  [InlineData(-1201, 0)]
  [InlineData(0, 901)]
  [InlineData(0, -901)]
  public void Update_PositionExceedingBounds_WrapsCorrectly(float initialX, float initialY)
  {
    var ship = new PlayerShip(new Vector2(initialX, initialY), Vector2.Zero, new Vector2(0, -1), 100, "blue");

    ship.Update(1.0f);

    Assert.InRange(ship.Position.X, -1200, 1200);
    Assert.InRange(ship.Position.Y, -900, 900);
  }

  [Fact]
  public void JsonSerialization_ExtremeValues_RetainsProperties()
  {
    var extremeShip = new PlayerShip(new Vector2(-1200, 900), new Vector2(-0.3f, 0.3f), new Vector2(1, 0), 100, "blue");

    var json = JsonSerializer.Serialize(extremeShip);
    var deserializedShip = JsonSerializer.Deserialize<PlayerShip>(json);

    Assert.Equal(extremeShip.Position, deserializedShip?.Position);
    Assert.Equal(extremeShip.Velocity, deserializedShip?.Velocity);
    Assert.Equal(extremeShip.Direction, deserializedShip?.Direction);
  }

  [Fact]
  public void ContinuousRotation_ReturnsToOriginalDirection()
  {
    var ship = new PlayerShip(Vector2.Zero, Vector2.Zero, Vector2.UnitY, 100, "blue")
    {
      InputState = new InputState { RotationDirection = RotationDirection.Right }
    };

    float totalRotation = 0;
    while (totalRotation < Math.PI * 2)
    {
      ship.Update(0.1f);
      totalRotation += 0.05f;
    }

    Assert.True(Math.Abs(ship.Direction.Y - Vector2.UnitY.Y) < 0.01, "Ship did not return to original direction after full rotation.");
  }

  [Fact]
  public void PositionUpdate_AtMaxVelocity_AccurateAfterContinuousThrusting()
  {
    var ship = new PlayerShip(Vector2.Zero, Vector2.Zero, new Vector2(0, -1), 100, "blue")
    {
      InputState = new InputState { Thrusting = true }
    };

    for (int i = 0; i < 500; i++)
    {
      ship.Update(0.1f);
    }

    float expectedPositionY = ship.VelocityCap * 500 * 0.1f;
    Assert.True(Math.Abs(ship.Position.Y - expectedPositionY) < 20, "Ship's position not as expected after continuous thrusting at max velocity.");
  }


}
