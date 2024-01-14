using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
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

    // variáveis sobre os prefabs específicos dos jogadores
    [SerializeField] private GameObject _player1Prefab;
    [SerializeField] private GameObject _player2Prefab;

    // para os objetos do nível - bola
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private GameObject _ballObject;
    private BallController _ballController;

    // para o som de marcar golo
    private AudioSource _goalAudioSource;

    // para os objetos do nível - balizas
    [SerializeField] private GameObject _goal1Prefab;
    [SerializeField] private GameObject _goal2Prefab;
    [SerializeField] private GameObject _goal1Object;
    [SerializeField] private GameObject _goal2Object;
    private GoalController _goal1Controller;
    private GoalController _goal2Controller;

    private bool _gameStarted = false;
    private bool _gameFinished = false;

    // para os objetos do nível - power ups
    private readonly List<GameObject> _powerUps = new();
    [SerializeField] private GameObject _powerUp;

    // para definir a ação dos jogadores neste nível
    private KickAction _kickAction;

    // referencia para as ações quando termina o nivel
    private SuccessAction _successAction;
    private FailureAction _failureAction;

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


    /* PROPRIEDADES PÚBLICAS */

    public static Level1Controller Instance
    {
        get { return _instance; }
        set { _instance = value; }
    }


    /* MÉTODOS DO MONOBEHAVIOUR */

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
        _timerController = TimerController.Instance;
        TimerController.Freeze();

        _roundController.DisplayCurrentRound();
        _roundController.DisplayMaxRounds();

        _ballController = _ballObject.GetComponent<BallController>();
        _goal1Controller = _goal1Object.GetComponent<GoalController>();
        _goal2Controller = _goal2Object.GetComponent<GoalController>();

        _goalAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // quando o jogo terminou
        if (_gameFinished)
        {
            return;
        }

        // quando o jogo ainda não iniciou
        if (!_gameStarted && IsAllPlayersSpawned())
        {
            _gameStarted = true;
            InitAfterPlayersReady();
            return;
        }

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

                int player1ID = GetPlayer1InScene().GetComponent<PlayerController>().PlayerID;
                int player1Score = GetPlayer1InScene().GetComponent<PlayerController>().Score;
                string finishedLevelText = "Jogador " + player1ID.ToString() + ": " + player1Score.ToString() + "\n";

                int player2ID = GetPlayer2InScene().GetComponent<PlayerController>().PlayerID;
                int player2Score = GetPlayer2InScene().GetComponent<PlayerController>().Score;
                finishedLevelText += "Jogador " + player2ID.ToString() + ": " + player2Score.ToString() + "\n";

                _finishedLevelPanel.SetActive(true);
                _finishedLevelDescription.GetComponent<Text>().text = finishedLevelText;

                FinishLevel(player1Score, player2Score);

                _buttonPause.SetActive(false);

                _gameFinished = true;
            }
            // senão iniciar outra ronda
            else
            {
                float freezingTime = 5f;
                FreezePlayers(freezingTime);

                GetPlayer1InScene().GetComponent<PlayerController>().StopMoveAnimation();
                GetPlayer2InScene().GetComponent<PlayerController>().StopMoveAnimation();

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

                int player1ID = GetPlayer1InScene().GetComponent<PlayerController>().PlayerID;
                int player1Score = GetPlayer1InScene().GetComponent<PlayerController>().Score;
                string finishedLevelText = "Jogador " + player1ID.ToString() + ": " + player1Score.ToString() + "\n";

                int player2ID = GetPlayer2InScene().GetComponent<PlayerController>().PlayerID;
                int player2Score = GetPlayer2InScene().GetComponent<PlayerController>().Score;
                finishedLevelText += "Jogador " + player2ID.ToString() + ": " + player2Score.ToString() + "\n";

                _finishedLevelPanel.SetActive(true);
                _finishedLevelDescription.GetComponent<Text>().text = finishedLevelText;

                FinishLevel(player1Score, player2Score);

                _buttonPause.SetActive(false);

                _gameFinished = true;
            }
            // senão - iniciar outra ronda
            else
            {
                float freezingTime = 5f;
                FreezePlayers(freezingTime);

                GetPlayer1InScene().GetComponent<PlayerController>().StopMoveAnimation();
                GetPlayer2InScene().GetComponent<PlayerController>().StopMoveAnimation();

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
    public void InitAfterPlayersReady()
    {
        TimerController.Unfreeze();

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

    private GameObject GetPlayer1InScene()
    {
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");

        if (player1 != null)
        {
            return player1;
        }
        else
        {
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
        PlayerController player1Controller = GetPlayer1InScene().GetComponent<PlayerController>();
        PlayerController player2Controller = GetPlayer2InScene().GetComponent<PlayerController>();

        player1Controller.Freeze(freezingTime);
        player2Controller.Freeze(freezingTime);
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
        GetPlayer1InScene().transform.position = _player1Prefab.transform.position;
        GetPlayer1InScene().transform.rotation = _player1Prefab.transform.rotation;
        GetPlayer2InScene().transform.position = _player2Prefab.transform.position;
        GetPlayer2InScene().transform.rotation = _player2Prefab.transform.rotation;

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
        _goalAudioSource.Play();
    }

    private void FinishLevel(int player1Score, int player2Score)
    {
        _ballObject.transform.position = _ballPrefab.transform.position;
        _ballObject.transform.rotation = _ballPrefab.transform.rotation;

        _goal1Object.transform.position = _goal1Prefab.transform.position;
        _goal1Object.transform.rotation = _goal1Prefab.transform.rotation;

        _goal2Object.transform.position = _goal2Prefab.transform.position;
        _goal2Object.transform.rotation = _goal2Prefab.transform.rotation;

        GameObject backgroundMusicObject = GameObject.Find("BackgroundMusicController");
        AudioSource backgroundMusicAudioSource = backgroundMusicObject.GetComponent<AudioSource>();
        backgroundMusicAudioSource.Stop();
        backgroundMusicAudioSource.clip = (AudioClip)Resources.Load("finish-level");
        backgroundMusicAudioSource.Play();

        GameObject player1Object = GetPlayer1InScene();
        GameObject player2Object = GetPlayer2InScene();

        player1Object.transform.position = new Vector3(43f, 6f, 73f);
        player1Object.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        player2Object.transform.position = new Vector3(57f, 6f, 73f);
        player2Object.transform.rotation = Quaternion.Euler(0f, 270f, 0f);

        player1Object.GetComponent<PlayerController>().StopMoveAnimation();
        player2Object.GetComponent<PlayerController>().StopMoveAnimation();

        Destroy(player1Object.GetComponent<KickAction>());
        Destroy(player1Object.GetComponent<WalkAction>());

        Destroy(player2Object.GetComponent<KickAction>());
        Destroy(player2Object.GetComponent<WalkAction>());

        // se empatarem
        if (player1Score == player2Score)
        {
            _failureAction = player1Object.AddComponent<FailureAction>();
            player1Object.GetComponent<PlayerController>().SetAction(_failureAction, this);

            _failureAction = player2Object.AddComponent<FailureAction>();
            player2Object.GetComponent<PlayerController>().SetAction(_failureAction, this);
        }
        // se jogador 1 ganhou
        else if (player1Score > player2Score)
        {
            _successAction = player1Object.AddComponent<SuccessAction>();
            player1Object.GetComponent<PlayerController>().SetAction(_successAction, this);

            _failureAction = player2Object.AddComponent<FailureAction>();
            player2Object.GetComponent<PlayerController>().SetAction(_failureAction, this);
        }
        // se jogador 2 ganhou
        else
        {
            _failureAction = player1Object.AddComponent<FailureAction>();
            player1Object.GetComponent<PlayerController>().SetAction(_failureAction, this);

            _successAction = player2Object.AddComponent<SuccessAction>();
            player2Object.GetComponent<PlayerController>().SetAction(_successAction, this);
        }
    }
}