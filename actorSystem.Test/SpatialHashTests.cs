using System.Collections.Generic;
using System.Numerics;
using Xunit;
using shared.Models;

namespace shared.Models.Tests;

public class SpatialHashTests
{
  [Fact]
  public void Insert_InsertsObjectIntoBucket()
  {
    var spatialHash = new SpatialHash(1f);
    var position = new Vector2(3f, 4f);
    var obj = new object();

    spatialHash.Insert(position, obj);

    var objectsInBucket = spatialHash.Query(position);
    Assert.Contains(obj, objectsInBucket);
  }

  [Fact]
  public void Query_ReturnsObjectsInSameBucket()
  {
    var spatialHash = new SpatialHash(1f);
    var position1 = new Vector2(3f, 4f);
    var obj1 = new object();
    var position2 = new Vector2(4f, 5f);
    var obj2 = new object();

    spatialHash.Insert(position1, obj1);
    spatialHash.Insert(position2, obj2);

    var objectsInBucket1 = spatialHash.Query(position1);
    var objectsInBucket2 = spatialHash.Query(position2);
    Assert.Contains(obj1, objectsInBucket1);
    Assert.Contains(obj2, objectsInBucket2);
  }

  [Fact]
  public void Query_ReturnsEmptyEnumerableWhenBucketIsEmpty()
  {
    var spatialHash = new SpatialHash(1f);
    var position = new Vector2(3f, 4f);

    var objectsInBucket = spatialHash.Query(position);

    Assert.Empty(objectsInBucket);
  }

  [Fact]
  public void Clear_RemovesAllObjects()
  {
    var spatialHash = new SpatialHash(1f);
    var position = new Vector2(3f, 4f);
    var obj = new object();
    spatialHash.Insert(position, obj);

    spatialHash.Clear();

    var objectsInBucket = spatialHash.Query(position);
    Assert.Empty(objectsInBucket);
  }

  [Fact]
  public void GetBucketKey_ReturnsCorrectBucketKey()
  {
    var spatialHash = new SpatialHash(2f);
    var position = new Vector2(3f, 4f);
    var expectedKey = new Vector2(1f, 2f);

    var key = spatialHash.GetBucketKey(position);

    Assert.Equal(expectedKey, key);
  }

  [Fact]
  public void GetBucketKey_ReturnsNegativeBucketKeyForNegativePosition()
  {
    var spatialHash = new SpatialHash(2f);
    var position = new Vector2(-3f, -4f);
    var expectedKey = new Vector2(-1f, -2f);

    var key = spatialHash.GetBucketKey(position);

    Assert.Equal(expectedKey, key);
  }
}