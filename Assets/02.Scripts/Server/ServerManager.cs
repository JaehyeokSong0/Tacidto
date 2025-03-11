using JaehyeokSong0.Tacidto.Application.Chat;
using JaehyeokSong0.Tacidto.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using VContainer;
using UniRx;
using Cysharp.Threading.Tasks;

namespace JaehyeokSong0.Tacidto.Server
{
    /// <summary>
    /// Dedicated / Linux 기반 서버를 관리합니다.
    /// 다음과 같은 실행 방식을 지원합니다:
    /// 1. Unity Editor - Scene을 통한 실행 (개발 및 테스트용)
    /// 2. Command Line - Command Line Argument를 통한 서버 설정 및 초기화 지원 (ex. ./Server.exe start -port 7777)
    /// 3. Executable - 실행 파일 직접 실행 시 CLI 모드로 전환 (설정 Argument 전달하지 않음)
    /// </summary>
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class ServerManager : MonoBehaviour
    {
        private const int TARGET_FRAME_RATE = 60;
        private readonly string IP_V4_ADDRESS = "0.0.0.0";

        private ushort _port = 2024;
        private bool _isServerRunning = false;
        private DateTime _serverStartTime;


        private ChatSystem _chatSystem;
        private NetworkManager _networkManager;
        private UnityTransport _transport;
        private Dictionary<string, CommandInfo> _commands;

        private CancellationTokenSource _cliCts;


        #region Event Functions
        [Inject]
        private void Construct(NetworkManager networkManager, ChatSystem chatSystem)
        {
            _networkManager = networkManager;
            _transport = _networkManager.GetComponent<UnityTransport>();
            _chatSystem = chatSystem;
        }

        private void Awake()
        {
#if UNITY_EDITOR
            DontDestroyOnLoad(this);
#endif
            InitializeCommands();
            DebugUtils.Log($"Transport initialized - Protocol: {_transport.Protocol}, ConnectionData: {_transport.ConnectionData.Address}:{_transport.ConnectionData.Port}");
        }

        private void Start()
        {
#if UNITY_EDITOR
            DebugUtils.Log("Instance started with symbol UNITY_EDITOR");
#endif
#if UNITY_SERVER
            DebugUtils.Log("Instance started with symbol UNITY_SERVER");
#endif

            // Editor에서는 바로 Server 실행. CLI에서는 커맨드 입력 대기.
#if UNITY_EDITOR
            StartServer();
#elif UNITY_SERVER
            InitializeFromCommandLine();
#endif
        }

        private void OnDestroy()
        {
            if (_cliCts != null)
            {
                _cliCts.Cancel();
                _cliCts.Dispose();
                _cliCts = null;
            }
            RemoveCallbacks();

            HandleExit();
        }
        #endregion


        #region Command System
        /// <summary>
        /// Unity Editor에서 서버 명령어를 실행하기 위한 개발/테스트용 메스드입니다.
        /// </summary>
        public bool ProcessEditorCommand(string[] command)
        {
#if UNITY_EDITOR
            return ProcessCommand(command);
#else
            return false;
#endif
        }

        private void InitializeCommands() // TODO : SO 혹은 ml로 포팅
        {
            try
            {
                _commands = new Dictionary<string, CommandInfo>
                {
                    { "help", new CommandInfo("help", "Show command list", CommandType.Global, HandleHelp) },
                    { "exit", new CommandInfo("exit", "Exit program", CommandType.Global, HandleExit) },
                    { "status", new CommandInfo("status", "Show server status", CommandType.Global, HandleStatus) },
                    { "start", new CommandInfo("start [-port <number>]", "Start server with optional port number", CommandType.PreServer, HandleStart) },
                    { "notice", new CommandInfo("notice <message>", "Send notice to all clients", CommandType.Running, HandleNotice) },
                    { "shutdown", new CommandInfo("shutdown", "Shutdown server", CommandType.Running, HandleShutdown) }
                };
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"Failed to initialize server commands : {ex}");
            }

            DebugUtils.Log("Server commands has been initailized successfully.");
        }

