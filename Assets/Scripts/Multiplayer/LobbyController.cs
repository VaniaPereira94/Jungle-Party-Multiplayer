using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Multiplayer
{
    public class LobbyController : Singleton<LobbyController>
    {
        /* ATRIBUTOS E PROPRIEDADES */

        private List<Lobby> _privateLobbies = new();
        private List<Lobby> _publicLobbies = new();

        private string _playerName;

        private Lobby _hostLobby;
        private Lobby _joinLobby;

        private float _heartbeatTimer;
        private float _lobbyUpdateTimer;

        private Coroutine _hearthbeatLobbyCoroutine;
        private Coroutine _refreshLobbyCoroutine;


        /* MÉTODOS */

        public string GetLobbyCode()
        {
            return _hostLobby?.LobbyCode;
        }

        public List<Dictionary<string, PlayerDataObject>> GetPlayersData()
        {
            List<Dictionary<string, PlayerDataObject>> playerData = new();

            foreach (Player player in _hostLobby.Players)
            {
                playerData.Add(player.Data);
            }

            return playerData;
        }

        public async Task CreatePrivateLobby(string lobbyName)
        {
            try
            {
                CreateLobbyOptions options = new CreateLobbyOptions();
                options.IsPrivate = true;
                //options.Player = GetPlayer();

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, options);

                _privateLobbies.Add(lobby);

                //ListPlayersOfLobby(lobby);

                //_hostLobby = lobby;
                //_joinedLobby = _hostLobby;

                Debug.Log("Sala privada criada! " + lobby.Id + " - " + lobby.Name + " - " + lobby.MaxPlayers + " - " + lobby.LobbyCode);
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
            }
        }

        public async Task<bool> CreatePublicLobby(string lobbyName, int maxPlayers, Dictionary<string, string> data)
        {
            try
            {
                Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
                Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);

                CreateLobbyOptions options = new();
                options.IsPrivate = false;
                options.Player = player;

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

                _hostLobby = lobby;

                _hearthbeatLobbyCoroutine = StartCoroutine(HearthbeatLobbyCorroutine(_hostLobby.Id, 6f));
                _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCorroutine(_hostLobby.Id, 1f));

                //_joinedLobby = _hostLobby;

                //ListPlayersOfLobby(lobby);

                Debug.Log("Sala pública criada! " + _hostLobby.Id + " - " + _hostLobby.Name + " - " + _hostLobby.MaxPlayers + " - " + _hostLobby.LobbyCode);
                return true;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
                return false;
            }
        }

        private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
        {
            Dictionary<string, PlayerDataObject> playerData = new();

            foreach (var (key, value) in data)
            {
                playerData.Add(
                    key,
                    new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member, value: value)
                );
            }

            return playerData;
        }

        public void OnApplicationQuit()
        {
            if (_hostLobby != null && _hostLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.DeleteLobbyAsync(_hostLobby.Id);
            }
        }

        private IEnumerator HearthbeatLobbyCorroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return new WaitForSecondsRealtime(waitTimeSeconds);
            }
        }

        private IEnumerator RefreshLobbyCorroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
                Task<Lobby> task = LobbyService.Instance.GetLobbyAsync(lobbyId);
                yield return new WaitUntil(() => task.IsCompleted);

                Lobby newLobby = task.Result;
                if (newLobby.LastUpdated > _hostLobby.LastUpdated)
                {
                    _hostLobby = newLobby;
                    LobbyEvents.OnLobbyUpdated?.Invoke(newLobby);
                }

                yield return new WaitForSecondsRealtime(waitTimeSeconds);
            }
        }

        public async Task JoinPrivateLobby(string lobbyCode)
        {
            try
            {
                Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

                _joinLobby = lobby;

                Debug.Log("Entrou na sala privada!");

                //ListPlayersOfLobby(_joinedLobby);
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
            }
        }

        public async Task<bool> JoinPublicLobby(string lobbyCode, Dictionary<string, string> playerData)
        {
            try
            {
                JoinLobbyByCodeOptions options = new();

                Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));

                options.Player = player;

                Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);

                _hostLobby = lobby;

                _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCorroutine(_hostLobby.Id, 1f));

                Debug.Log("Entrou na sala pública! - " + _hostLobby.LobbyCode);

                //ListPlayersOfLobby(_joinedLobby);
                return true;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
                return false;
            }
        }

        public async Task<string> CreateGame()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);

                string gameCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                Debug.Log("Criou jogo! Código do jogo: " + gameCode);

                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartHost();

                return gameCode;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
                return null;
            }
        }

        public async Task<bool> PlayGame(string relayCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                Debug.Log("Entrou no jogo! Código do jogo: " + relayCode);
                return true;
            }
            catch (LobbyServiceException exception)
            {
                MultiplayerMenuController.ShowError(exception.Message);
                return false;
            }
        }

        public async Task ListPublicLobbies()
        {
            try
            {
                // filtros para a procura e ordem das lobbies
                QueryLobbiesOptions options = new QueryLobbiesOptions();
                options.Filters = new List<QueryFilter>()
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                };
                options.Order = new List<QueryOrder>()
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                };

                QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();

                Debug.Log("Salas encontradas: " + response.Results.Count);
                foreach (Lobby lobby in response.Results)
                {
                    Debug.Log("Sala: " + lobby.Name + " - " + lobby.MaxPlayers + " - " + lobby.LobbyCode);
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

                _joinLobby = _hostLobby;

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
            if (_joinLobby != null)
            {
                _lobbyUpdateTimer -= Time.deltaTime;

                if (_lobbyUpdateTimer < 0f)
                {
                    float lobbyUpdateTimerMax = 1.1f;
                    _lobbyUpdateTimer = lobbyUpdateTimerMax;

                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinLobby.Id);
                    _joinLobby = lobby;
                }
            }
        }

        private async void UpdatePlayerName(string newPlayerName)
        {
            _playerName = newPlayerName;

            try
            {
                await LobbyService.Instance.UpdatePlayerAsync(_joinLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
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

        private async void LeaveLobby()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void KickPlayer()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinLobby.Id, _joinLobby.Players[1].Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}