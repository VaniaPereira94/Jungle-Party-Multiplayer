using Unity.Services.Lobbies.Models;

namespace Multiplayer
{
    public class LobbyEvents
    {
        public delegate void LobbyUpdated(Lobby lobby);
        public static LobbyUpdated OnLobbyUpdated;
    }

    public static class LobbyGameEvents
    {
        public delegate void LobbyUpdated();
        public static LobbyUpdated OnLobbyUpdated;
    }
}