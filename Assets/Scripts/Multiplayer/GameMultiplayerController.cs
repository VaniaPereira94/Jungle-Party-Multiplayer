using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;


public class GameMultiplayerController : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;

        if (RelayController.Instance.IsHost)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApproval;
            (byte[] allocationId, byte[] key, byte[] connectionData, string ip, int port) = RelayController.Instance.GetHostConnectionInfo();
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(ip, (ushort)port, allocationId, key, connectionData, true);
            NetworkManager.Singleton.StartHost();
            Debug.Log("StartHost()");
        }
        else
        {
            (byte[] allocationId, byte[] key, byte[] connectionData, byte[] hostConnectionData, string ip, int port) = RelayController.Instance.GetClientConnectionInfo();
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(ip, (ushort)port, allocationId, key, connectionData, hostConnectionData, true);
            NetworkManager.Singleton.StartClient();
            Debug.Log("StartClient()");
        }
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log("Player conectado:" + request.ClientNetworkId);
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Pending = false;
    }
}