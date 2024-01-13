using TMPro;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// É responsável por tratar da pontuação de cada jogador e exibir-la no ecrã.
/// </summary>
public class ScoreController : NetworkBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    // variável para a pontuação por ronda
    [SerializeField] private int _pointsPerRound;

    // para os componentes da UI - texto da pontuação de cada jogador
    [SerializeField] private GameObject _scorePlayer1;
    [SerializeField] private GameObject _scorePlayer2;


    /* MÉTODOS */

    public int AddScore()
    {
        return _pointsPerRound;
    }

    public void DisplayScoreObjectText(int winnerID, int score)
    {
        if (winnerID == 1)
        {
            _scorePlayer1.GetComponent<TextMeshProUGUI>().text = score.ToString();
        }
        else if (winnerID == 2)
        {
            _scorePlayer2.GetComponent<TextMeshProUGUI>().text = score.ToString();
        }
    }
}