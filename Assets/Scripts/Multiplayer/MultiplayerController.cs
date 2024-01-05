using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

namespace Multiplayer
{
    public class MultiplayerController : Singleton<MultiplayerController>
    {
        /* ATRIBUTOS */

        private const int _MAX_PLAYERS_IN_LOOBY = 2;
        private List<LobbyPlayerData> _lobbyPlayersDatas = new();
        private LobbyPlayerData _localLobbyPlayerData;


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

        public List<LobbyPlayerData> GetPlayers()
        {
            return _lobbyPlayersDatas;
        }

        public async Task<bool> CreatePublicLobby()
        {
            string lobbyName = "Lobby";

            LobbyPlayerData playerData = new();
            playerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");

            bool isSuccess = await LobbyController.Instance.CreatePublicLobby(lobbyName, _MAX_PLAYERS_IN_LOOBY, playerData.Serialize());
            return isSuccess;
        }

        public async Task<string> CreateGame()
        {
            string gameCode = await LobbyController.Instance.CreateGame();
            return gameCode;
        }

        public async Task<bool> JoinPublicLobby(string lobbyCode)
        {
            LobbyPlayerData playerData = new();
            playerData.Initialize(AuthenticationService.Instance.PlayerId, "JoinPlayer");

            bool isSuccess = await LobbyController.Instance.JoinPublicLobby(lobbyCode, playerData.Serialize());
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

            foreach (Dictionary<string, PlayerDataObject> data in playerData)
            {
                LobbyPlayerData lobbyPlayerData = new();
                lobbyPlayerData.Initialize(data);

                if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
                {
                    _localLobbyPlayerData = lobbyPlayerData;
                }

                _lobbyPlayersDatas.Add(lobbyPlayerData);
            }

            LobbyGameEvents.OnLobbyUpdated?.Invoke();
        }
    }
}