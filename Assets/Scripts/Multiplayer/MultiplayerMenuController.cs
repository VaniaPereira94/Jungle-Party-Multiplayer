using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LobbyManager;

/// <summary>
/// Trata das interações do utilizador com o menu de multiplayer.
/// </summary>
public class MultiplayerMenuController : NetworkBehaviour
{
    /* ATRIBUTOS */

    // referências para objetos de UI
    [Header("UI")]
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _createLobby;
    [SerializeField] private GameObject _joinLobby;
    [SerializeField] private TMP_InputField _privateLobbyCode;
    [SerializeField] private GameObject _gameLobby;
    [SerializeField] private GameObject _leaderboard;
    [SerializeField] private GameObject _scorePublic;
    [SerializeField] private GameObject _scorePrivate;
    [SerializeField] private GameObject _authentication;

    // referências para outros controladores
    [Header("Controllers")]
    [SerializeField] private AuthController _authController;
    [SerializeField] private LobbyController _lobbyController;
    //[SerializeField] private GameController _gameController;

    //private NetworkVariable<FixedString32Bytes> relayCode = new();
    //public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    //public class OnLobbyListChangedEventArgs : EventArgs
    //{
    //    public string RelayCode { get; set; }
    //}

    [SerializeField] private TMP_InputField _gameCode;


    /* MÉTODOS */

    private void Start()
    {
        _authController = AuthController.Instance;
        _lobbyController = LobbyController.Instance;
        //_gameController = GameController.Instance;

        //OnLobbyListChanged += LobbyManager_OnLobbyListChanged;

        InitializeMultiplayer();
    }

    //private void LobbyManager_OnLobbyListChanged(object sender, OnLobbyListChangedEventArgs e)
    //{
    //    relayCode = e.RelayCode;
    //    Debug.Log($"RelayCode alterado: {e.RelayCode}");
    //}

    //public override void OnNetworkSpawn()
    //{
    //    relayCode.OnValueChanged += OnRelayCodeChanged;
    //}

    private void OnRelayCodeChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        Debug.Log($"Detected NetworkVariable Change: Previous: {previous} | Current: {current}");
    }

    //[ClientRpc]
    //public void ToggleClientRpc(FixedString32Bytes newRelayCode)
    //{
    //    relayCode.Value = newRelayCode;

    //    SyncTestClientRpc(relayCode.Value.ToString());
    //}

    private async void InitializeMultiplayer()
    {
        await _authController.Connect();
        await _authController.AuthenticateAnonymous();
    }

    public void OpenCreateLobby()
    {
        _menu.SetActive(false);
        _createLobby.SetActive(true);
    }

    public void CloseCreateLobby()
    {
        _menu.SetActive(true);
        _createLobby.SetActive(false);
    }

    public async void CreatePrivateLobby()
    {
        string lobbyName = "abcde";
        await _lobbyController.CreatePrivateLobby(lobbyName);
        OpenGameLobby();
    }

    public async void CreatePublicLobby()
    {
        string lobbyName = "abcde";
        await _lobbyController.CreatePublicLobby(lobbyName);

        string a = await _lobbyController.CreateGame();
        _gameCode.text = a;
        //FixedString32Bytes b = new FixedString32Bytes(a);

        //relayCode.Value = a;
        //OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { RelayCode = code });
        //CmdSyncTestClientRpc(relayCode.Value);
        //ToggleClientRpc(b);

        OpenGameLobby();
    }

    // Comando de sincronização chamado no servidor
    //[ClientRpc]
    //void CmdSyncTestClientRpc(string value)
    //{
    //    // Modifica o valor sincronizado no servidor
    //    relayCode.Value = value;

    //    // Exibe o valor no servidor
    //    Debug.Log("Valor no servidor: " + relayCode.Value);

    //    // Informa aos clientes sobre a mudança
    //    SyncTestClientRpc(value);
    //}

    // RPC (Remote Procedure Call) chamado em todos os clientes
    //[ClientRpc]
    //void SyncTestClientRpc(string value)
    //{
    //    // Modifica o valor sincronizado em cada cliente
    //    relayCode.Value = value;

    //    // Exibe o valor em cada cliente
    //    Debug.Log("Valor em cliente remoto: " + relayCode);
    //}

    public async void OpenJoinLobby()
    {
        _menu.SetActive(false);
        _joinLobby.SetActive(true);

        //await _lobbyController.ListPublicLobbies();
    }

    public void CloseJoinLobby()
    {
        _menu.SetActive(true);
        _joinLobby.SetActive(false);
    }

    public async void JoinPrivateLobby()
    {
        string lobbyCode = _privateLobbyCode.text;
        await _lobbyController.JoinPrivateLobby(lobbyCode);
    }

    public async void JoinPublicLobby()
    {
        string lobbyCode = "a1b2c3";
        await _lobbyController.JoinPublicLobby(lobbyCode);
    }

    public void OpenLeaderboard()
    {
        _menu.SetActive(false);
        _leaderboard.SetActive(true);
    }

    public async void PlayGame()
    {
        if (await _lobbyController.PlayGame(_gameCode.text))
        {
            OpenGameLobby();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync("Level2MultiplayerController");
    }

    public void OpenGameLobby()
    {
        _gameLobby.SetActive(true);
    }

    public void CloseLeaderboard()
    {
        _menu.SetActive(true);
        _leaderboard.SetActive(false);
    }

    public void OpenScorePrivate()
    {
        _leaderboard.SetActive(false);
        _scorePrivate.SetActive(true);
    }

    public void CloseScorePrivate()
    {
        _leaderboard.SetActive(true);
        _scorePrivate.SetActive(false);
    }

    public void OpenScorePublic()
    {
        _leaderboard.SetActive(false);
        _scorePublic.SetActive(true);
    }

    public void CloseScorePublic()
    {
        _leaderboard.SetActive(true);
        _scorePublic.SetActive(false);
    }

    public void OpenAuthentication()
    {
        _joinLobby.SetActive(false);
        _authentication.SetActive(true);
    }

    public void CloseAuthentication()
    {
        _joinLobby.SetActive(true);
        _authentication.SetActive(false);
    }

    public static void ShowError(string message)
    {
        Debug.LogError(message);
    }
}