        /// <summary>
        /// 현재 사용 가능한 모든 명령어를 출력합니다. 
        /// </summary>
        private void DisplayAvailableCommands()
        {
            // Global
            DebugUtils.Log($"[Available Commands]\n");
            foreach (var cmd in _commands.Values.Where(c => c.Type == CommandType.Global))
            {
                DebugUtils.Log($"{cmd.Usage}\t{cmd.Description}");
            }

            // PreServer
            if (_isServerRunning == false)
            {
                foreach (var cmd in _commands.Values.Where(c => c.Type == CommandType.PreServer))
                {
                    DebugUtils.Log($"{cmd.Usage}\t{cmd.Description}");
                }
            }
            // Running
            else
            {
                foreach (var cmd in _commands.Values.Where(c => c.Type == CommandType.Running))
                {
                    DebugUtils.Log($"{cmd.Usage}\t{cmd.Description}");
                }
            }

            DebugUtils.Log("\n");
        }

        /// <summary>
        /// 서버의 실행 모드를 초기화합니다.
        /// 다음의 두 가지 실행 방식을 지원합니다:
        /// 1. Command Line - Argument가 제공된 경우 해당 명령을 처리합니다.
        /// 2. Executable - Argument를 입력받지 않은 것으로 간주하며 즉시 CLI 모드로 전환됩니다.
        /// 두 경우 모두 CLI를 시작하여 사용자가 서버 실행 중에도 명령을 입력할 수 있도록 합니다.
        /// </summary>
        private void InitializeFromCommandLine()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            // 경로(args[0]) 제거
            if (args.Length > 1)
            {
                args = args.Skip(1).ToArray();
            }
            else
            {
                args = new string[0];
            }

            ProcessCommand(args); // argument 있는 경우 처리
            StartCLI();
        }

        /// <summary>
        /// 입력받은 명령어를 처리합니다.
        /// </summary>
        /// <returns>명령어 처리가 성공한 경우 true</returns>
        private bool ProcessCommand(string[] args)
        {
            if (args.Length == 0)
            {
                return false;
            }

            string command = args[0].ToLower();

            if (_commands.TryGetValue(command, out var cmdInfo))
            {
                if (_isServerRunning == false
                    && cmdInfo.Type == CommandType.Running)
                {
                    DebugUtils.LogError("This command is only available when server is running.");
                    return false;
                }
                if (_isServerRunning && cmdInfo.Type == CommandType.PreServer)
                {
                    DebugUtils.LogError("This command is only available before server starts.");
                    return false;
                }

                cmdInfo.Handler(args.Skip(1).ToArray());

                return true;
            }

            DebugUtils.LogError($"Unknown command: {command}");

            return false;
        }

        private void StartCLI()
        {
            DisplayAvailableCommands();

            if (_cliCts != null)
            {
                _cliCts.Cancel();
                _cliCts.Dispose();
                _cliCts = null;
            }
            _cliCts = new CancellationTokenSource();

            ProcessCLIInputAsync(_cliCts.Token).Forget();
        }

        /// <summary>
        /// 사용자 CLI 입력을 비동기적으로 처리합니다.
        /// Input 쓰레드를 분리하여 네트워킹 기능과의 쓰레드 블로킹을 방지합니다.
        /// </summary>
        private async UniTaskVoid ProcessCLIInputAsync(CancellationToken token = default)
        {
            try
            {
                await UniTask.RunOnThreadPool(async () =>
                {
                    while (token.IsCancellationRequested == false)
                    {
                        string input = Console.ReadLine().Trim();

                        if (string.IsNullOrEmpty(input) == false)
                        {
                            string[] args = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            await UniTask.SwitchToMainThread(token);

                            ProcessCommand(args);

                            await UniTask.SwitchToThreadPool();
                        }
                    }
                }, cancellationToken: token);
            }
            catch (Exception ex)
            {
                await UniTask.SwitchToMainThread();
                DebugUtils.LogError($"CLI Processing Error : {ex.Message}");
            }
        }
