using UnityEngine;


/// <summary>
/// Trata da ação do jogador de lançar a mação no nível 2.
/// </summary>
public class ThrowLvl2Action : MonoBehaviour, IPlayerAction
{
    /* ATRIBUTOS PRIVADOS */

    // referência ao nível 2
    private Level2Controller _level2;

    // referência ao controlador do jogador
    private PlayerController _player;

    // para controlar as animações
    private Animator _animator;

    // para a maçã atual que o jogador apanhou
    private GameObject _apple;


    /* PROPRIEDADES PÚBLICAS */

    public MonoBehaviour Level
    {
        get { return _level2; }
        set { _level2 = (Level2Controller)value; }
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
        if (_apple != null)
        {
            AppleController appleController = _apple.GetComponent<AppleController>();
            appleController.ThrowApple();

            _apple = null;
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
        if (_apple == null)
        {
            _apple = collider.gameObject;
            AppleController appleController = _apple.GetComponent<AppleController>();

            if (appleController.HasThrown)
            {
                _level2.PlaySplashSound();

                GameObject healthBarCtrl = GameObject.FindGameObjectWithTag("HealthBarCtrl");
                healthBarCtrl.GetComponent<HealthBarController>().TakeDamage(_player.PlayerID);

                Destroy(collider.gameObject);
            }
            else
            {
                appleController.SetPlayer(_player.gameObject);
                appleController.SetPlayerAsParent(_player.gameObject);
                appleController.SetLocalPosition(new Vector3(0.0419f, 0.389f, 0.445f));
                appleController.SetLocalRotation(Quaternion.Euler(-90, 0, 0));
            }
        }
    }

    public void SetThrownActions()
    {
        _apple = null;
        _animator.Play("Throw", -1, 0.1f);
        _animator.Update(0f);
    }
}