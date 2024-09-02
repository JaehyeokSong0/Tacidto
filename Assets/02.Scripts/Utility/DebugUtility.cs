using System;
using UnityEngine;

namespace JaehyeokSong0.Tacidto.Utility
{
    public static class DebugUtility
    {
        public static void Log(string msg, Color color = default)
        {
#if UNITY_EDITOR
            if (color.Equals(default) == true)
            {
                Debug.Log(msg);
            }
            else
            {
                Debug.Log($"<color={color}>{msg}</color>");
            }
#elif UNITY_SERVER
            Console.WriteLine($"[{System.DateTime.Now}] : {msg}");
#endif
        }

        public static void LogError(string msg, Color color = default)
        {
#if UNITY_EDITOR
            if (color.Equals(default) == true)
            {
                Debug.LogError($"<color=red>{msg}</color>");
            }
            else
            {
                Debug.LogError($"<color={color}>{msg}</color>");
            }
#elif UNITY_SERVER
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{System.DateTime.Now}] : {msg}");
                Console.ResetColor();
#endif
        }
    }
}