#endregion


        #region Command Handlers
        // CommandType.Global
        private void HandleHelp(string[] args)
        {
            DisplayAvailableCommands();
        }
        private void HandleExit(string[] args = null)
        {
            if (_isServerRunning == true)
            {
                _chatSystem.ShutDown();
                _networkManager.Shutdown();
            }

            DebugUtils.Log("Exiting program.");

#if UNITY_SERVER && !UNITY_EDITOR
            System.Diagnostics.Process.GetCurrentProcess().Kill();
#else
            UnityEngine.Application.Quit();
#endif
        }

        private void HandleStatus(string[] args)
        {
            DebugUtils.Log("\n[Server Status]");

            if (_isServerRunning == false)
            {
                DebugUtils.LogError("Server is not running.");
                DebugUtils.LogError($"Current port: {_port}");
                return;
            }

            TimeSpan uptime = DateTime.Now - _serverStartTime;

            DebugUtils.Log($"Status: Running");
            DebugUtils.Log($"Uptime: {uptime.Days}d {uptime:hh\\:mm\\:ss}");
            DebugUtils.Log($"Port: {_port}");
            DebugUtils.Log($"Target Frame Rate: {TARGET_FRAME_RATE}");

            DebugUtils.Log($"\n[Network Status]");
            DebugUtils.Log($"Connected Clients: {_networkManager.ConnectedClientsList.Count}");
            DebugUtils.Log($"Transport: {_transport.GetType().Name}");

            if (_transport is UnityTransport unityTransport)
            {
                DebugUtils.Log($"Heartbeat Timeout: {unityTransport.HeartbeatTimeoutMS}ms");
            }

            DebugUtils.Log($"\n[System Resources]");
            DebugUtils.Log($"Memory Usage: {GC.GetTotalMemory(false) / 1024 / 1024}MB\n");
        }

        // CommandType.PreServer
        private void HandleStart(string[] args)
        {
            if (args.Length >= 2
                && args[0] == "-port"
                && ushort.TryParse(args[1], out ushort port))
            {
                _port = port;
                DebugUtils.Log($"Port set to: {port}");
            }

            StartServer();
        }

        // CommandType.Running
        private void HandleNotice(string[] args)
        {
            if (args.Length == 0)
            {
                DebugUtils.LogError("Usage: notice <message>");
                return;
            }

            string message = $"[NOTICE] {string.Join(" ", args)}";
            if(_chatSystem.SendSystemMessage(message) == true)
            {
                DebugUtils.Log($"Notice sent: {message}");
            }
        }

        private void HandleShutdown(string[] args)
        {
            if (_isServerRunning == false)
            {
                DebugUtils.LogError("Server is not running.");
                return;
            }

            try
            {
                _chatSystem.SendSystemMessage("[NOTICE] Server is shutting down...");
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"Failed to send shutdown message: {ex.Message}");
                DebugUtils.LogError($"Exception Details: {ex}");
            }

            _chatSystem.ShutDown();
            _networkManager.Shutdown();
            RemoveCallbacks();

            _isServerRunning = false;

            DebugUtils.Log("Server has been shut down.");
        }
        #endregion


        private void StartServer()
        {
            DebugUtils.Log($"Starting server...");
            DebugUtils.Log($"Transport state before SetConnectionData: Protocol={_transport.Protocol}, MaxPacketSize={_transport.MaxPacketQueueSize}");

            UnityEngine.Application.targetFrameRate = TARGET_FRAME_RATE;

            _transport.SetConnectionData(IP_V4_ADDRESS, _port);

            DebugUtils.Log($"Transport state after SetConnectionData: Address={_transport.ConnectionData.Address}, Port={_transport.ConnectionData.Port}");

            SetupCallbacks();


            if (_networkManager.StartServer() == true)
            {
                _serverStartTime = DateTime.Now;
                _isServerRunning = true;
                _chatSystem.Initialize();
            }
            else
            {
                RemoveCallbacks();
                DebugUtils.LogError($"Server initialization failed : {IP_V4_ADDRESS}:{_port}");
            }
        }

        /// <summary>
        /// Callback을 설정합니다.
        /// </summary>
        private void SetupCallbacks()
        {
            if (_networkManager == null)
            {
                DebugUtils.LogError("Setup callbacks failed. Cannot find NetworkManager.");
                return;
            }

            _networkManager.OnServerStarted += OnServerStarted;
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;

            DebugUtils.Log("Setup callbacks completed successfully.");
        }

        private void RemoveCallbacks()
        {
            if (_networkManager == null)
            {
                DebugUtils.LogError("Remove callbacks failed. Cannot find NetworkManager.");
                return;
            }

            _networkManager.OnServerStarted -= OnServerStarted;
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        #region Callbacks
        private void OnServerStarted()
        {
            DebugUtils.Log($"Server started : {IP_V4_ADDRESS}:{_port}");
        }
        private void OnClientConnected(ulong clientID)
        {
            DebugUtils.Log($"Client Connected / ID : {clientID}");
        }
        private void OnClientDisconnected(ulong clientID)
        {
            DebugUtils.Log($"Client Disconnected / ID : {clientID}");
        }
        #endregion
    }
}