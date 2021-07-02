using UnityEditor;

namespace Game.Editor
{
    public static class CommandBuild
    {
        [MenuItem("Build Tools/Full Build[cmd]", priority = 0)]
        public static void FullBuild()
        {
        }

        [MenuItem("Build Tools/Assets Build[cmd]", priority = 1)]
        public static void AssetsBuild()
        {
        }
    }
}