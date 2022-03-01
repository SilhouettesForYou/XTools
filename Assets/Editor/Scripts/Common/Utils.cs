using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XTools
{
    public class Utils : SingleBase<Utils>
    {
        public static string[] GetSplittedPath(string path, char separator)
        {
            return path.Split(separator);
        }

        public static string GetFileNameFromPath(string path, char separator)
        {
            var splits = GetSplittedPath(path, separator);
            return splits[splits.Length - 1].ToString();
        }
    }
}
