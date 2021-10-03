using System;
using System.IO;
using UnityEngine;

namespace GameServices
{
    public static class FileManager
    {
        public static bool FileExists(string fileName)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, fileName);
            return File.Exists(fullPath);
        }

        public static bool WriteFile(string fileName, string contents)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, fileName);

            try
            {
                File.WriteAllText(fullPath, contents);
                Debug.Log($"Written to {fullPath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {fullPath} with exception {e}");
                return false;
            }
        }

        public static bool ReadFile(string fileName, out string result)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, fileName);

            try
            {
                result = File.ReadAllText(fullPath);
                Debug.Log($"Read from {fullPath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from {fullPath} with exception {e}");
                result = "";
                return false;
            }
        }
    }
}