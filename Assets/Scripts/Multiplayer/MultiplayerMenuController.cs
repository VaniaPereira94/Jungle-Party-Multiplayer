using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer
{
    /// <summary>
    /// Trata das interações do utilizador com o menu de multiplayer.
    /// </summary>
    //public class MultiplayerMenuController : NetworkBehaviour
    public class MultiplayerMenuController : MonoBehaviour
    {
        /* ATRIBUTOS */

        // referências para objetos de UI
        [Header("UI - Menu")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private TMP_InputField _privateLobbyCodeInput;
        [SerializeField] private TMP_InputField _publicLobbyCodeInput;

        [Header("UI - Create Lobby")]
        [SerializeField] private GameObject _createLobbyPanel;

        [Header("UI - Join Lobby")]
        [SerializeField] private GameObject _joinLobbyPanel;
        [SerializeField] private GameObject _joinPrivateLobbyPopup;
        [SerializeField] private GameObject _gameLobbyPanel;
        [SerializeField] private TextMeshProUGUI _gameLobbyCodeText;

        [Header("UI - Leaderboard")]
        [SerializeField] private GameObject _leaderboardPanel;
        [SerializeField] private GameObject _privateScorePanel;
        [SerializeField] private GameObject _publicScorePanel;

        // referências para outros controladores
        //[Header("Controllers")]
        //[SerializeField] private GameController _gameController;


        /* MÉTODOS */

        private void Start()
        {
            //_gameController = GameController.Instance;

            InitializeMultiplayer();
        }

        private async void InitializeMultiplayer()
        {
            await AuthController.Instance.Connect();
            await AuthController.Instance.AuthenticateAnonymous();
        }

        public void OpenCreateLobby()
        {
            _menuPanel.SetActive(false);
            _createLobbyPanel.SetActive(true);
        }

        public void CloseCreateLobby()
        {
            _menuPanel.SetActive(true);
            _createLobbyPanel.SetActive(false);
        }

        public async void CreatePrivateLobby()
        {
            string lobbyName = "abcde";
            await LobbyController.Instance.CreatePrivateLobby(lobbyName);
            OpenGameLobby();
        }

        public async void CreatePublicLobby()
        {
            bool isSuccess = await MultiplayerController.Instance.CreatePublicLobby();

            if (isSuccess)
            {
                string gameCode = await MultiplayerController.Instance.CreateGame();

                if (gameCode != null)
                {
                    OpenGameLobby();
                }
            }
        }

        public void OpenJoinLobby()
        {
            _menuPanel.SetActive(false);
            _joinLobbyPanel.SetActive(true);

            //await _lobbyController.ListPublicLobbies();
        }

        public void CloseJoinLobby()
        {
            _menuPanel.SetActive(true);
            _joinLobbyPanel.SetActive(false);
        }

        public void OpenAuthentication()
        {
            _joinLobbyPanel.SetActive(false);
            _joinPrivateLobbyPopup.SetActive(true);
        }

        public void CloseAuthentication()
        {
            _joinLobbyPanel.SetActive(true);
            _joinPrivateLobbyPopup.SetActive(false);
        }

        public async void JoinPrivateLobby()
        {
            string lobbyCode = _privateLobbyCodeInput.text;
            await LobbyController.Instance.JoinPrivateLobby(lobbyCode);
        }

        public async void JoinPublicLobby()
        {
            string lobbyCode = _publicLobbyCodeInput.text;
            bool isSuccess = await MultiplayerController.Instance.JoinPublicLobby(lobbyCode);

            if (isSuccess)
            {
                OpenGameLobby();
                _gameLobbyCodeText.text = $"Código da sala: {MultiplayerController.Instance.GetLobbyCode()}";
            }
        }

        public async void PlayGame()
        {
            string gameCode = _publicLobbyCodeInput.text;
            bool isSuccess = await MultiplayerController.Instance.PlayGame(gameCode);

            if (isSuccess)
            {
                SceneManager.LoadSceneAsync("Level2MultiplayerScene");
            }
        }

        public void StartGame()
        {
            SceneManager.LoadSceneAsync("Level2MultiplayerScene");
        }

        public void OpenLeaderboard()
        {
            _menuPanel.SetActive(false);
            _leaderboardPanel.SetActive(true);
        }

        public void OpenGameLobby()
        {
            _gameLobbyPanel.SetActive(true);
        }

        public void CloseLeaderboard()
        {
            _menuPanel.SetActive(true);
            _leaderboardPanel.SetActive(false);
        }

        public void OpenScorePrivate()
        {
            _leaderboardPanel.SetActive(false);
            _privateScorePanel.SetActive(true);
        }

        public void CloseScorePrivate()
        {
            _leaderboardPanel.SetActive(true);
            _privateScorePanel.SetActive(false);
        }

        public void OpenScorePublic()
        {
            _leaderboardPanel.SetActive(false);
            _publicScorePanel.SetActive(true);
        }

        public void CloseScorePublic()
        {
            _leaderboardPanel.SetActive(true);
            _publicScorePanel.SetActive(false);
        }

        public static void ShowError(string message)
        {
            Debug.LogError(message);
        }
    }
}