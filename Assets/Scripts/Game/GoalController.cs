using UnityEngine;


/// <summary>
/// Esta classe controla cada baliza existente no nível 1.
/// </summary>
public class GoalController : MonoBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    // variável para a velocidade que a baliza se move
    [SerializeField] private float _initialSpeed;
    private float _speed;

    // para a distância do movimento tanto à esquerda como à direita
    [SerializeField] private float _distance;

    // para a posição central do movimento (por padrão é a posição inicial)
    private float _center;

    // para a direção atual do movimento (1.0 é direita | -1.0 é esquerda)
    private float _direction = 1.0f;

    // para ativar ou desativar o movimento
    private bool _activeMoviemnt = true;


    /* MÉTODOS */

    void Start()
    {
        // define a posição inicial (centro da baliza)
        _center = transform.position.x;

        _speed = _initialSpeed;
    }

    void Update()
    {
        if (_activeMoviemnt)
        {
            // move o objeto ao longo do eixo X
            transform.position += Vector3.right * _speed * _direction * Time.deltaTime;

            // se passou o limite à direita - definir para o alvo à esquerda e mudar para essa direção
            if (transform.position.x > _center + _distance && _direction == 1.0f)
            {
                _direction = -1.0f;
            }
            // se passou o limite à esquerda - definir para o alvo à direita e mudar para essa direção
            else if (transform.position.x < _center - _distance && _direction == -1.0f)
            {
                _direction = 1.0f;
            }
        }
    }

    public void Freeze()
    {
        _speed = 0.0f;
    }

    public void Unfreeze()
    {
        _speed = _initialSpeed;
    }
}