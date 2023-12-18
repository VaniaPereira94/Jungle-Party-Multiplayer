using UnityEngine;


/// <summary>
/// Trata da ação de andar do jogador.
/// </summary>
public class WalkAction : MonoBehaviour, IPlayerAction
{
    /* ATRIBUTOS PRIVADOS */

    // referência ao nível atual
    private MonoBehaviour _level;

    // referência ao controlador do jogador
    private PlayerController _player;

    // para controlar as animações
    private Animator _animator;


    /* PROPRIEDADES PÚBLICAS */

    public MonoBehaviour Level
    {
        get { return _level; }
        set { _level = value; }
    }

    public PlayerController Player
    {
        get { return _player; }
        set { _player = value; }
    }

    public Animator Animator
    {
        get { return _animator; }
        set { _animator = value; }
    }


    /* MÉTODOS */

    public void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Enter()
    {
        _animator.SetBool("isWalking", true);
    }

    public void Exit()
    {
        _animator.SetBool("isWalking", false);
    }

    public void Collide(Collision collision)
    {
        // nada para ser implementado
    }

    public void Trigger(Collider collider)
    {
        // nada para ser implementado 
    }
}