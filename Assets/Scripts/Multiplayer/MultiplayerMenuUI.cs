using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;


namespace Multiplayer
{
    public class MultiplayerMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _playerRow;
        [SerializeField] private Button _readyButton;       // botão de jogar no painel de cada lobby


        private void OnEnable()
        {
            _readyButton.onClick.AddListener(OnReadyClicked);

            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }

        private void OnDisable()
        {
            _readyButton.onClick.RemoveAllListeners();

            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }

        private void Start()
        {

        }

        private void OnLobbyUpdated(Lobby lobby)
        {

        }

        public void OnReadyClicked()
        {
            _readyButton.gameObject.SetActive(false);
        }
    }
}