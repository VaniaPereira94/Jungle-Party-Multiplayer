using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class LobbyController : SingletonMonoBehaviour<LobbyController>
{
    /* ATRIBUTOS E PROPRIEDADES */

    private Lobby _currentLobby;

    private Coroutine _hearthbeatLobbyCoroutine;
    private Coroutine _refreshLobbyCoroutine;

    public event EventHandler OnLeftLobby;


    /* MÉTODOS */

    public string GetLobbyCode()
    {
        return _currentLobby?.LobbyCode;
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

    public async Task<bool> CreatePublicLobby(string lobbyName, int maxPlayers, Dictionary<string, string> playerData, Dictionary<string, string> lobbyData)
    {
        try
        {
            Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));

            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                Data = SerializeLobbyData(lobbyData),
                IsPrivate = false,
                Player = player

            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

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

    public async void OnApplicationQuit()
    {
        if (_currentLobby != null && _currentLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                // Limpe os dados do lobby local
                _currentLobby = null;
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception.Message);
            }
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

            return true;
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogError(exception.Message);
            return false;
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

            UpdateLobbyOptions options = new()
            {
                Data = lobbyData
            };

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

    public async void KickPlayer()
    {
        try
        {
            if (_currentLobby.Players.Count > 1)
            {
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, _currentLobby.Players[1].Id);
                _currentLobby.Players.RemoveAt(1);
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId);
            _currentLobby.Players.RemoveAt(0);
            LobbyEvents.OnLobbyUpdated(_currentLobby);
            _currentLobby = null;

            OnLeftLobby?.Invoke(this, EventArgs.Empty);
            StopAllCoroutines();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }
}