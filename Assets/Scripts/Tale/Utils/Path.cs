using System.Collections.Generic;

namespace TaleUtil
{
    public static class Path
    {
        // Normalizes a path to a valid runtime Asset path.
        // (removes extensions, removes prefixes such as Assets/Resources, etc).
        // Also adds a root to the path, only if the path is relative.
        public static string NormalizeResourcePath(string path, bool keepExtension = false)
        {
            return NormalizeResourcePath("", path, keepExtension);
        }

        // Also adds a root to the path, only if the path is relative.
        public static string NormalizeResourcePath(string root, string path, bool keepExtension = false)
        {
            bool relative = true;

            if (path.Length > 0 && path[0] == '/')
            {
                path = path.Substring(1);
                relative = false;
            }

            if (path.StartsWith("Assets/"))
            {
                path = path.Substring("Assets/".Length);
                relative = false;
            }

            // Also handles Assets/Resources/
            if (path.StartsWith("Resources/"))
            {
                path = path.Substring("Resources/".Length);
                relative = false;
            }

            if (System.IO.Path.HasExtension(path) && !keepExtension)
            {
                path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path)).Replace('\\', '/');
            }

            if (relative)
            {
                return System.IO.Path.Combine(root, path);
            }

            return path;
        }

        // Normalizes multiple path to valid Asset paths.
        public static List<string> NormalizeAssetPath(string root, List<string> paths)
        {
            List<string> result = new List<string>();

            foreach (string path in paths)
            {
                result.Add(NormalizeResourcePath(root, path));
            }

            return result;
        }
    }
}
