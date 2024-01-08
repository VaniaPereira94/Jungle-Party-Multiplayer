using System.Collections.Generic;
using UnityEngine;


public class LobbyMenuSpawner : MonoBehaviour
{
    [SerializeField] private List<LobbyPlayer> _players;

    private void OnEnable()
    {
        LobbyGameEvents.OnLobbyUpdated += OnLobbyUpdated;
    }

    private void OnDisable()
    {
        LobbyGameEvents.OnLobbyUpdated -= OnLobbyUpdated;
    }

    private void OnLobbyUpdated()
    {
        List<LobbyPlayerData> playerDatas = MultiplayerController.Instance.GetPlayers();

        for (int i = 0; i < playerDatas.Count; i++)
        {
            LobbyPlayerData data = playerDatas[i];
            _players[i].SetMenuData(data);
        }
    }
}