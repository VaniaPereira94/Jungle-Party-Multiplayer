using Multiplayer;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Controla o nível 1.
/// O nível consiste em uma partida de futebol com várias rondas.
/// </summary>
public class Level1Controller : MonoBehaviour, ILevelController
{
    /* ATRIBUTOS PRIVADOS */

    // variável para a referência do controlador de jogo
    private GameController _gameController;

    // variáveis sobre os jogadores
    private List<LevelPlayerModel> _levelPlayers = new();

    // variáveis sobre os prefabs específicos dos jogadores
    [SerializeField] private GameObject _player1Level1Prefab;
    [SerializeField] private GameObject _player2Level1Prefab;

    // para os objetos do nível - bola
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private GameObject _ballObject;
    private BallController _ballController;

    // para para o som de marcar golo
    private AudioSource _audioSource;

    // para os objetos do nível - balizas
    [SerializeField] private GameObject _goal1Prefab;
    [SerializeField] private GameObject _goal2Prefab;
    [SerializeField] private GameObject _goal1Object;
    [SerializeField] private GameObject _goal2Object;
    private GoalController _goal1Controller;
    private GoalController _goal2Controller;

    // para os objetos do nível - power ups
    private readonly List<GameObject> _powerUps = new();
    [SerializeField] private GameObject _powerUp;

    // para definir a ação dos jogadores neste nível
    private KickAction _kickAction;

    // referência do controlador do relógio
    private TimerController _timerController;

    // referência do controlador das rondas
    [SerializeField] private RoundController _roundController;

    // referência do controlador da pontuação
    [SerializeField] private ScoreController _scoreController;

    // para os componentes da UI - painel de introdução, botão de pause e painel do fim de nível
    [SerializeField] private GameObject _introPanel;
    [SerializeField] private GameObject _buttonPause;
    [SerializeField] private GameObject _finishedLevelPanel;
    [SerializeField] private GameObject _finishedLevelDescription;


    /* MÉTODOS DO MONOBEHAVIOUR */

    private void Start()
    {
        _gameController = GameController.Instance;

        // TEST: usar isto enquanto é testado apenas o nível atual (sem iniciar pelo menu)
        _gameController.GamePlayers = new();
        _gameController.InitiateGame();

        // armazenar dados de cada jogador neste nível,
        // sabendo que um jogo tem vários níveis e já existem dados que passam de nível para nível, como a pontuação
        CreatePlayersDataForLevel();

        _timerController = TimerController.Instance;
        TimerController.Freeze();

        _roundController.DisplayCurrentRound();
        _roundController.DisplayMaxRounds();

        DisplayObjectInScene();

        _ballController = _ballObject.GetComponent<BallController>();
        _goal1Controller = _goal1Object.GetComponent<GoalController>();
        _goal2Controller = _goal2Object.GetComponent<GoalController>();

        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // quando está no intervalo entre rondas, ou seja o tempo está parado
        if (_timerController.IsOnPause())
        {
            return;
        }

        // se o tempo acabou - congelar objetos, cancelar spawn de power ups, atribuir pontos e iniciar nova ronda
        if (_timerController.HasFinished())
        {
            _timerController.Pause();

            // congela bola e balizas
            _ballController.Freeze();
            _goal1Controller.Freeze();
            _goal2Controller.Freeze();

            CancelInvoke(nameof(SpawnPowerUp));

            // se estiver na última ronda - mostrar o painel do fim de nível
            if (_roundController.IsLastRound())
            {
                // congela para sempre
                FreezePlayers(-1);

                string finishedLevelText = "";
                foreach (LevelPlayerModel levelPlayer in _levelPlayers)
                {
                    finishedLevelText += "Jogador " + levelPlayer.ID + ": " + levelPlayer.LevelScore + "\n";
                }

                _finishedLevelPanel.SetActive(true);
                _finishedLevelDescription.GetComponent<Text>().text = finishedLevelText;

                _buttonPause.SetActive(false);
            }
            // senão iniciar outra ronda
            else
            {
                float freezingTime = 5f;
                FreezePlayers(freezingTime);

                _roundController.NextRound();
                _roundController.DisplayNextRoundIntro();
                _roundController.DisplayCurrentRound();

                Invoke(nameof(RestartRound), freezingTime);
            }

            return;
        }

        // se alguém marcou golo - congelar objetos, cancelar spawn de power ups, atribuir pontos e iniciar nova ronda
        if (_ballController.IsGoalScored())
        {
            _timerController.Pause();

            PlayGoalSound();

            // congela bola e balizas
            _ballController.Freeze();
            _goal1Controller.Freeze();
            _goal2Controller.Freeze();

            CancelInvoke(nameof(SpawnPowerUp));

            LevelPlayerModel scorer = GetScorer();
            UpdateScore(scorer.ID);

            // se estiver na última ronda - mostrar o painel do fim de nível
            if (_roundController.IsLastRound())
            {
                // congela para sempre
                FreezePlayers(-1);

                string finishedLevelText = "";
                foreach (LevelPlayerModel levelPlayer in _levelPlayers)
                {
                    finishedLevelText += "Jogador " + levelPlayer.ID + ": " + levelPlayer.LevelScore + "\n";
                }

                _finishedLevelPanel.SetActive(true);
                _finishedLevelDescription.GetComponent<Text>().text = finishedLevelText;

                _buttonPause.SetActive(false);
            }
            // senão iniciar outra ronda
            else
            {
                float freezingTime = 5f;
                FreezePlayers(freezingTime);

                _roundController.NextRound();
                _roundController.DisplayNextRoundIntro();
                _roundController.DisplayCurrentRound();

                Invoke(nameof(RestartRound), freezingTime);
            }
        }
    }


