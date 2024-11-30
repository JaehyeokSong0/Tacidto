using Cysharp.Threading.Tasks;
using JaehyeokSong0.Tacidto.Utility;
using System;
using System.Threading;
using UniRx;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace JaehyeokSong0.Tacidto.Network
{
    /// <summary>
    /// Client를 통해 Server에 접속합니다.
    /// ApplicationScope에서 Singleton으로 관리됩니다.
    /// </summary>
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class ServerConnector : MonoBehaviour
    {
        public enum ServerConnectionStatus
        {
            None,
            Connecting,
            Error,
            Connected,
            Disconnected,
        }

        public IReadOnlyReactiveProperty<ServerConnectionStatus> Status => _status;

        private const string SERVER_IP = "127.0.0.1"; // localhost
        private const ushort SERVER_PORT = 2024;
        private const float TIME_OUT = 10f;

        private NetworkManager _networkManager;
        private UnityTransport _transport;

        private UniTaskCompletionSource<bool> _connectionCompletion;

        private ReactiveProperty<ServerConnectionStatus> _status = new ReactiveProperty<ServerConnectionStatus>(ServerConnectionStatus.None);

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
            _transport = GetComponent<UnityTransport>();
        }

        public async UniTask<bool> ConnectToServer()
        {
            _status.Value = ServerConnectionStatus.Connecting;

            // 아직 진행중인 task가 존재할 때
            if (_connectionCompletion != null
                && _connectionCompletion.Task.Status.IsCompleted() == false)
            {
                DebugUtils.LogError("Connection task is already in progress");
                _status.Value = ServerConnectionStatus.Error;

                return false;
            }

            _connectionCompletion = new UniTaskCompletionSource<bool>();
            _transport.SetConnectionData(SERVER_IP, SERVER_PORT);
            SetCallback();

            if (_networkManager.StartClient() == false)
            {
                DebugUtils.LogError("StartClient Failed");
                _status.Value = ServerConnectionStatus.Error;
                ResetCallback();

                return false;
            }

            DebugUtils.Log("StartClient Success");

            try
            {
                // CancellationTokenSource은 IDisposable
                // TIME_OUT 이후 cancel 신호 발생
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIME_OUT)))
                {
                    bool result = await _connectionCompletion.Task.AttachExternalCancellation(cts.Token);

                    if (result == true)
                    {
                        _status.Value = ServerConnectionStatus.Connected;
                    }

                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                DebugUtils.LogError($"Connection timed out after {TIME_OUT} seconds");
                _status.Value = ServerConnectionStatus.Error;
                _networkManager.Shutdown();

                return false;
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"Exception in ServerConnector : {ex}");
                _status.Value = ServerConnectionStatus.Error;
                _networkManager.Shutdown();

                return false;
            }
            finally
            {
                ResetCallback();
                _connectionCompletion = null;
            }
        }

        #region Callbacks
        private void SetCallback()
        {
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void ResetCallback()
        {
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnClientConnected(ulong clientID)
        {
            DebugUtils.Log($"Connected to Server / ID : {clientID}");
            _connectionCompletion.TrySetResult(true);
        }

        private void OnClientDisconnected(ulong clientID)
        {
            DebugUtils.Log($"Disconnected from Server / ID : {clientID}");
            _connectionCompletion.TrySetResult(false);
        }
        #endregion
    }
}