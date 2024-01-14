using UnityEngine;


/// <summary>
/// Trata da animação do jogador quando perde no fim do jogo.
/// </summary>
public class FailureAction : MonoBehaviour, IPlayerAction
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

    public void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Enter()
    {
        if (_animator != null)
        {
            _animator.SetBool("isFailure", true);
        }
    }

    public void Exit()
    {
        // nada para ser implementado
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