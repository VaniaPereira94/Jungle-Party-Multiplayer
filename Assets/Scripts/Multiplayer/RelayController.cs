using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;


namespace Multiplayer
{
    public class RelayController : SingletonMonoBehaviour<RelayController>
    {
        /* ATRIBUTOS E PROPRIEDADES */

        private string _joinCode;
        private string _ip;
        private int _port;
        private Guid _allocationId;
        private byte[] _connectionData;


        /* MÉTODOS */

        public string GetAllocationId()
        {
            return _allocationId.ToString();
        }

        public string GetConnectionData()
        {
            return _connectionData.ToString();
        }

        public async Task<string> CreateRelay(int maxConnections)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(
                    relayServerEndpoint => relayServerEndpoint.ConnectionType == "dtls"
                );

                _ip = dtlsEndpoint.Host;
                _port = dtlsEndpoint.Port;
                _allocationId = allocation.AllocationId;
                _connectionData = allocation.ConnectionData;

                Debug.Log("Criou jogo! Código do jogo: " + _joinCode);
                return _joinCode;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
                return null;
            }
        }

        public async Task<bool> JoinRelay(string joinCode)
        {
            try
            {
                _joinCode = joinCode;

                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);

                RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(
                    relayServerEndpoint => relayServerEndpoint.ConnectionType == "dtls"
                );

                _ip = dtlsEndpoint.Host;
                _port = dtlsEndpoint.Port;
                _allocationId = allocation.AllocationId;
                _connectionData = allocation.ConnectionData;

                Debug.Log("Entrou no jogo! Código do jogo: " + _joinCode);
                return true;
            }
            catch (LobbyServiceException exception)
            {
                MultiplayerMenuController.ShowError(exception.Message);
                return false;
            }
        }
    }
}