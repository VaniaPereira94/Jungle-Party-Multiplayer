using UnityEngine;
using TMPro;

namespace Multiplayer
{
    public class LobbyPlayer : MonoBehaviour
    {
        //[SerializeField] private TextMeshPro _playerName;
        private LobbyPlayerData _lobbyPlayerData;

        public void SetData(LobbyPlayerData data)
        {
            _lobbyPlayerData = data;
            //_playerName.text = data.GamerTag;
            gameObject.SetActive(true);
        }
    }
}