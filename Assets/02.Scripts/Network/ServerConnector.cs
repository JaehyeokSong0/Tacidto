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
    /// </summary>
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class ServerConnector : MonoBehaviour
    {
        private readonly string SERVER_IP = "127.0.0.1"; // localhost
        private readonly ushort SERVER_PORT = 2222;
        private const float TIME_OUT = 10f;

        private NetworkManager _networkManager;
        private UnityTransport _transport;

        private UniTaskCompletionSource<bool> _connectionCompletion;

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
            _transport = GetComponent<UnityTransport>();
        }

        public async UniTask<bool> ConnectToServer(CancellationToken cancellationToken = default)
        {
            _connectionCompletion = new UniTaskCompletionSource<bool>();
            _transport.SetConnectionData(SERVER_IP, SERVER_PORT);

            SetCallback();

            if (_networkManager.StartClient() == true)
            {
                DebugUtility.Log("StartClient Success");
            }
            else
            {
                DebugUtility.Log("StartClient Failed");
            }

            try
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(TIME_OUT));
                    return await _connectionCompletion.Task.AttachExternalCancellation(cts.Token);
                }
            }
            catch (Exception ex)
            {
                DebugUtility.LogError($"Exception in ServerConnector : {ex}");
                return false;
            }
            finally
            {
                ResetCallback();
            }
        }


        #region Callbacks
        private void SetCallback()
        {
            _networkManager.OnClientConnectedCallback +=
                (clientID) => OnClientConnected(clientID);

            _networkManager.OnClientDisconnectCallback +=
                (clientID) => OnClientDisconnected(clientID);
        }

        private void ResetCallback()
        {
            _networkManager.OnClientConnectedCallback -=
                (clientID) => OnClientConnected(clientID);

            _networkManager.OnClientDisconnectCallback -=
                (clientID) => OnClientDisconnected(clientID);
        }

        private void OnClientConnected(ulong clientID)
        {
            DebugUtility.Log($"Connected to Server / ID : {clientID}");
        }

        private void OnClientDisconnected(ulong clientID)
        {
            DebugUtility.Log($"Disconnected from Server / ID : {clientID}");
        }
        #endregion
    }
}