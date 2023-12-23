using Unity.Services.Core;
using UnityEngine;

/// <summary>
/// Trata das interações do utilizador com o menu de multiplayer.
/// </summary>
public class MultiplayerMenuController : MonoBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    // referências para objetos de UI
    [Header("UI")]
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _createLobby;
    [SerializeField] private GameObject _score;
    [SerializeField] private GameObject _scorePublic;
    [SerializeField] private GameObject _scorePrivate;

    // referências para outros controladores
    [Header("Controllers")]
    [SerializeField] private AuthenticationController _authenticationController;
    //[SerializeField] private GameController _gameController;
    //[SerializeField] private LobbyController _lobbyController;


    /* MÉTODOS */

    private void Start()
    {
        _authenticationController = AuthenticationController.Instance;
        //_lobbyController = LobbyController.Instance;
        //_gameController = GameController.Instance;

        InitializeMultiplayer();
    }

    private async void InitializeMultiplayer()
    {
        await _authenticationController.Connect();
        await _authenticationController.AuthenticateAnonymous();
    }

    public void OpenCreateLobby()
    {
        _menu.SetActive(false);
        _createLobby.SetActive(true);
    }

    public void CloseCreateLobby()
    {
        _menu.SetActive(true);
        _createLobby.SetActive(false);
    }

    public void OpenScore()
    {
        _menu.SetActive(false);
        _score.SetActive(true);
    }

    public void CloseScore()
    {
        _menu.SetActive(true);
        _score.SetActive(false);
    }

    public void OpenScorePublic()
    {
        _score.SetActive(false);
        _scorePublic.SetActive(true);
    }

    public void CloseScorePublic()
    {
        _score.SetActive(true);
        _scorePublic.SetActive(false);
    }

    public void OpenScorePrivate()
    {
        _score.SetActive(false);
        _scorePrivate.SetActive(true);
    }

    public void CloseScorePrivate()
    {
        _score.SetActive(true);
        _scorePrivate.SetActive(false);
    }
}