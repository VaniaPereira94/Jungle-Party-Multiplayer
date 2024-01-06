using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;


namespace Multiplayer
{
    public class MultiplayerController : SingletonMonoBehaviour<MultiplayerController>
    {
        /* ATRIBUTOS PRIVADOS */

        private const int _MAX_PLAYERS_IN_LOOBY = 2;
        private List<LobbyPlayerData> _lobbyPlayersDatas = new();
        private LobbyPlayerData _localLobbyPlayerData;


        /* PROPRIEDADES PRIVADAS */

        public bool IsHost
        {
            get
            {
                return _localLobbyPlayerData != null && _localLobbyPlayerData.Id == LobbyController.Instance.GetHostId();
            }
        }


        /* MÉTODOS */

        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }

        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }

        public string GetLobbyCode()
        {
            return LobbyController.Instance.GetLobbyCode();
        }

        public string GetGameCode()
        {
            return LobbyController.Instance.GetGameCode();
        }

        public List<LobbyPlayerData> GetPlayers()
        {
            return _lobbyPlayersDatas;
        }

        public async Task<bool> SetPlayerReady()
        {
            _localLobbyPlayerData.IsReady = true;

            bool isSuccess = await LobbyController.Instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize());
            return isSuccess;
        }

        public async Task<bool> CreatePublicLobby()
        {
            string lobbyName = "Lobby";

            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");

            bool isSuccess = await LobbyController.Instance.CreatePublicLobby(lobbyName, _MAX_PLAYERS_IN_LOOBY, _localLobbyPlayerData.Serialize());
            return isSuccess;
        }

        public async Task<string> CreateGame()
        {
            string gameCode = await LobbyController.Instance.CreateGame();
            return gameCode;
        }

        public async Task<bool> JoinPublicLobby(string lobbyCode)
        {
            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "JoinPlayer");

            bool isSuccess = await LobbyController.Instance.JoinPublicLobby(lobbyCode, _localLobbyPlayerData.Serialize());
            return isSuccess;
        }

        public async Task<bool> PlayGame(string gameCode)
        {
            bool isSuccess = await LobbyController.Instance.PlayGame(gameCode);
            return isSuccess;
        }

        private void OnLobbyUpdated(Lobby lobby)
        {
            List<Dictionary<string, PlayerDataObject>> playerData = LobbyController.Instance.GetPlayersData();

            _lobbyPlayersDatas.Clear();

            int numberOfPlayers = 0;

            foreach (Dictionary<string, PlayerDataObject> data in playerData)
            {
                LobbyPlayerData lobbyPlayerData = new();
                lobbyPlayerData.Initialize(data);

                if (lobbyPlayerData.IsReady)
                {
                    numberOfPlayers++;
                }

                if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
                {
                    _localLobbyPlayerData = lobbyPlayerData;
                }

                _lobbyPlayersDatas.Add(lobbyPlayerData);
            }

            LobbyGameEvents.OnLobbyUpdated?.Invoke();

            if (numberOfPlayers == lobby.Players.Count)
            {
                LobbyGameEvents.OnLobbyReady?.Invoke();
            }
        }
    }
}