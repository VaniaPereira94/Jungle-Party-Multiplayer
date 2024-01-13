using UnityEngine;


/// <summary>
/// Trata da ação do jogador de chutar a bola no nível 1.
/// </summary>
public class KickAction : MonoBehaviour, IPlayerAction
{
    /* ATRIBUTOS PRIVADOS */

    // referência ao nível 1
    private Level1Controller _level1;

    // referência ao controlador do jogador
    private PlayerController _player;

    // para controlar as animações
    private Animator _animator;

    // referência do controlador da bola
    private GameObject _ballObject;

    // para a força que a bola é empurrada e chutada
    private float _pushForce = 2f;
    private float _kickForce = 8f;

    // para saber se a jogador chutou e tocou na bola ao mesmo tempo
    private bool _kickingAndColliding = false;


    /* PROPRIEDADES PÚBLICAS */

    public MonoBehaviour Level
    {
        get { return _level1; }
        set { _level1 = (Level1Controller)value; }
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
        _ballObject = GameObject.FindGameObjectWithTag("Ball");
    }

    public void Enter()
    {
        _animator.SetBool("isKicking", true);
        _kickingAndColliding = true;
    }

    public void Exit()
    {
        if (_animator != null)
        {
            _animator.SetBool("isKicking", false);
        }
    }

    public void Collide(Collision collision)
    {
        if (_kickingAndColliding)
        {
            KickBall(collision);
            _kickingAndColliding = false;
        }
        else
        {
            PushBall(collision);
        }
    }

    public void PushBall(Collision collision)
    {
        Vector3 direction = collision.contacts[0].point - transform.position;

        // garante que a direção será sempre correta, independente da força aplicada
        direction = direction.normalized;

        Rigidbody rigidbodyBall = _ballObject.GetComponent<BallController>().Rigidbody;
        rigidbodyBall.AddForce(direction * _pushForce, ForceMode.Impulse);
    }

    public void KickBall(Collision collision)
    {
        Vector3 direction = collision.contacts[0].point - transform.position;

        // garante que a direção será sempre correta, independente da força aplicada
        direction = direction.normalized;

        Rigidbody rigidbodyBall = _ballObject.GetComponent<Rigidbody>();
        rigidbodyBall.AddForce(direction * _kickForce, ForceMode.Impulse);
    }

    public void Trigger(Collider collider)
    {
        // nada para ser implementado 
    }
}