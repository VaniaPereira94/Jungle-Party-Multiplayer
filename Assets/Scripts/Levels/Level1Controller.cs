using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// Controla o nível 1.
/// O nível consiste em uma partida de futebol com várias rondas.
/// </summary>
public class Level1Controller : NetworkBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    // para guardar uma instância única desta classe
    private static Level1Controller _instance;

    // variável para a referência do controlador de jogo
    //private GameController _gameController;

    // variáveis sobre os jogadores
    private List<LevelPlayerModel> _levelPlayers = new();
    //private NetworkVariable<List<LevelPlayerModel>> _levelPlayers = new NetworkVariable<List<LevelPlayerModel>>();
    //[SerializeField] private List<LobbyPlayer> _players;

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

    // para os componentes da UI - botão de pause e painel do fim de nível
    [SerializeField] private GameObject _buttonPause;
    [SerializeField] private GameObject _finishedLevelPanel;
    [SerializeField] private GameObject _finishedLevelDescription;

    public static Level1Controller Instance
    {
        get { return _instance; }
        set { _instance = value; }
    }


    /* MÉTODOS DO MONOBEHAVIOUR */

    //private void OnEnable()
    //{
    //    LobbyGameEvents.OnLobbyUpdated += OnLobbyUpdated;
    //}

    //private void OnDisable()
    //{
    //    LobbyGameEvents.OnLobbyUpdated -= OnLobbyUpdated;
    //}

    /// <summary>
    /// É executado antes da função Start().
    /// </summary>
    void Awake()
    {
        if (_instance != null)
        {
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        // armazenar dados de cada jogador neste nível,
        // sabendo que um jogo tem vários níveis e já existem dados que passam de nível para nível, como a pontuação
        //CreatePlayersDataForLevel();

        _timerController = TimerController.Instance;
        //TimerController.Freeze();

        _roundController.DisplayCurrentRound();
        _roundController.DisplayMaxRounds();

        //DisplayObjectInScene();
        Debug.Log("_player1Level1Prefab.transform.position" + _player1Level1Prefab.transform.position);
        Debug.Log("_player2Level1Prefab.transform.position" + _player2Level1Prefab.transform.position);
    
        _ballController = _ballObject.GetComponent<BallController>();
        _goal1Controller = _goal1Object.GetComponent<GoalController>();
        _goal2Controller = _goal2Object.GetComponent<GoalController>();

        _audioSource = GetComponent<AudioSource>();

        InitAfterPlayersReady();
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

            string scorerTag = GetScorer();
            UpdateScore(scorerTag);

            // se estiver na última ronda - mostrar o painel do fim de nível
            if (_roundController.IsLastRound())
            {
                // congela para sempre
                FreezePlayers(-1);

                //string finishedLevelText = "";
                //foreach (LevelPlayerModel levelPlayer in _levelPlayers)
                //{
                //    finishedLevelText += "Jogador " + levelPlayer.ID + ": " + levelPlayer.LevelScore + "\n";
                //}
                int player1ID = GetPlayer1InScene().GetComponent<PlayerController>().PlayerID;
                int player1Score = GetPlayer1InScene().GetComponent<PlayerController>().Score;
                string finishedLevelText = "Jogador " + player1ID.ToString() + ": " + player1Score.ToString() + "\n";

                int player2ID = GetPlayer1InScene().GetComponent<PlayerController>().PlayerID;
                int player2Score = GetPlayer1InScene().GetComponent<PlayerController>().Score;
                finishedLevelText += "Jogador " + player2ID.ToString() + ": " + player2Score.ToString() + "\n";

                _finishedLevelPanel.SetActive(true);
                _finishedLevelDescription.GetComponent<Text>().text = finishedLevelText;

                _buttonPause.SetActive(false);
            }
            // senão - iniciar outra ronda
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

    private void OnLobbyUpdated()
    {
        //List<LobbyPlayerData> playerDatas = MultiplayerController.Instance.GetPlayers();

        //for (int i = 0; i < playerDatas.Count; i++)
        //{
        //    LobbyPlayerData data = playerDatas[i];
        //    _players[i].SetGameData(data);
        //}

        //if (playerDatas.Count == 2)
        //{
        //    _player1Level1Prefab = _players[0].gameObject;
        //    _player2Level1Prefab = _players[1].gameObject;

        //    // armazenar dados de cada jogador neste nível,
        //    // sabendo que um jogo tem vários níveis e já existem dados que passam de nível para nível, como a pontuação
        //    CreatePlayersDataForLevel();

        //    _timerController = TimerController.Instance;
        //    TimerController.Freeze();

        //    _roundController.DisplayCurrentRound();
        //    _roundController.DisplayMaxRounds();

        //    //DisplayObjectInScene();

        //    _ballController = _ballObject.GetComponent<BallController>();
        //    _goal1Controller = _goal1Object.GetComponent<GoalController>();
        //    _goal2Controller = _goal2Object.GetComponent<GoalController>();

        //    _audioSource = GetComponent<AudioSource>();

        //    InitAfterPlayersReader();

        //    _levelStarted = true;
        //}
    }

    /// <summary>
    /// É executado ao clicar no botão de iniciar, no painel de introdução do nível.
    /// Permite que os jogadores comecem de facto a jogar.
    /// </summary>
    public void InitAfterPlayersReady()
    {
        //TimerController.Unfreeze();

        _roundController.NextRound();
        _roundController.DisplayCurrentRound();

        _buttonPause.SetActive(true);

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
        LevelPlayerModel levelPlayer1 = new(1, 0, _player1Level1Prefab.transform.position, _player1Level1Prefab.transform.rotation);
        LevelPlayerModel levelPlayer2 = new(2, 0, _player2Level1Prefab.transform.position, _player2Level1Prefab.transform.rotation);
        //LevelPlayerModel levelPlayer1 = new(_gameController.GamePlayers[0].ID, 0, _player1Level1Prefab.transform.position, _player1Level1Prefab.transform.rotation);
        //LevelPlayerModel levelPlayer2 = new(_gameController.GamePlayers[1].ID, 0, _player2Level1Prefab.transform.position, _player2Level1Prefab.transform.rotation);

        _levelPlayers.Add(levelPlayer1);
        _levelPlayers.Add(levelPlayer2);
    }

    public void CreateLevelPlayer(int listIndex)
    {
        LevelPlayerModel levelPlayer = new(listIndex + 1, 0, _player1Level1Prefab.transform.position, _player1Level1Prefab.transform.rotation);
        _levelPlayers.Add(levelPlayer);
    }

    //private void DisplayObjectInScene()
    //{
    //    SpawnPlayers();
    //    AddActionToPlayers();
    //}

    //private void SpawnPlayers()
    //{
    //    _levelPlayers[0].Object = Instantiate(_player1Level1Prefab);
    //    _levelPlayers[1].Object = Instantiate(_player2Level1Prefab);
    //}

    private void SetPlayersObjects()
    {
        _levelPlayers[0].Object = GetPlayer1InScene();
        _levelPlayers[1].Object = GetPlayer2InScene();
    }

    public void SetPlayerObject(GameObject player, int listIndex)
    {
        _levelPlayers[listIndex].Object = player;
    }

    /// <summary>
    /// Adiciona o script da ação a cada um dos objetos dos jogadores, para definir essa ação ao personagem.
    /// </summary>
    public void AddActionToPlayer(int listIndex)
    {
        _kickAction = _levelPlayers[listIndex].Object.AddComponent<KickAction>();
        _levelPlayers[listIndex].Object.GetComponent<PlayerController>().SetAction(_kickAction, this);
    }

    /// <summary>
    /// Adiciona o script da ação a cada um dos objetos dos jogadores, para definir essa ação ao personagem.
    /// </summary>
    private void AddActionToPlayers()
    {
        _kickAction = _levelPlayers[0].Object.AddComponent<KickAction>();
        _levelPlayers[0].Object.GetComponent<PlayerController>().SetAction(_kickAction, this);

        _kickAction = _levelPlayers[1].Object.AddComponent<KickAction>();
        _levelPlayers[1].Object.GetComponent<PlayerController>().SetAction(_kickAction, this);
    }

    private GameObject GetPlayer1InScene()
    {
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");

        if (player1 != null)
        {
            return player1;
        }
        else
        {
            Debug.Log("nao:GetPlayer1InScene");
            return null;
        }
    }

    private GameObject GetPlayer2InScene()
    {
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

        if (player2 != null)
        {
            return player2;
        }
        else
        {
            Debug.Log("nao:GetPlayer1InScene");
            return null;
        }
    }

    public bool IsAllPlayersSpawned()
    {
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

        if (player1 != null && player2 != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private string GetScorer()
    {
        if (_ballController.Player1Scored)
        {
            return GetPlayer1InScene().tag;
        }
        else if (_ballController.Player2Scored)
        {
            return GetPlayer2InScene().tag;
        }
        else
        {
            return "";
        }
    }

    /// <summary>
    /// Atribui os pontos do marcador e atualiza no ecrã.
    /// </summary>
    private void UpdateScore(string scorerTag)
    {
        if (scorerTag == "Player1")
        {
            int currentScore = GetPlayer1InScene().GetComponent<PlayerController>().Score;
            int scorePerRound = _scoreController.AddScore();
            int newScore = currentScore + scorePerRound;
            _scoreController.DisplayScoreObjectText(1, newScore);

            GetPlayer1InScene().GetComponent<PlayerController>().Score = newScore;
        }
        if (scorerTag == "Player2")
        {
            int currentScore = GetPlayer2InScene().GetComponent<PlayerController>().Score;
            int scorePerRound = _scoreController.AddScore();
            int newScore = currentScore + scorePerRound;
            _scoreController.DisplayScoreObjectText(2, currentScore);

            GetPlayer2InScene().GetComponent<PlayerController>().Score = newScore;
        }
    }

    private void FreezePlayers(float freezingTime)
    {
        GameObject player1 = GetPlayer1InScene();
        GameObject player2 = GetPlayer2InScene();
        PlayerController player1Object = player1.GetComponent<PlayerController>();
        PlayerController player2Object = player2.GetComponent<PlayerController>();

        player1Object.Freeze(freezingTime);
        player2Object.Freeze(freezingTime);

        //_levelPlayers[0].Object.GetComponent<PlayerController>().Freeze(freezingTime);
        //_levelPlayers[1].Object.GetComponent<PlayerController>().Freeze(freezingTime);
    }

    /// <summary>
    /// É executado após o intervalo de espera para iniciar outra ronda.
    /// Responsável por inicializar novamente os componentes necessários para que a ronda comece.
    /// </summary>
    private void RestartRound()
    {
        _timerController.Play();

        _timerController.SetInitialTime();

        _roundController.DisableNextRoundIntro();

        _ballController.Player1Scored = false;
        _ballController.Player2Scored = false;

        _ballController.Unfreeze();
        _goal1Controller.Unfreeze();
        _goal2Controller.Unfreeze();

        SetInitialPositions();

        DestroyAllPowerUps();

        InvokeRepeating(nameof(SpawnPowerUp), 10f, 10f);
    }

    private void SetInitialPositions()
    {
        Vector3 player1Position = GetPlayer1InScene().transform.position;
        player1Position = _player1Level1Prefab.transform.position;

        Vector3 player2Position = GetPlayer2InScene().transform.position;
        player2Position = _player2Level1Prefab.transform.position;

        //_levelPlayers[0].Object.transform.position = _levelPlayers[0].InitialPosition;
        //_levelPlayers[0].Object.transform.rotation = _levelPlayers[0].InitialRotation;

        //_levelPlayers[1].Object.transform.position = _levelPlayers[1].InitialPosition;
        //_levelPlayers[1].Object.transform.rotation = _levelPlayers[1].InitialRotation;

        _ballObject.transform.position = _ballPrefab.transform.position;
        _ballObject.transform.rotation = _ballPrefab.transform.rotation;

        _goal1Object.transform.position = _goal1Prefab.transform.position;
        _goal1Object.transform.rotation = _goal1Prefab.transform.rotation;

        _goal2Object.transform.position = _goal2Prefab.transform.position;
        _goal2Object.transform.rotation = _goal2Prefab.transform.rotation;
    }

    private void DestroyAllPowerUps()
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
        SceneManager.LoadScene("MainMenuScene");
    }
}