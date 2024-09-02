using JaehyeokSong0.Tacidto.Utility;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace JaehyeokSong0.Tacidto.Server
{
    /// <summary>
    /// Dedicated / Linux 기반 서버를 관리합니다.
    /// </summary>
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class ServerManager : MonoBehaviour
    {
        private const int TARGET_FRAME_RATE = 60;
        private readonly string IP_V4_ADDRESS = "0.0.0.0";

        private ushort _port = 7777;
        private bool _isServerRunning = false;

        private NetworkManager _networkManager;
        private UnityTransport _transport;

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
            _transport = GetComponent<UnityTransport>();
        }

        private void Start()
        {
#if UNITY_EDITOR
            SetUpServer();
#elif UNITY_SERVER
            GetCommands();
#endif
        }

        #region CLI
        private void ShowCommands()
        {
            Console.WriteLine("==== Command List ====");
            Console.WriteLine("start [-port <number>] \t Start Server (optionally specify port)");
            Console.WriteLine("exit \t\t\t Exit Program\n");
        }

        private void GetCommands()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            if (ProcessCommandArguments(args) == false)
            {
                RunServerCLI();
            }
        }

        private bool ProcessCommandArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "start")
                {
                    if (i + 2 < args.Length && args[i + 1] == "-port" && ushort.TryParse(args[i + 2], out ushort port))
                    {
                        _port = port;
                        i += 2;
                    }
                    SetUpServer();
                    return true;
                }
                else if (args[i].ToLower() == "exit")
                {
                    Console.WriteLine("Exiting program.");
                    UnityEngine.Application.Quit();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    return false;
                }
            }
            return false;
        }

        private void RunServerCLI()
        {
            while (_isServerRunning == false)
            {
                ShowCommands();

                string input = Console.ReadLine().Trim();
                ProcessCommandArguments(input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        #endregion

        private void SetUpServer()
        {
            UnityEngine.Application.targetFrameRate = TARGET_FRAME_RATE;
            _transport.SetConnectionData(IP_V4_ADDRESS, _port);

            SetUpCallback();

            if (_networkManager.StartServer() == true)
            {
                _isServerRunning = true;
                DebugUtility.Log($"SetUpServer Success : {IP_V4_ADDRESS}:{_port}");
            }
            else
            {
                DebugUtility.Log($"SetUpServer Failed : {IP_V4_ADDRESS}:{_port}");
            }
        }

        private void SetUpCallback()
        {
            _networkManager.OnClientConnectedCallback +=
                (clientID) => DebugUtility.Log($"Client Connected / ID : {clientID}");

            _networkManager.OnClientDisconnectCallback +=
                (clientID) => DebugUtility.Log($"Client Disconnected / ID : {clientID}");
        }
    }
}