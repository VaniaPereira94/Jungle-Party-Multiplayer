using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Controla o nível 3.
/// O nível consiste em empurrar o adversário para fora do ringue.
/// O nível é constituido por várias rondas.
/// </summary>
public class Level3Controller : MonoBehaviour, ILevelController
{
    /* ATRIBUTOS PRIVADOS */

    // variável para a referência do controlador de jogo
    private GameController _gameController;

    // variáveis sobre os jogadores
    private List<LevelPlayerModel> _levelPlayers = new();

    // para os objetos do nível - power ups
    private readonly List<GameObject> _powerUps = new();
    [SerializeField] private GameObject _powerUp;

    // para definir a ação dos jogadores neste nível
    private CarryAction _carryAction;

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

    // para saber se algum jogador saiu da arena e quem foi
    private bool _outOfArena = false;
    private int _playerOutID = -1;


    /* PROPRIEDADES PÚBLICAS */

    public bool OutOfArena
    {
        get { return _outOfArena; }
        set { _outOfArena = value; }
    }

    public int PlayerOutID
    {
        get { return _playerOutID; }
        set { _playerOutID = value; }
    }


    /* MÉTODOS DO MONOBEHAVIOUR */

    private void Start()
    {
        _gameController = GameController.Instance;

        // TEST: usar isto enquanto é testado apenas o nível atual (sem iniciar pelo menu)
        //_gameController.GamePlayers = new();
        //_gameController.InitiateGame();

        // armazenar dados de cada jogador neste nível,
        // sabendo que um jogo tem vários níveis e já existem dados que passam de nível para nível, como a pontuação
        CreatePlayersDataForLevel();

        _timerController = TimerController.Instance;
        TimerController.Freeze();

        _roundController.DisplayCurrentRound();
        _roundController.DisplayMaxRounds();

        DisplayObjectInScene();
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

        // se alguém saiu da arena - congelar objetos, cancelar spawn de power ups, atribuir pontos e iniciar nova ronda
        if (IsOutOfArena())
        {
            _timerController.Pause();

            CancelInvoke(nameof(SpawnPowerUp));

            LevelPlayerModel winner = GetWinner();
            UpdateScore(winner.ID);

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


    /* MÉTODOS DO LEVEL3CONTROLLER */

    /// <summary>
    /// É executado ao clicar no botão de iniciar, no painel de introdução do nível.
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

    private void SpawnPowerUp()
    {
        System.Random rnd = new();
        int xValue = rnd.Next(42, 58);
        int zValue = rnd.Next(71, 84);

        Instantiate(_powerUp, new Vector3(xValue, _powerUp.transform.position.y, zValue), Quaternion.identity);
    }

    private void CreatePlayersDataForLevel()
    {
        foreach (GamePlayerModel gamePlayer in _gameController.GamePlayers)
        {
            LevelPlayerModel levelPlayer = new(gamePlayer.ID, 0, gamePlayer.Prefab.transform.position, gamePlayer.Prefab.transform.rotation);
            _levelPlayers.Add(levelPlayer);
        }
    }

    private void DisplayObjectInScene()
    {
        SpawnPlayers();
        AddActionToPlayers();
    }

    private void SpawnPlayers()
    {
        _levelPlayers[0].Object = Instantiate(_gameController.GamePlayers[0].Prefab);
        _levelPlayers[1].Object = Instantiate(_gameController.GamePlayers[1].Prefab);
    }

    /// <summary>
    /// Adiciona o script da ação a cada um dos objetos dos jogadores, para definir essa ação ao personagem.
    /// </summary>
    private void AddActionToPlayers()
    {
        _carryAction = _levelPlayers[0].Object.AddComponent<CarryAction>();
        _levelPlayers[0].Object.GetComponent<PlayerController>().SetAction(_carryAction, this);

        _carryAction = _levelPlayers[1].Object.AddComponent<CarryAction>();
        _levelPlayers[1].Object.GetComponent<PlayerController>().SetAction(_carryAction, this);
    }

    public bool IsOutOfArena()
    {
        return _outOfArena;
    }

    private LevelPlayerModel GetWinner()
    {
        if (_playerOutID == 1)
        {
            return _levelPlayers[1];
        }
        else if (_playerOutID == 2)
        {
            return _levelPlayers[0];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Atribui os pontos do marcador e atualiza no ecrã.
    /// </summary>
    private void UpdateScore(int scorerID)
    {
        _levelPlayers[scorerID - 1].LevelScore += _scoreController.AddScore();
        _scoreController.DisplayScoreObjectText(scorerID, _levelPlayers[scorerID - 1].LevelScore);
    }

    private void FreezePlayers(float freezingTime)
    {
        _levelPlayers[0].Object.GetComponent<PlayerController>().Freeze(freezingTime);
        _levelPlayers[1].Object.GetComponent<PlayerController>().Freeze(freezingTime);
    }

    /// <summary>
    /// É executado após o intervalo de espera para iniciar outra ronda.
    /// Responsável por inicializar novamente os componentes necessários para que a ronda comece.
    /// </summary>
    private void RestartRound()
    {
        _outOfArena = false;

        _timerController.Play();

        _timerController.SetInitialTime();

        _roundController.DisableNextRoundIntro();

        SetInitialPosition();

        DestroyAllPowerUps();

        InvokeRepeating(nameof(SpawnPowerUp), 10f, 10f);
    }

    private void SetInitialPosition()
    {
        _levelPlayers[0].Object.transform.position = _levelPlayers[0].InitialPosition;
        _levelPlayers[0].Object.transform.rotation = _levelPlayers[0].InitialRotation;

        _levelPlayers[1].Object.transform.position = _levelPlayers[1].InitialPosition;
        _levelPlayers[1].Object.transform.rotation = _levelPlayers[1].InitialRotation;
    }

    private void DestroyAllPowerUps()
    {
        GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag("PowerUp");
        foreach (GameObject obj in objectsToDestroy)
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// É executado quando é clicado o botão de próximo nível, no painel de fim de nível.
    /// </summary>
    public void FinishLevel()
    {
        _gameController.NextLevel(_levelPlayers[0].LevelScore, _levelPlayers[1].LevelScore);
    }
}