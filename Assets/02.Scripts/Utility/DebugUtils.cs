using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEngine;
#elif UNITY_SERVER
using System;
#endif

namespace JaehyeokSong0.Tacidto.Utility
{
    public static class DebugUtils
    {
        public enum LogColor
        {
            white,
            red,
            green,
            blue,
            yellow
        }

        public static void Log(string msg, LogColor color = LogColor.white)
        {
#if UNITY_EDITOR
            if (color.Equals(LogColor.white) == true)
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

        public static void LogError(string msg, LogColor color = LogColor.red, [CallerMemberName] string callerMemberName = "")
        {
#if UNITY_EDITOR
            Debug.LogError($"<color={color}>{msg}</color>");
#elif UNITY_SERVER
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{System.DateTime.Now}] [{callerMemberName}] : {msg}");
            Console.ResetColor();
#endif
        }
    }
}