    /* MÉTODOS DO LEVEL1CONTROLLER */

    /// <summary>
    /// É executado ao clicar no botão de iniciar, no painel de introdução do nível.
    /// Permite que os jogadores comecem de facto a jogar.
    /// </summary>
    public void InitAfterIntro()
    {
        TimerController.Unfreeze();

        _roundController.NextRound();
        _roundController.DisplayCurrentRound();

        _buttonPause.SetActive(true);
        Destroy(_introPanel);

        InvokeRepeating(nameof(SpawnPowerUp), 10f, 10f);
    }

    void SpawnPowerUp()
    {
        System.Random rnd = new();
        int xValue = rnd.Next(42, 58);
        int zValue = rnd.Next(71, 84);

        Instantiate(_powerUp, new Vector3(xValue, _powerUp.transform.position.y, zValue), Quaternion.identity);
    }

    void CreatePlayersDataForLevel()
    {
        LevelPlayerModel levelPlayer1 = new(_gameController.GamePlayers[0].ID, 0, _player1Level1Prefab.transform.position, _player1Level1Prefab.transform.rotation);
        LevelPlayerModel levelPlayer2 = new(_gameController.GamePlayers[1].ID, 0, _player2Level1Prefab.transform.position, _player2Level1Prefab.transform.rotation);

        _levelPlayers.Add(levelPlayer1);
        _levelPlayers.Add(levelPlayer2);
    }

    void DisplayObjectInScene()
    {
        SpawnPlayers();
        AddActionToPlayers();
    }

    void SpawnPlayers()
    {
        _levelPlayers[0].Object = Instantiate(_player1Level1Prefab);
        _levelPlayers[1].Object = Instantiate(_player2Level1Prefab);
    }

    /// <summary>
    /// Adiciona o script da ação a cada um dos objetos dos jogadores, para definir essa ação ao personagem.
    /// </summary>
    void AddActionToPlayers()
    {
        _kickAction = _levelPlayers[0].Object.AddComponent<KickAction>();
        _levelPlayers[0].Object.GetComponent<PlayerController>().SetAction(_kickAction, this);

        _kickAction = _levelPlayers[1].Object.AddComponent<KickAction>();
        _levelPlayers[1].Object.GetComponent<PlayerController>().SetAction(_kickAction, this);
    }

    LevelPlayerModel GetScorer()
    {
        if (_ballController.Player1Scored)
        {
            return _levelPlayers[0];
        }
        else if (_ballController.Player2Scored)
        {
            return _levelPlayers[1];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Atribui os pontos do marcador e atualiza no ecrã.
    /// </summary>
    void UpdateScore(int scorerID)
    {
        _levelPlayers[scorerID - 1].LevelScore += _scoreController.AddScore();
        _scoreController.DisplayScoreObjectText(scorerID, _levelPlayers[scorerID - 1].LevelScore);
    }

    void FreezePlayers(float freezingTime)
    {
        _levelPlayers[0].Object.GetComponent<PlayerController>().Freeze(freezingTime);
        _levelPlayers[1].Object.GetComponent<PlayerController>().Freeze(freezingTime);
    }

    /// <summary>
    /// É executado após o intervalo de espera para iniciar outra ronda.
    /// Responsável por inicializar novamente os componentes necessários para que a ronda comece.
    /// </summary>
    void RestartRound()
    {
        _timerController.Play();

        _timerController.SetInitialTime();

        _roundController.DisableNextRoundIntro();

        // redefinir o jogador que marcou como falso,
        // para que quando inicia uma nova rodada, ainda nenhum jogador marcou
        _ballController.Player1Scored = false;
        _ballController.Player2Scored = false;

        _ballController.Unfreeze();
        _goal1Controller.Unfreeze();
        _goal2Controller.Unfreeze();

        SetInitialPosition();

        DestroyAllPowerUps();

        InvokeRepeating(nameof(SpawnPowerUp), 10f, 10f);
    }

    void SetInitialPosition()
    {
        _levelPlayers[0].Object.transform.position = _levelPlayers[0].InitialPosition;
        _levelPlayers[0].Object.transform.rotation = _levelPlayers[0].InitialRotation;

        _levelPlayers[1].Object.transform.position = _levelPlayers[1].InitialPosition;
        _levelPlayers[1].Object.transform.rotation = _levelPlayers[1].InitialRotation;

        _ballObject.transform.position = _ballPrefab.transform.position;
        _ballObject.transform.rotation = _ballPrefab.transform.rotation;

        _goal1Object.transform.position = _goal1Prefab.transform.position;
        _goal1Object.transform.rotation = _goal1Prefab.transform.rotation;

        _goal2Object.transform.position = _goal2Prefab.transform.position;
        _goal2Object.transform.rotation = _goal2Prefab.transform.rotation;
    }

    void DestroyAllPowerUps()
    {
        GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag("PowerUp");
        foreach (GameObject obj in objectsToDestroy)
        {
            Destroy(obj);
        }
    }

    public void PlayGoalSound()
    {
        _audioSource.Play();
    }

    /// <summary>
    /// É executado quando é clicado o botão de próximo nível, no painel de fim de nível.
    /// </summary>
    public void FinishLevel()
    {
        _gameController.NextLevel(_levelPlayers[0].LevelScore, _levelPlayers[1].LevelScore);
    }
}