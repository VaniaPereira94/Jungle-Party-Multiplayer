using UnityEngine;


/// <summary>
/// Armazenar informações de cada jogador.
/// Necessário para manter os dados dos jogadores (pontuação, etc.) entre cenas.
/// </summary>
public class GamePlayerModel
{
    // ATRIBUTOS PRIVADOS

    private int _id;
    private GameObject _prefab;
    private int _globalScore;


    // PROPRIEDADES PÚBLICAS

    public int ID
    {
        get { return _id; }
        set { _id = value; }
    }

    public GameObject Prefab
    {
        get { return _prefab; }
        set { _prefab = value; }
    }

    public int GlobalScore
    {
        get { return _globalScore; }
        set { _globalScore = value; }
    }


    // CONSTRUTOR

    public GamePlayerModel(int id, GameObject prefab, int globalScore)
    {
        _id = id;
        _prefab = prefab;
        _globalScore = globalScore;
    }
}