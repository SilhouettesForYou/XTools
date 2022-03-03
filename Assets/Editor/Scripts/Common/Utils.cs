using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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

        static public T CreateAsset<T>(string path, string name) where T : ScriptableObject
        {
            var scriptableObj = ScriptableObject.CreateInstance<T>();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }

            path = path.Replace('\\', '/');
            path = path.TrimEnd('/');
            path += "/";
            string filePath = path + name + ".asset";

            AssetDatabase.CreateAsset(scriptableObj, filePath);

            return scriptableObj;
        }
    }
}
