using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


/// <summary>
/// Controla o nível 4.
/// O nível consiste em trocar de bomba de jogador para jogador,
/// até que o tempo acabe e quem tem a bomba perde.
/// O nível é constituido por várias rondas.
/// </summary>
public class Level4Controller : MonoBehaviour, ILevelController
{
    /* ATRIBUTOS PRIVADOS */

    // variável para a referência do controlador de jogo
    private GameController _gameController;

    // variáveis sobre os jogadores
    private List<LevelPlayerModel> _levelPlayers = new();
    private int _playerIDWithBomb = -1;

    // para saber se os jogadores colidiram
    private bool _collisionOccurred = false;

    // para os objetos do nível - bomba
    [SerializeField] private GameObject _bombPrefab;
    private GameObject _bombObject;
    private BombController _bombController;

    // para os objetos do nível - power ups
    private readonly List<GameObject> _powerUps = new();
    [SerializeField] private GameObject _powerUp;

    // para definir a ação dos jogadores neste nível
    private ThrowLvl4Action _throwAction;

    // referência do controlador do relógio
    private TimerController _timerController;

    // referência do controlador das rondas
    [SerializeField] private RoundController _roundController;

    // referência do controlador da pontuação
    [SerializeField] private ScoreController _scoreController;

    // para detetar que os objetos estão congelados quando a ronda acaba
    private bool _freezeObjects = false;

    // para para o som da bomba a explodir
    private AudioSource _audioSource;

    // para os componentes da UI - painel de introdução, botão de pause e painel do fim de nível
    [SerializeField] private GameObject _introPanel;
    [SerializeField] private GameObject _buttonPause;
    [SerializeField] private GameObject _finishedLevelPanel;
    [SerializeField] private GameObject _finishedLevelDescription;


    /* PROPRIEDADES PÚBLICAS */

    public bool CollisionOccurred
    {
        get { return _collisionOccurred; }
        set { _collisionOccurred = value; }
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

        int randomID = GenerateFirstPlayerWithBomb();
        _playerIDWithBomb = randomID;

        DisplayObjectInScene();

        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // se o tempo da ronda ainda não acabou
        if (!_timerController.HasFinished())
        {
            return;
        }

        // se a ronda acaba - congelar objetos, cancelar spawn de power ups e atribuir pontos
        if (!_freezeObjects)
        {
            _freezeObjects = true;

            PlayBombSound();

            CancelInvoke(nameof(SpawnPowerUp));

            UpdateWinnerScore();

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


    /* MÉTODOS DO LEVEL4CONTROLLER */

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

    private int GenerateFirstPlayerWithBomb()
    {
        // previne que o Random não fique viciado
        Random.InitState(DateTime.Now.Millisecond);

        return Random.Range(1, 3);
    }

    private void DisplayObjectInScene()
    {
        SpawnPlayers();
        AddActionToPlayers();

        SpawnBomb();
        AssignBomb();
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
        _throwAction = _levelPlayers[0].Object.AddComponent<ThrowLvl4Action>();
        _levelPlayers[0].Object.GetComponent<PlayerController>().SetAction(_throwAction, this);

        _throwAction = _levelPlayers[1].Object.AddComponent<ThrowLvl4Action>();
        _levelPlayers[1].Object.GetComponent<PlayerController>().SetAction(_throwAction, this);
    }

    private void SpawnBomb()
    {
        _bombObject = Instantiate(_bombPrefab, _bombPrefab.transform.position, Quaternion.identity);
    }

    public void AssignBomb()
    {
        _bombController = _bombObject.GetComponent<BombController>();
        _bombController.SetPlayer(this.GetPlayerWithBomb());
        _bombController.SetPlayerAsParent(this.GetPlayerWithBomb());
        _bombController.SetLocalPosition(new Vector3(0.042f, 0.39f, 0.352f));
        _bombController.SetLocalRotation(Quaternion.Euler(270f, 0f, 0f));
        _bombController.SetLocalScale(new Vector3(85f, 85f, 85f));
    }

    private void FreezePlayers(float freezingTime)
    {
        _levelPlayers[0].Object.GetComponent<PlayerController>().Freeze(freezingTime);
        _levelPlayers[1].Object.GetComponent<PlayerController>().Freeze(freezingTime);
    }

    /// <summary>
    /// Atribui os pontos do vencedor e atualiza no ecrã.
    /// </summary>
    private void UpdateWinnerScore()
    {
        int winnerID = GetWinnerID();

        _levelPlayers[winnerID - 1].LevelScore += _scoreController.AddScore();
        _scoreController.DisplayScoreObjectText(winnerID, _levelPlayers[winnerID - 1].LevelScore);
    }

    private int GetWinnerID()
    {
        return _playerIDWithBomb == 1 ? 2 : 1;
    }

    /// <summary>
    /// É executado após o intervalo de espera para iniciar outra ronda.
    /// Responsável por inicializar novamente os componentes necessários para que a ronda comece.
    /// </summary>
    private void RestartRound()
    {
        _freezeObjects = false;

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

    public GameObject GetPlayerWithBomb()
    {
        if (_playerIDWithBomb == 1)
        {
            return _levelPlayers[0].Object;
        }
        else
        {
            return _levelPlayers[1].Object;
        }
    }

    public void ChangePlayerTurn()
    {
        if (_playerIDWithBomb == 1)
        {
            _playerIDWithBomb = 2;
        }
        else
        {
            _playerIDWithBomb = 1;
        }
    }

    public void PlayBombSound()
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