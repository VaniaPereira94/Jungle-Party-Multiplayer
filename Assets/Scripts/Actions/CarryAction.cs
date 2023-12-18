using UnityEngine;


/// <summary>
/// Trata da ação do jogador de empurrar no nível 3.
/// </summary>
public class CarryAction : MonoBehaviour, IPlayerAction
{
    /* ATRIBUTOS PRIVADOS */

    // referência ao nível 1
    private Level3Controller _level3;

    // referência ao controlador do jogador
    private PlayerController _player;

    // para controlar as animações
    private Animator _animator;

    // para a força que o jogador é empurrado 
    private float _pushForce = 5f;


    /* PROPRIEDADES PÚBLICAS */

    public MonoBehaviour Level
    {
        get { return _level3; }
        set { _level3 = (Level3Controller)value; }
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
        _animator.SetBool("isCarryingMove", true);
    }

    public void Exit()
    {
        _animator.SetBool("isCarryingMove", false);
    }

    public void Collide(Collision collision)
    {
        PushPlayer(collision);
    }

    public void PushPlayer(Collision collision)
    {
        // direção oposta de onde sofreu a colisão do outro jogador
        Vector3 direction = transform.position - collision.transform.position;
        direction = direction.normalized;

        // aplica a força
        Player.GetComponent<Rigidbody>().AddForce(direction * _pushForce, ForceMode.Impulse);
    }

    public void Trigger(Collider collider)
    {
        _level3.OutOfArena = true;
        GameObject loser = Player.gameObject;

        if (loser.tag == "Player1")
        {
            _level3.PlayerOutID = 1;
        }
        else if (loser.tag == "Player2")
        {
            _level3.PlayerOutID = 2;
        }
    }
}