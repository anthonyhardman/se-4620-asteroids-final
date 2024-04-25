using Raft.Grpc;

namespace actorSystem;

public interface IRaftService
{
  public  Task<(string value, int version)> StrongGet(string key);

  public  Task<(T value, int version)> StrongGet<T>(string key);

  public  Task<T> EventualGet<T>(string key);
  public  Task<CompareAndSwapResponse> CompareAndSwap(string key, string value, string expectedValue, int version);

  public  Task<CompareAndSwapResponse> CompareAndSwap<T>(string key, T value, T expectedValue, int version);
}