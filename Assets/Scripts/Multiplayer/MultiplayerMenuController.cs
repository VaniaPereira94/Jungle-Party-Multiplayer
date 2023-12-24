using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Trata das interações do utilizador com o menu de multiplayer.
/// </summary>
public class MultiplayerMenuController : MonoBehaviour
{
    /* ATRIBUTOS */

    // referências para objetos de UI
    [Header("UI")]
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _createLobby;
    [SerializeField] private GameObject _joinLobby;
    [SerializeField] private TMP_InputField _privateLobbyCode;
    [SerializeField] private GameObject _leaderboard;
    [SerializeField] private GameObject _scorePublic;
    [SerializeField] private GameObject _scorePrivate;

    // referências para outros controladores
    [Header("Controllers")]
    [SerializeField] private AuthController _authController;
    [SerializeField] private LobbyController _lobbyController;
    //[SerializeField] private GameController _gameController;


    /* MÉTODOS */

    private void Start()
    {
        _authController = AuthController.Instance;
        _lobbyController = LobbyController.Instance;
        //_gameController = GameController.Instance;

        InitializeMultiplayer();
    }

    private async void InitializeMultiplayer()
    {
        await _authController.Connect();
        await _authController.AuthenticateAnonymous();
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

    public async void CreatePrivateLobby()
    {
        string lobbyName = "abcde";
        await _lobbyController.CreatePrivateLobby(lobbyName);
    }

    public async void CreatePublicLobby()
    {
        string lobbyName = "abcde";
        await _lobbyController.CreatePublicLobby(lobbyName);
    }

    public async void OpenJoinLobby()
    {
        _menu.SetActive(false);
        _joinLobby.SetActive(true);

        await _lobbyController.ListPublicLobbies();
    }

    public void CloseJoinLobby()
    {
        _menu.SetActive(true);
        _joinLobby.SetActive(false);
    }

    public async void JoinPrivateLobby()
    {
        string lobbyCode = _privateLobbyCode.text;
        await _lobbyController.JoinPrivateLobby(lobbyCode);
    }

    public async void JoinPublicLobby()
    {
        string lobbyCode = "a1b2c3";
        await _lobbyController.JoinPublicLobby(lobbyCode);
    }

    public void OpenLeaderboard()
    {
        _menu.SetActive(false);
        _leaderboard.SetActive(true);
    }

    public void CloseLeaderboard()
    {
        _menu.SetActive(true);
        _leaderboard.SetActive(false);
    }

    public void OpenScorePrivate()
    {
        _leaderboard.SetActive(false);
        _scorePrivate.SetActive(true);
    }

    public void CloseScorePrivate()
    {
        _leaderboard.SetActive(true);
        _scorePrivate.SetActive(false);
    }

    public void OpenScorePublic()
    {
        _leaderboard.SetActive(false);
        _scorePublic.SetActive(true);
    }

    public void CloseScorePublic()
    {
        _leaderboard.SetActive(true);
        _scorePublic.SetActive(false);
    }
}