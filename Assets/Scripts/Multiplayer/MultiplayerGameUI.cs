using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;


namespace Multiplayer
{
    public class MultiplayerGameUI : MonoBehaviour
    {
        [SerializeField] private Button _initButton;        // botão de iniciar no painel de introdução de cada nível
        [SerializeField] private GameObject _introPanel;
        [SerializeField] private MonoBehaviour _levelController;


        private void OnEnable()
        {
            if (MultiplayerController.Instance.IsHost)
            {
                _initButton.onClick.AddListener(OnInitClicked);
            }

            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }

        private void OnDisable()
        {
            if (MultiplayerController.Instance.IsHost)
            {
                _initButton.onClick.RemoveAllListeners();
            }

            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }

        private void Start()
        {
            _initButton.gameObject.SetActive(false);
        }

        private void OnLobbyUpdated(Lobby lobby)
        {

        }

        public async void OnInitClicked()
        {
            bool isSuccess = await MultiplayerController.Instance.SetPlayerReady();

            if (isSuccess)
            {
                ILevelController currentLevelController = GetCurrentLevelController();

                if (currentLevelController != null)
                {
                    currentLevelController.InitAfterIntro();
                }

                //SetPlayerReady
            }
        }

        private ILevelController GetCurrentLevelController()
        {
            // obtém o tipo da classe do LevelController atual
            string levelControllerClassName = "Level" + GameController.Instance.CurrentLevelID + "Controller";
            Type levelControllerType = Type.GetType(levelControllerClassName);

            if (levelControllerType != null)
            {
                MonoBehaviour currentLevelController = _levelController;
                return currentLevelController as ILevelController;
            }
            else
            {
                return null;
            }
        }
    }
}