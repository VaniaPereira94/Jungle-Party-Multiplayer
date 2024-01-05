using UnityEngine;
using TMPro;

namespace Multiplayer
{
    public class MultiplayerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _lobbyCodeText;

        private void Start()
        {
            ShowLobbyCode();
        }

        public void ShowLobbyCode()
        {
            _lobbyCodeText.text = $"LobbyCode: {MultiplayerController.Instance.GetLobbyCode()}";
        }
    }
}