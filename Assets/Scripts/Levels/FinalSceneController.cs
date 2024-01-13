using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// Trata da cena final, quando todos os níveis terminam e exibe quem ganhou.
/// </summary>
public class FinalSceneController : MonoBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    //// variável para a referência do controlador de jogo
    //private GameController _gameController;

    //// referencia ao acao do nivel
    //private SuccessAction _successAction;
    //private FailureAction _failureAction;

    //// variáveis sobre os jogadores
    //private List<LevelPlayerModel> _levelPlayers = new();

    //// referência do controlador da pontuação
    //[SerializeField] private ScoreController _scoreController;

    //// para os componentes da UI - painel de final de jogo
    //[SerializeField] private GameObject _finishedGamePanel;
    //[SerializeField] private GameObject _finishedGameDescription;


    ///* MÉTODOS */

    //private void Start()
    //{
    //    _gameController = GameController.Instance;

    //    // TEST: usar isto enquanto é testado apenas o nível atual (sem iniciar pelo menu)
    //    //_gameController.GamePlayers = new();
    //    //_gameController.InitiateGame();

    //    // armazenar dados de cada jogador neste nível,
    //    // sabendo que um jogo tem vários níveis e já existem dados que passam de nível para nível, como a pontuação
    //    CreatePlayersDataForLevel();

    //    DisplayObjectInScene();

    //    string finishedGameText = "";

    //    foreach (LevelPlayerModel levelPlayer in _levelPlayers)
    //    {
    //        finishedGameText += "Jogador " + levelPlayer.ID + ": " + levelPlayer.LevelScore + "\n";
    //    }

    //    _finishedGamePanel.SetActive(true);
    //    _finishedGameDescription.GetComponent<Text>().text = finishedGameText;
    //}

    //private void CreatePlayersDataForLevel()
    //{
    //    foreach (GamePlayerModel gamePlayer in _gameController.GamePlayers)
    //    {
    //        LevelPlayerModel levelPlayer = new(gamePlayer.ID, gamePlayer.GlobalScore, gamePlayer.Prefab.transform.position, gamePlayer.Prefab.transform.rotation);
    //        _levelPlayers.Add(levelPlayer);
    //    }
    //}

    //private void DisplayObjectInScene()
    //{
    //    SpawnPlayers();
    //    AddActionToPlayers();
    //}

    //private void SpawnPlayers()
    //{
    //    _levelPlayers[0].Object = Instantiate(_gameController.GamePlayers[0].Prefab);
    //    _levelPlayers[1].Object = Instantiate(_gameController.GamePlayers[1].Prefab);
    //}

    //private void AddActionToPlayers()
    //{
    //    // se empatarem
    //    if (_gameController.GamePlayers[0].GlobalScore == _gameController.GamePlayers[1].GlobalScore)
    //    {
    //        _failureAction = _levelPlayers[0].Object.AddComponent<FailureAction>();
    //        _levelPlayers[0].Object.GetComponent<PlayerController>().SetAction(_failureAction, this);

    //        _failureAction = _levelPlayers[1].Object.AddComponent<FailureAction>();
    //        _levelPlayers[1].Object.GetComponent<PlayerController>().SetAction(_failureAction, this);
    //    }
    //    // se jogador 1 ganhou
    //    else if (_gameController.GamePlayers[0].GlobalScore > _gameController.GamePlayers[1].GlobalScore)
    //    {
    //        _successAction = _levelPlayers[0].Object.AddComponent<SuccessAction>();
    //        _levelPlayers[0].Object.GetComponent<PlayerController>().SetAction(_successAction, this);

    //        _failureAction = _levelPlayers[1].Object.AddComponent<FailureAction>();
    //        _levelPlayers[1].Object.GetComponent<PlayerController>().SetAction(_failureAction, this);
    //    }
    //    // se jogador 2 ganhou
    //    else
    //    {
    //        _failureAction = _levelPlayers[0].Object.AddComponent<FailureAction>();
    //        _levelPlayers[0].Object.GetComponent<PlayerController>().SetAction(_failureAction, this);

    //        _successAction = _levelPlayers[1].Object.AddComponent<SuccessAction>();
    //        _levelPlayers[1].Object.GetComponent<PlayerController>().SetAction(_successAction, this);
    //    }
    //}

    /// <summary>
    /// É executado quando é clicado o botão de menu, no painel de fim de jogo.
    /// </summary>
    public void Quit()
    {
        TimerController.Unfreeze();

        string sceneName = "MainMenuScene";
        SceneManager.LoadScene(sceneName);
    }
}