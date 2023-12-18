using UnityEngine;


/// <summary>
/// Diferente da classe GamePlayerModel que armazena as informações de cada jogador de forma geral, de nível para nível.
/// Nesta classe são apenas armazenadas as informações no respetivo nível.
/// </summary>
public class LevelPlayerModel
{
    // ATRIBUTOS PRIVADOS

    private int _id;
    private int _levelScore;
    private GameObject _object;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;


    // PROPRIEDADES PÚBLICAS

    public int ID
    {
        get { return _id; }
        set { _id = value; }
    }

    public int LevelScore
    {
        get { return _levelScore; }
        set { _levelScore = value; }
    }

    public GameObject Object
    {
        get { return _object; }
        set { _object = value; }
    }

    public Vector3 InitialPosition
    {
        get { return _initialPosition; }
        set { _initialPosition = value; }
    }

    public Quaternion InitialRotation
    {
        get { return _initialRotation; }
        set { _initialRotation = value; }
    }


    // CONSTRUTOR

    public LevelPlayerModel(int id, int levelScore, Vector3 initialPosition, Quaternion initialRotation)
    {
        _id = id;
        _levelScore = levelScore;
        _initialPosition = initialPosition;
        _initialRotation = initialRotation;
    }
}