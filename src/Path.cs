using System.Collections.Generic;

namespace TaleUtil
{
    public static class Path
    {
        // Adds a root to a path.
        public static string Enroot(string root, string path) =>
            System.IO.Path.Combine(root, path);

        // Adds a root to all paths in a list.
        public static List<string> Enroot(string root, List<string> paths)
        {
            List<string> result = new List<string>();

            foreach(string path in paths)
                result.Add(System.IO.Path.Combine(root, path));

            return result;
        }
    }
}
