#if UNITY_EDITOR
using JaehyeokSong0.Tacidto.Server;
using UnityEditor;
using UnityEngine;

namespace JaehyeokSong0.Editor.Server
{
    public class ServerCommandWindow : EditorWindow
    {
        private ServerManager _serverManager;
        private string _input;


        [MenuItem("CustomEditor/Server/Command Input")]
        public static void ShowWindow()
        {
            GetWindow<ServerCommandWindow>("Server Command");
        }


        private void OnGUI()
        {
            if (_serverManager == null)
            {
                _serverManager = FindAnyObjectByType<ServerManager>();   
            }

            EditorGUILayout.BeginHorizontal();
            _input = EditorGUILayout.TextField(_input);

            if (GUILayout.Button("Execute", GUILayout.Width(70)) 
                || Event.current.isKey == true
                && Event.current.keyCode == KeyCode.Return  // Enter 키 인식
                && Event.current.type == EventType.KeyDown)
            {
                if (string.IsNullOrEmpty(_input) == false)
                {
                    string[] args = _input.Split(' ');
                    _serverManager.ProcessEditorCommand(args);

                    _input = "";
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif