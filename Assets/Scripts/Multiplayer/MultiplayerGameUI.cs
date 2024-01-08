using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;


public class MultiplayerGameUI : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _levelController;


    private void OnEnable()
    {
        LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
    }

    private void OnDisable()
    {
        LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
    }

    private void Start()
    {

    }

    private void OnLobbyUpdated(Lobby lobby)
    {

    }

    //public async void OnInitClicked()
    //{
    //    bool isSuccess = await MultiplayerController.Instance.SetPlayerReady();

    //    if (isSuccess)
    //    {
    //        ILevelController currentLevelController = GetCurrentLevelController();

    //        if (currentLevelController != null)
    //        {
    //            currentLevelController.InitAfterIntro();
    //        }
    //    }
    //}

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