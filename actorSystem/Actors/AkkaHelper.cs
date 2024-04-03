using System.Text.RegularExpressions;

namespace actorSystem;

public static class AkkaHelper
{
  public static string UsernameToActorPath(string username)
  {
    if (string.IsNullOrEmpty(username))
    {
      throw new ArgumentException("Username cannot be null or empty.", nameof(username));
    }

    string validActorPath = Regex.Replace(username, "[\\$\\/\\#\\s]+", "_").ToLower();

    return validActorPath;
  }
}