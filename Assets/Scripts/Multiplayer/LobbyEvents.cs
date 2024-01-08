using Unity.Services.Lobbies.Models;


public class LobbyEvents
{
    public delegate void LobbyUpdated(Lobby lobby);
    public static LobbyUpdated OnLobbyUpdated;
}

public static class LobbyGameEvents
{
    public delegate void LobbyUpdated();
    public static LobbyUpdated OnLobbyUpdated;

    public delegate void LobbyReady();
    public static LobbyReady OnLobbyReady;

    public delegate void GameInited();
    public static GameInited OnGameInited;
}