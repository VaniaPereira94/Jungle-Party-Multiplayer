using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Existe apenas uma instância desta classe durante a execução do jogo.
/// É inicializada no menu (uma vez) e passada de cena para cena (nível para nível),
/// para preversar dados necessários, como a pontuação ou o estado atual do jogo.
/// Para funcionar corretamente, cada cena deve ter um objeto na hierarquia com este script.
/// </summary>
public class GameController : MonoBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    // variáveis sobre os jogadores
    [SerializeField] private GameObject _player1Prefab;
    [SerializeField] private GameObject _player2Prefab;
    private List<GamePlayerModel> _gamePlayers = new();

    // para identificar o nível atual e o número total de níveis
    [SerializeField] private int _currentLevelID;
    [SerializeField] private int _numberOfLevels;

    // para guardar uma instância única desta classe
    private static GameController _instance;


    /* PROPRIEDADES PÚBLICAS */

    public List<GamePlayerModel> GamePlayers
    {
        get { return _gamePlayers; }
        set { _gamePlayers = value; }
    }

    public int CurrentLevelID
    {
        get { return _currentLevelID; }
        set { _currentLevelID = value; }
    }

    public static GameController Instance
    {
        get { return _instance; }
        set { _instance = value; }
    }


    /* MÉTODOS */

    /// <summary>
    /// É executado antes da função Start().
    /// </summary>
    void Awake()
    {
        if (_instance != null)
        {
            return;
        }

        // guarda em memória apenas uma instância desta classe,
        // e cria-la quando ainda não existe, tal como não destrui-la quando a cena muda.
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Chamar esta função sempre que um novo jogo iniciar,
    /// para alterar os valores atuais para os valores originais.
    /// </summary>
    public void InitiateGame()
    {
        _gamePlayers.Clear();

        AddPlayer(1, _player1Prefab, 0);
        AddPlayer(2, _player2Prefab, 0);
    }

    void AddPlayer(int id, GameObject prefab, int globalScore)
    {
        _gamePlayers.Add(new GamePlayerModel(id, prefab, globalScore));
    }

    /// <summary>
    /// É executado quando é clicado o botão de próximo nível, no painel de fim de nível,
    /// mas é chamado na função "FinishLevel" no controlador de nível.
    /// </summary>
    public void NextLevel(int scorePlayer1, int scorePlayer2)
    {
        string sceneName;
        _currentLevelID++;

        UpdateGlobalScore(scorePlayer1, scorePlayer2);

        // ir para a cena de final de jogo após o último nível
        if (_currentLevelID > _numberOfLevels)
        {
            sceneName = "FinalScene";
            SceneManager.LoadScene(sceneName);

            return;
        }

        sceneName = "Level" + _currentLevelID + "Scene";
        SceneManager.LoadScene(sceneName);
    }

    void UpdateGlobalScore(int scorePlayer1, int scorePlayer2)
    {
        _gamePlayers[0].GlobalScore += scorePlayer1;
        _gamePlayers[1].GlobalScore += scorePlayer2;
    }
}