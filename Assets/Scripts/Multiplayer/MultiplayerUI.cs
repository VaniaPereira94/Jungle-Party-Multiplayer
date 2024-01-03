using UnityEngine;
using TMPro;

namespace Multiplayer
{
    public class MultiplayerUI : Singleton<MultiplayerUI>
    {
        [SerializeField] private TextMeshProUGUI _lobbyCode;

        private void Start()
        {
            ShowLobbyCode();
        }

        public void ShowLobbyCode()
        {
            _lobbyCode.text = $"Code: {LobbyController.Instance.GetLobbyCode()}";
        }
    }
}