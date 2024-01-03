using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class TestSync : NetworkBehaviour
{
    private NetworkVariable<int> score = new();
    //public NetworkVariable<int> score = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Update()
    {
        if (Input.GetKey(KeyCode.F))
        {
            Debug.Log("Tecla Espaço pressionada");
            OnStateChangedServerRpc(10);
        }
        if (Input.GetKey(KeyCode.G))
        {
            Debug.Log("Tecla Espaço pressionada");
            OnStateChangedServerRpc(0);
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        score.OnValueChanged += OnStateChanged;
        base.OnNetworkSpawn();
    }

    public void OnStateChanged(int previous, int current)
    {
        if (score.Value > 0)
        {
            Debug.Log("numero maior que 0");
        }
        else
        {
            Debug.Log("numero igual a 0");
        }
    }

    [ServerRpc]
    public void OnStateChangedServerRpc(int newScore)
    {
        score.Value += newScore;
        Debug.Log("Valor local: " + score.Value);
    }

    //public void SetScore(int newScore)
    //{
    //    if (score.Value == 0)
    //    {
    //        // Modifica o valor sincronizado
    //        score.Value = Random.Range(1, 100);
    //    }
    //    else
    //    {
    //        score.Value = newScore;
    //    }

    //    // Exibe o valor localmente
    //    Debug.Log("Valor local: " + score.Value);
    //}
}