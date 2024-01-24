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

    [Header("UI - Game Lobby")]
    [SerializeField] private GameObject _gameLobbyPanel;
    [SerializeField] private TextMeshProUGUI _gameLobbyCodeText;
    [SerializeField] private Button _setReadyPlayerButton;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Image _mapCoverImage;
    [SerializeField] private MapListScriptable _mapListScriptable;
    private int _currentMapIndex = 0;


    /* MÉTODOS */

    private void OnEnable()
    {
        _setReadyPlayerButton.onClick.AddListener(OnSetReadyPlayerClicked);

        LobbyGameEvents.OnLobbyUpdated += OnLobbyUpdated;
    }

    private void OnDisable()
    {
        _setReadyPlayerButton.onClick.RemoveAllListeners();
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
        if (AuthController.Instance.CurrentPlayerId == null)
        {
            await AuthController.Instance.Connect();
            await AuthController.Instance.AuthenticateAnonymous();
        };
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
    }

    public void CloseJoinLobby()
    {
        _menuPanel.SetActive(true);
        _joinLobbyPanel.SetActive(false);
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

            _startGameButton.gameObject.SetActive(false);
        }
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

    public void OpenGameLobby()
    {
        _gameLobbyPanel.SetActive(true);
    }

    public void CloseGameLobby()
    {
        LobbyController.Instance.LeaveLobby();
        _gameLobbyPanel.SetActive(false);
        _createLobbyPanel.SetActive(false);
        _joinLobbyPanel.SetActive(false);
        _menuPanel.SetActive(true);
    }
}