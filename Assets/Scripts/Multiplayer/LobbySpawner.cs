using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class LobbySpawner : MonoBehaviour
    {
        [SerializeField] private List<LobbyPlayer> _players;

        private void OnEnable()
        {
            LobbyGameEvents.OnLobbyUpdated += OnLobbtUpdated;
        }

        private void OnDisable()
        {
            LobbyGameEvents.OnLobbyUpdated -= OnLobbtUpdated;
        }

        private void OnLobbtUpdated()
        {
            List<LobbyPlayerData> playerDatas = MultiplayerController.Instance.GetPlayers();

            for (int i = 0; i < playerDatas.Count; i++)
            {
                LobbyPlayerData data = playerDatas[i];
                _players[i].SetData(data);
            }
        }
    }
}