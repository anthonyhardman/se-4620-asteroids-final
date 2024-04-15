using System.Numerics;

namespace shared.Models;

public class SpatialHash
{
  private readonly Dictionary<Vector2, List<object>> buckets = [];
  private readonly float cellSize;

  public SpatialHash(float cellSize)
  {
    this.cellSize = cellSize;
  }

  public Vector2 GetBucketKey(Vector2 position)
  {
    return new Vector2((int)(position.X / cellSize), (int)(position.Y / cellSize));
  }

  public void Insert(Vector2 position, object obj)
  {
    var key = GetBucketKey(position);
    if (!buckets.TryGetValue(key, out List<object>? value))
    {
      value = [];
      buckets[key] = value;
    }

    value.Add(obj);
  }

  public IEnumerable<object> Query(Vector2 position)
  {
    var key = GetBucketKey(position);
    if (buckets.TryGetValue(key, out List<object>? value))
    {
      foreach (var obj in value)
      {
        yield return obj;
      }
    }
  }

  public void Clear()
  {
    buckets.Clear();
  }
}