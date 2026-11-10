using UnityEngine;

// Which Unity Gaming Services environment the game talks to. The editor and development builds use
// "development", so testing never touches players' accounts, saves or leaderboards; release builds use
// "production". Set in code so a build never depends on the editor's environment selection, which the
// Deployment window also uses as its deploy target.
public static class ServicesEnvironment
{
    public const string Development = "development";
    public const string Production = "production";

    // Debug.isDebugBuild is always true in the editor
    public static string Current => Select(Debug.isDebugBuild);

    public static string Select(bool isDebugBuild) => isDebugBuild ? Development : Production;
}
