//using System.Collections.Generic;
//using Unity.Services.Authentication;
//using Unity.Services.Core;
//using Unity.Services.Lobbies;
//using Unity.Services.Lobbies.Models;
//using UnityEngine;

//public class MultiplayerController : MonoBehaviour
//{
//    private Lobby _hostLobby;
//    private Lobby _joinedLobby;

//    private float _heartbeatTimer;
//    private float _lobbyUpdateTimer;

//    private string _playerName;


//    private async void Start()
//    {
//        await UnityServices.InitializeAsync();

//        AuthenticationService.Instance.SignedIn += () =>
//        {
//            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
//        };

//        await AuthenticationService.Instance.SignInAnonymouslyAsync();

//        _playerName = "CodeMonkey" + UnityEngine.Random.Range(10, 99);
//        Debug.Log("Current player name:" + _playerName);
//    }

//    private void Update()
//    {
//        HandleLobbyHeartbeat();
//        HandleLobbyPollForUpdates();
//    }

//    public async void CreatePublicLobby()
//    {
//        try
//        {
//            string lobbyName = "MyLobby";
//            int maxPlayers = 2;

//            CreateLobbyOptions options = new CreateLobbyOptions
//            {
//                IsPrivate = false,
//                Player = GetPlayer(),
//                Data = new Dictionary<string, DataObject>
//                {
//                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag") },
//                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, "Map1") }
//                }
//            };

//            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

//            ListPlayersOfLobby(lobby);

//            _hostLobby = lobby;
//            _joinedLobby = _hostLobby;

//            Debug.Log("Created public lobby! " + lobby.Id + " - " + lobby.Name + " - " + lobby.MaxPlayers + " - " + lobby.LobbyCode);
//        }
//        catch (LobbyServiceException exception)
//        {
//            Debug.LogError(exception.Message);
//        }
//    }

//    public async void CreatePrivateLobby()
//    {
//        try
//        {
//            //Debug.Log("Created private lobby! " + lobby.Id + " - " + lobby.Name + " - " + lobby.MaxPlayers + " - " + lobby.LobbyCode);
//        }
//        catch (LobbyServiceException exception)
//        {
//            Debug.LogError(exception.Message);
//        }
//    }

//    public async void ListLobbies()
//    {
//        try
//        {
//            // filtros para ordenar a lista de lobbies
//            QueryLobbiesOptions options = new QueryLobbiesOptions
//            {
//                Count = 25,
//                Filters = new List<QueryFilter>
//                {
//                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
//                },
//                Order = new List<QueryOrder>
//                {
//                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
//                }
//            };

//            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();

//            Debug.Log("Lobbies found: " + response.Results.Count);

//            foreach (Lobby lobby in response.Results)
//            {
//                Debug.Log(lobby.Name + " - " + lobby.MaxPlayers + " - " + lobby.Data["GameMode"].Value);
//            }
//        }
//        catch (LobbyServiceException exception)
//        {
//            Debug.LogError(exception.Message);
//        }
//    }

//    public async void JoinPublicLobby(string lobbyCode)
//    {
//        try
//        {
//            await LobbyService.Instance.QuickJoinLobbyAsync();

//            Debug.Log("Lobby public joined");
//        }
//        catch (LobbyServiceException exception)
//        {
//            Debug.LogError(exception.Message);
//        }
//    }

//    public async void JoinPrivateLobby(string lobbyCode)
//    {
//        try
//        {
//            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
//            {
//                Player = GetPlayer()
//            };

//            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
//            _joinedLobby = lobby;

//            Debug.Log("Lobby private joined: " + lobbyCode);

//            ListPlayersOfLobby(_joinedLobby);
//        }
//        catch (LobbyServiceException exception)
//        {
//            Debug.LogError(exception.Message);
//        }
//    }

//    public void ListPlayersOfLobby(Lobby lobby)
//    {
//        Debug.Log("Players in lobby: " + lobby.Name + " - " + lobby.Players.Count + " - " + lobby.Data["GameMode"].Value + " - " + lobby.Data["Map"].Value);

//        foreach (Player player in lobby.Players)
//        {
//            Debug.Log("Player: " + player.Id + " - " + player.Data["PlayerName"].Value);
//        }
//    }

//    private async void UpdateLobbyGameMode(string gameMode)
//    {
//        try
//        {
//            _hostLobby = await Lobbies.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
//            {
//                Data = new Dictionary<string, DataObject>
//                {
//                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public,gameMode) }
//                }
//            });

//            _joinedLobby = _hostLobby;

//            ListPlayersOfLobby(_hostLobby);
//        }
//        catch (LobbyServiceException exception)
//        {
//            Debug.LogError(exception.Message);
//        }
//    }

//    private async void HandleLobbyHeartbeat()
//    {
//        if (_hostLobby != null)
//        {
//            _heartbeatTimer -= Time.deltaTime;

//            if (_heartbeatTimer < 0f)
//            {
//                float heartbeatTimerMax = 15f;
//                _heartbeatTimer = heartbeatTimerMax;

//                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
//            }
//        }
//    }

//    private async void HandleLobbyPollForUpdates()
//    {
//        if (_joinedLobby != null)
//        {
//            _lobbyUpdateTimer -= Time.deltaTime;

//            if (_lobbyUpdateTimer < 0f)
//            {
//                float lobbyUpdateTimerMax = 1.1f;
//                _lobbyUpdateTimer = lobbyUpdateTimerMax;

//                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
//                _joinedLobby = lobby;
//            }
//        }
//    }

//    private Player GetPlayer()
//    {
//        return new Player
//        {
//            Data = new Dictionary<string, PlayerDataObject>
//            {
//                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
//            }
//        };
//    }

//    private async void UpdatePlayerName(string newPlayerName)
//    {
//        _playerName = newPlayerName;

//        try
//        {
//            await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
//            {
//                Data = new Dictionary<string, PlayerDataObject>
//                {
//                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
//                }
//            });
//        }
//        catch (LobbyServiceException exception)
//        {
//            Debug.LogError(exception.Message);
//        }
//    }
//}