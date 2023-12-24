using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    /* ATRIBUTOS E PROPRIEDADES */

    public static LobbyController Instance { get; private set; }

    private Lobby _hostLobby;
    private Lobby _joinedLobby;

    private float _heartbeatTimer;
    private float _lobbyUpdateTimer;

    private const int _MAX_PLAYERS_IN_LOOBY = 2;
    private string _playerName;


    /* MÉTODOS */

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //_playerName = "CodeMonkey" + UnityEngine.Random.Range(10, 99);
        //Debug.Log("Current player name:" + _playerName);
    }

    public void GetPlayerId(string playerId)
    {
        Debug.Log("Player ID: " + playerId);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    public async Task CreatePrivateLobby(string lobbyName)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = true;
            //options.Player = GetPlayer();

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, _MAX_PLAYERS_IN_LOOBY, options);

            //ListPlayersOfLobby(lobby);

            _hostLobby = lobby;
            //_joinedLobby = _hostLobby;

            Debug.Log("Sala privada criada! " + lobby.Id + " - " + lobby.Name + " - " + lobby.MaxPlayers + " - " + lobby.LobbyCode);
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    public async Task CreatePublicLobby(string lobbyName)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Player = GetPlayer();

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, _MAX_PLAYERS_IN_LOOBY, options);

            ListPlayersOfLobby(lobby);

            _hostLobby = lobby;
            //_joinedLobby = _hostLobby;

            Debug.Log("Sala pública criada! " + lobby.Id + " - " + lobby.Name + " - " + lobby.MaxPlayers + " - " + lobby.LobbyCode);
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    public async Task JoinPrivateLobby(string lobbyCode)
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            _joinedLobby = lobby;

            Debug.Log("Entrou na sala privada!");

            ListPlayersOfLobby(_joinedLobby);
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    public async Task JoinPublicLobby(string lobbyCode)
    {
        try
        {
            //JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            //{
            //    Player = GetPlayer()
            //};

            await LobbyService.Instance.QuickJoinLobbyAsync();

            Debug.Log("Entrou na sala pública!");

            ListPlayersOfLobby(_joinedLobby);
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    public async Task ListPublicLobbies()
    {
        try
        {
            // filtros para a procura de lobbies
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
            };
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(false,QueryOrder.FieldOptions.Created)
            };

            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Salas encontradas: " + response.Results.Count);

            foreach (Lobby lobby in response.Results)
            {
                Debug.Log(lobby.Name + " - " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    public void ListPlayersOfLobby(Lobby lobby)
    {
        Debug.Log("Players in sala: " + lobby.Name + " - " + lobby.Players.Count);

        foreach (Player player in lobby.Players)
        {
            Debug.Log("Player: " + player.Id);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            _hostLobby = await Lobbies.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public,gameMode) }
                }
            });

            _joinedLobby = _hostLobby;

            ListPlayersOfLobby(_hostLobby);
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (_hostLobby != null)
        {
            _heartbeatTimer -= Time.deltaTime;

            if (_heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                _heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (_joinedLobby != null)
        {
            _lobbyUpdateTimer -= Time.deltaTime;

            if (_lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                _lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                _joinedLobby = lobby;
            }
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
            }
        };
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        _playerName = newPlayerName;

        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
                }
            });
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogError(exception.Message);
        }
    }
}