using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _createLobby;
    [SerializeField] private GameObject _score;
    [SerializeField] private GameObject _scorePublic;
    [SerializeField] private GameObject _scorePrivate;

    // referência para o controlador de jogo
    private GameController _game;

    // Start is called before the first frame update
    void Start()
    {

        _game = GameController.Instance;
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

    public void OpenScore()
    {
        _menu.SetActive(false);
        _score.SetActive(true);
    }

    public void CloseScore()
    {
        _menu.SetActive(true);
        _score.SetActive(false);
    }

    public void OpenScorePublic()
    {
        _score.SetActive(false);
        _scorePublic.SetActive(true);
    }
    public void CloseScorePublic()
    {
        _score.SetActive(true);
        _scorePublic.SetActive(false);
    }
    public void OpenScorePrivate()
    {
        _score.SetActive(false);
        _scorePrivate.SetActive(true);
    }
    public void CloseScorePrivate()
    {
        _score.SetActive(true);
        _scorePrivate.SetActive(false);
    }
}
