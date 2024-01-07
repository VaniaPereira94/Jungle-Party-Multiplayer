using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


namespace Multiplayer
{
    public class LobbyController : SingletonMonoBehaviour<LobbyController>
    {
        /* ATRIBUTOS E PROPRIEDADES */

        private Lobby _currentLobby;

        private string _gameCode;

        private Coroutine _hearthbeatLobbyCoroutine;
        private Coroutine _refreshLobbyCoroutine;


        /* MÉTODOS */

        public string GetLobbyCode()
        {
            return _currentLobby?.LobbyCode;
        }

        public string GetGameCode()
        {
            return _gameCode;
        }

        public string GetHostId()
        {
            return _currentLobby.HostId;
        }

        public List<Dictionary<string, PlayerDataObject>> GetPlayersData()
        {
            List<Dictionary<string, PlayerDataObject>> playerData = new();

            foreach (Player player in _currentLobby.Players)
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

                Debug.Log("Sala privada criada! " + lobby.Id + " - " + lobby.Name + " - " + lobby.MaxPlayers + " - " + lobby.LobbyCode);
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
            }
        }

        public async Task<bool> CreatePublicLobby(string lobbyName, int maxPlayers, Dictionary<string, string> playerData, Dictionary<string, string> lobbyData)
        {
            try
            {
                Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));

                CreateLobbyOptions options = new CreateLobbyOptions();
                options.IsPrivate = false;
                options.Player = player;
                options.Data = SerializeLobbyData(lobbyData);

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

                _currentLobby = lobby;

                _hearthbeatLobbyCoroutine = StartCoroutine(HearthbeatLobbyCorroutine(_currentLobby.Id, 6f));
                _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCorroutine(_currentLobby.Id, 1f));

                Debug.Log("Sala pública criada! " + _currentLobby.Id + " - " + _currentLobby.Name + " - " + _currentLobby.MaxPlayers + " - " + _currentLobby.LobbyCode);
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

        private Dictionary<string, DataObject> SerializeLobbyData(Dictionary<string, string> data)
        {
            Dictionary<string, DataObject> lobbyData = new();

            foreach (var (key, value) in data)
            {
                lobbyData.Add(
                    key,
                    new DataObject(visibility: DataObject.VisibilityOptions.Member, value: value)
                );
            }

            return lobbyData;
        }

        public void OnApplicationQuit()
        {
            if (_currentLobby != null && _currentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
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
                if (newLobby.LastUpdated > _currentLobby.LastUpdated)
                {
                    _currentLobby = newLobby;
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
                _currentLobby = lobby;

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
                Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));

                JoinLobbyByCodeOptions options = new();
                options.Player = player;

                Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);

                _currentLobby = lobby;
                _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCorroutine(_currentLobby.Id, 1f));

                Debug.Log("Entrou na sala pública! - " + _currentLobby.LobbyCode);

                //ListPlayersOfLobby(_joinedLobby);
                return true;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
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

        public async Task<bool> UpdatePlayerData(string playerId, Dictionary<string, string> data, string allocationId = default, string connectionData = default)
        {
            try
            {
                Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);

                UpdatePlayerOptions options = new();
                options.Data = playerData;
                options.AllocationId = allocationId;
                options.ConnectionInfo = connectionData;

                _currentLobby = await LobbyService.Instance.UpdatePlayerAsync(_currentLobby.Id, playerId, options);

                LobbyEvents.OnLobbyUpdated(_currentLobby);

                return true;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
                return false;
            }
        }

        public async Task<bool> UpdateLobbyData(Dictionary<string, string> data)
        {
            try
            {
                Dictionary<string, DataObject> lobbyData = SerializeLobbyData(data);

                UpdateLobbyOptions options = new();
                options.Data = lobbyData;

                _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, options);

                LobbyEvents.OnLobbyUpdated(_currentLobby);

                return true;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
                return false;
            }
        }

        private async void KickPlayer()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, _currentLobby.Players[1].Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void LeaveLobby()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}