using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Trata das interações do utilizador com o menu de multiplayer.
/// </summary>
public class MultiplayerMenuController : MonoBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    [Header("UI - Menu")]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private TMP_InputField _publicLobbyCodeInput;

    [Header("UI - Create Lobby")]
    [SerializeField] private GameObject _createLobbyPanel;

    [Header("UI - Join Lobby")]
    [SerializeField] private GameObject _joinLobbyPanel;
    [SerializeField] private GameObject _joinPrivateLobbyPopup;

    [Header("UI - Game Lobby")]
    [SerializeField] private GameObject _gameLobbyPanel;
    [SerializeField] private TextMeshProUGUI _gameLobbyCodeText;
    [SerializeField] private Button _setReadyPlayerButton;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _previousMapButton;
    [SerializeField] private Button _nextMapButton;
    [SerializeField] private Image _mapCoverImage;
    [SerializeField] private MapListScriptable _mapListScriptable;
    private int _currentMapIndex = 0;

    [Header("UI - Leaderboard")]
    [SerializeField] private GameObject _leaderboardPanel;
    [SerializeField] private GameObject _privateScorePanel;
    [SerializeField] private GameObject _publicScorePanel;


    /* MÉTODOS */

    private void OnEnable()
    {
        _setReadyPlayerButton.onClick.AddListener(OnSetReadyPlayerClicked);
        LobbyGameEvents.OnLobbyUpdated += OnLobbyUpdated;
    }

    private void OnDisable()
    {
        _setReadyPlayerButton.onClick.RemoveAllListeners();
        _previousMapButton.onClick.RemoveAllListeners();
        _nextMapButton.onClick.RemoveAllListeners();
        _startGameButton.onClick.RemoveAllListeners();
        LobbyGameEvents.OnLobbyReady -= OnLobbyReadyClicked;
        LobbyGameEvents.OnLobbyUpdated -= OnLobbyUpdated;
    }

    private void Start()
    {
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
            OpenGameLobby();
            _gameLobbyCodeText.text = MultiplayerController.Instance.GetLobbyCode();

            if (MultiplayerController.Instance.IsHost)
            {
                _startGameButton.gameObject.SetActive(false);

                _previousMapButton.onClick.AddListener(OnPreviousMapClicked);
                _nextMapButton.onClick.AddListener(OnNextMapClicked);
                _startGameButton.onClick.AddListener(OnStartGameClicked);
                LobbyGameEvents.OnLobbyReady += OnLobbyReadyClicked;

                await MultiplayerController.Instance.SetMap(_currentMapIndex, _mapListScriptable.Maps[_currentMapIndex].SceneName);
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
        //string lobbyCode = _privateLobbyCodeInput.text;
        //await LobbyController.Instance.JoinPrivateLobby(lobbyCode);
    }

    public async void JoinPublicLobby()
    {
        string lobbyCode = "";

        if (MultiplayerController.Instance.IsHost)
        {
            lobbyCode = MultiplayerController.Instance.GetLobbyCode();
        }
        else
        {
            lobbyCode = _publicLobbyCodeInput.text;
        }

        bool isSuccess = await MultiplayerController.Instance.JoinPublicLobby(lobbyCode);

        if (isSuccess)
        {
            OpenGameLobby();
            _gameLobbyCodeText.text = MultiplayerController.Instance.GetLobbyCode();

            _previousMapButton.gameObject.SetActive(false);
            _nextMapButton.gameObject.SetActive(false);
            _startGameButton.gameObject.SetActive(false);
        }
    }

    public async void OnPreviousMapClicked()
    {
        if (_currentMapIndex - 1 > 0)
        {
            _currentMapIndex--;
        }
        else
        {
            _currentMapIndex = 0;
        }

        UpdateMapUI();
        await MultiplayerController.Instance.SetMap(_currentMapIndex, _mapListScriptable.Maps[_currentMapIndex].SceneName);
    }

    public async void OnNextMapClicked()
    {
        if (_currentMapIndex + 1 < _mapListScriptable.Maps.Count - 1)
        {
            _currentMapIndex++;
        }
        else
        {
            _currentMapIndex = _mapListScriptable.Maps.Count - 1;
        }

        UpdateMapUI();
        await MultiplayerController.Instance.SetMap(_currentMapIndex, _mapListScriptable.Maps[_currentMapIndex].SceneName);
    }

    private void UpdateMapUI()
    {
        _mapCoverImage.sprite = _mapListScriptable.Maps[_currentMapIndex].coverName;
    }

    public void OnLobbyUpdated()
    {
        _currentMapIndex = MultiplayerController.Instance.GetMapIndex();
        UpdateMapUI();
    }

    public async void OnSetReadyPlayerClicked()
    {
        bool isSuccess = await MultiplayerController.Instance.SetPlayerReady();

        if (isSuccess)
        {
            _setReadyPlayerButton.gameObject.SetActive(false);
        }
    }

    public void OnLobbyReadyClicked()
    {
        _startGameButton.gameObject.SetActive(true);
    }

    public async void OnStartGameClicked()
    {
        await MultiplayerController.Instance.StartGame();
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

    public void CloseGameLobby()
    {
        _gameLobbyPanel.SetActive(false);
        _createLobbyPanel.SetActive(false);
        _joinLobbyPanel.SetActive(false);
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