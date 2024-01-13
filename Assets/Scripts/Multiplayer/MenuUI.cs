using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public static MenuUI Instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetLobbyListUI(List<Lobby> lobbies)
    {
        GameObject lobbyList = GameObject.Find("LobbyList");

        GameObject lobbyContainer = GameObject.Find("LobbyContainer");



        for (int i = 0; i < lobbies.Count; i++)
        {
            Lobby lobby = lobbies[i];

            GameObject g = Instantiate(lobbyContainer, lobbyList.transform);
            GameObject lobbyNameText = g.transform.Find("LobbyNameText").gameObject;
            GameObject playersText = g.transform.Find("PlayersText").gameObject;
            GameObject codeText = g.transform.Find("CodeText").gameObject;

            lobbyNameText.GetComponent<TextMeshProUGUI>().text = lobby.Id;
            playersText.GetComponent<TextMeshProUGUI>().text = lobby.Players.Count + "/" + lobby.MaxPlayers;
            codeText.GetComponent<TextMeshProUGUI>().text = lobby.LobbyCode;
        }

        Destroy(lobbyContainer);
    }

    public void SetLobbyDashboard(Lobby lobby)
    {
        GameObject playersList = GameObject.Find("PlayersList");
        GameObject playerContainer = GameObject.Find("PlayerContainer");

        foreach (Player player in lobby.Players)
        {
            GameObject g = Instantiate(playerContainer, playersList.transform);
            GameObject lobbyNameText = g.transform.Find("PlayerNameText").gameObject;
            GameObject playersText = g.transform.Find("PlayerPosText").gameObject;
            GameObject codeText = g.transform.Find("PointText").gameObject;

            lobbyNameText.GetComponent<TextMeshProUGUI>().text = player.Id;
            playersText.GetComponent<TextMeshProUGUI>().text = "1";
            codeText.GetComponent<TextMeshProUGUI>().text = "12";
        }

        Destroy(playerContainer);
    }
}

