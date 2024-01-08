using UnityEngine;
using TMPro;


public class LobbyPlayer : MonoBehaviour
{
    //[SerializeField] private TextMeshPro _playerName;
    private LobbyPlayerData _lobbyPlayerData;

    public void SetMenuData(LobbyPlayerData lobbyPlayerData)
    {
        _lobbyPlayerData = lobbyPlayerData;
        //_playerName.text = data.GamerTag;
        gameObject.SetActive(true);

        if (lobbyPlayerData.IsReady)
        {
            // começar jogo
        }

        GameObject playerNameTextObject = this.gameObject.transform.Find("PlayerNameText").gameObject;
        playerNameTextObject.GetComponent<TextMeshProUGUI>().text = lobbyPlayerData.GamerTag;
    }

    public void SetGameData(LobbyPlayerData lobbyPlayerData)
    {
        _lobbyPlayerData = lobbyPlayerData;
        gameObject.SetActive(true);
    }
}