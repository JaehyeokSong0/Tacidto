using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using JaehyeokSong0.Tacidto.Utility;
using System;

namespace JaehyeokSong0.Tacidto.Network
{
    /// <summary>
    /// Client를 통해 Server에 접속합니다.
    /// ApplicationScope에서 Singleton으로 관리됩니다.
    /// </summary>
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class ServerConnector : MonoBehaviour
    {
        private const string SERVER_IP = "127.0.0.1"; // localhost
        private const ushort SERVER_PORT = 2024;
        private const float TIME_OUT = 10f;

        private NetworkManager _networkManager;
        private UnityTransport _transport;

        private UniTaskCompletionSource<bool> _connectionCompletion;
         
        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
            _transport = GetComponent<UnityTransport>();
        }

        public async UniTask<bool> ConnectToServer()
        {
            // 아직 진행중인 task가 존재할 때
            if (_connectionCompletion != null
                && _connectionCompletion.Task.Status.IsCompleted() == false)
            {
                DebugUtility.LogError("Connection task is already in progress");
                return false;
            }

            _connectionCompletion = new UniTaskCompletionSource<bool>();
            _transport.SetConnectionData(SERVER_IP, SERVER_PORT);
            SetCallback();

            if (_networkManager.StartClient() == false)
            {
                DebugUtility.LogError("StartClient Failed");
                ResetCallback();
                return false;
            }

            DebugUtility.Log("StartClient Success");

            try
            {
                // CancellationTokenSource은 IDisposable
                // TIME_OUT 이후 cancel 신호 발생
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIME_OUT)))
                {
                    return await _connectionCompletion.Task.AttachExternalCancellation(cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                DebugUtility.LogError($"Connection timed out after {TIME_OUT} seconds");
                _networkManager.Shutdown();
                return false;
            }
            catch (Exception ex)
            {
                DebugUtility.LogError($"Exception in ServerConnector : {ex}");
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
            DebugUtility.Log($"Connected to Server / ID : {clientID}");
            _connectionCompletion.TrySetResult(true);
        }

        private void OnClientDisconnected(ulong clientID)
        {
            DebugUtility.Log($"Disconnected from Server / ID : {clientID}");
            _connectionCompletion.TrySetResult(false);
        }
        #endregion
    }
}