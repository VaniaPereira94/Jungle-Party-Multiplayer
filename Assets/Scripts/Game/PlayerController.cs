using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;


/// <summary>
/// Armazena dados sobre cada personagem criada, no início do jogo.
/// E controla os movimentos, ações e características do mesmo.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    // variável para identificação do jogador
    [SerializeField] private int _playerID;

    // variáveis para a física (movimento e velocidade do personagem)
    private Rigidbody _rigidbody;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpForce;

    // para controlar as animações
    private Animator _animator;
    private bool _isWalking = false;

    // para verificar se o personagem está a pisar no chão
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundDistance;

    // para verificar se o personagem está congelado
    private bool _isFrozen = false;
    private readonly float _freezingTime = 3f;

    // guarda a ação de andar do jogador (é igual para todos os níveis)
    private WalkAction _walkAction;

    // guarda a ação de chutar
    private KickAction _kickAction;

    // guarda qual ação o jogador deve executar
    private IPlayerAction _currentAction;

    // para os efeitos das power ups no jogador
    private readonly float _effectTime = 3f;
    private float _normalSpeed;
    private float _halfSpeed;
    private float _doubleSpeed;

    private PlayerControls _playerControls;

    [SerializeField] private string _tigerPlayer1MaterialName;
    [SerializeField] private string _tigerPlayer2MaterialName;


    /* PROPRIEDADES PÚBLICAS */

    public int PlayerID
    {
        get { return _playerID; }
        set { _playerID = value; }
    }

    public bool IsWalking
    {
        get { return _isWalking; }
        set { _isWalking = value; }
    }


    /* MÉTODOS DO NETWORKBEHAVIOUR E MONOBEHAVIOUR */

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer && IsOwner)
        {
            SetInitialPlayerServerRPC(new Vector3(49.95f, 6f, 73f), "Player1", _tigerPlayer1MaterialName);
        }
        else if (!IsServer && IsOwner)
        {
            SetInitialPlayerServerRPC(new Vector3(49.95f, 6f, 81.83f), "Player2", _tigerPlayer2MaterialName);
        }
    }

    /// <summary>
    /// É executado antes da primeira frame.
    /// </summary>
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _walkAction = gameObject.AddComponent<WalkAction>();

        _normalSpeed = _moveSpeed;
        _halfSpeed = _moveSpeed / 2;
        _doubleSpeed = _moveSpeed * 2;

        _playerControls = new PlayerControls();
        _playerControls.Enable();
    }

    /// <summary>
    /// É executado uma vez por frame.
    /// </summary>
    private void Update()
    {
        bool actionInput = _playerControls.Player.Action.triggered;

        if (IsServer && IsLocalPlayer)
        {
            DoActions(actionInput);
        }
        else if (IsLocalPlayer)
        {
            DoActionsServerRPC(actionInput);
        }
    }

    /// <summary>
    /// É executado em intervalos fixos.
    /// </summary>
    private void FixedUpdate()
    {
        Vector3 movementInput = _playerControls.Player.Move.ReadValue<Vector2>();

        if (IsServer && IsLocalPlayer)
        {
            Move(movementInput);
        }
        else if (IsLocalPlayer)
        {
            MoveServerRPC(movementInput);
        }
    }


    /* MÉTODOS DE SINCRONIZAÇÃO */

    [ServerRpc]
    private void SetInitialPlayerServerRPC(Vector3 position, string tagName, string tigerMaterialPath)
    {
        SetInitialPlayerClientRPC(position, tagName, tigerMaterialPath);
    }

    [ClientRpc]
    private void SetInitialPlayerClientRPC(Vector3 position, string tagName, string tigerMaterialPath)
    {
        transform.position = position;
        tag = tagName;

        GameObject tigerMesh = transform.Find("Tiger Mesh").gameObject;
        if (tigerMesh != null)
        {
            //Material tigerMaterial = Resources.Load<Material>(tigerMaterialPath);
            //SkinnedMeshRenderer skinnedMeshRenderer = tigerMesh.GetComponent<SkinnedMeshRenderer>();
            //skinnedMeshRenderer.materials[0] = tigerMaterial;
            //skinnedMeshRenderer.material = tigerMaterial;

            //Material[] mats = skinnedMeshRenderer.materials;
            //Material mat = (Material)Resources.Load(tigerMaterialPath);
            //mats[0] = mat;
            //skinnedMeshRenderer.materials = mats;
        }

        AddActionToPlayer();
    }

    [ServerRpc]
    private void MoveServerRPC(Vector2 movementInput)
    {
        Move(movementInput);
    }

    [ServerRpc]
    private void DoActionsServerRPC(bool actionInput)
    {
        DoActions(actionInput);
    }


    /* MÉTODOS DE PLAYERCONTROLLER */

    private bool IsAllPlayersSpawned()
    {
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

        if (player1 != null && player2 != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// É executado quando existe alguma colisão do jogador com outro objeto.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // colisão com alguma power up - destroi a power up e aplica o efeito ao jogador
        if (collision.gameObject.CompareTag("PowerUp"))
        {
            Destroy(collision.gameObject);

            int value = GenerateEffect();
            ApplyEffect(value);
        }

        if (IsAllPlayersSpawned())
        {
            string oppositePlayerTag = GetOppositePlayer().tag;

            // colisão com o outro jogador
            if (collision.gameObject.CompareTag(oppositePlayerTag))
            {
                if (_currentAction is CarryAction)
                {
                    _currentAction.Collide(collision);
                }

                if (_currentAction is ThrowLvl4Action)
                {
                    _currentAction.Collide(collision);
                }
            }
        }

        if (_currentAction is KickAction)  // significa que está no nível 1
        {
            // colisão com a bola
            if (collision.gameObject.CompareTag("Ball"))
            {
                _currentAction.Collide(collision);

                bool actionInput = _playerControls.Player.Action.triggered;

                // se o jogador pressiona a tecla de ação
                if (actionInput)
                {
                    _currentAction.Enter();
                }
            }
        }
    }

    /// <summary>
    /// É executado quando colide com algum objeto e entra no seu colisor.
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        // colisão com alguma parede da arena - impede que o jogador saia da arena
        if (collision.gameObject.CompareTag("Wall"))
        {
            // atualiza a posição do jogador para entrar novamente na arena
            Vector3 oppositeDirection = transform.position - collision.collider.ClosestPoint(transform.position);
            transform.position += oppositeDirection.normalized * 0.12f;
        }
    }

    /// <summary>
    /// É executado quando o jogador colide com algum objeto que tem o "isTrigger" ativado no seu colisor.
    /// </summary>
    private void OnTriggerEnter(Collider collider)
    {
        // colisão com alguma parede da arena - para saber que o jogador saiu da arena
        if (collider.CompareTag("Wall"))
        {
            if (_currentAction is CarryAction)
            {
                _currentAction.Trigger(collider);
            }
        }

        // colisão com alguma maçã
        if (collider.gameObject.CompareTag("Apple"))
        {
            if (_currentAction is ThrowLvl2Action)
            {
                _currentAction.Trigger(collider);
            }
        }
    }


    /* MÉTODOS DO PLAYERCONTROLLER */

    void AddActionToPlayer()
    {
        _kickAction = this.gameObject.AddComponent<KickAction>();

        Level1Controller level1Controller = GameObject.Find("Level1Controller").GetComponent<Level1Controller>();
        SetAction(_kickAction, level1Controller);
    }

    public void SetAction(IPlayerAction action, MonoBehaviour level)
    {
        _currentAction = action;
        _currentAction.Level = level;
        _currentAction.Player = this;
    }

    private void DoActions(bool actionInput)
    {
        if (_currentAction is SuccessAction)
        {
            _currentAction.Enter();
        }
        if (_currentAction is FailureAction)
        {
            _currentAction.Enter();
        }

        if (!_isFrozen)
        {
            //bool actionInput = _playerControls.Player.Action.triggered;

            // se o jogador pressiona a tecla de ação
            if (actionInput)
            {
                if (_currentAction is KickAction)  // significa que está no nível 1
                {
                    _currentAction.Enter();
                }
                if (_currentAction is ThrowLvl2Action)  // significa que está no nível 2
                {
                    _currentAction.Enter();
                }
                if (_currentAction is ThrowLvl4Action)  // significa que está no nível 4
                {
                    if (!_isWalking)
                    {
                        _currentAction.Enter();
                    }
                }
            }
            else
            {
                if (_currentAction is KickAction)
                {
                    _currentAction.Exit();
                }
                if (_currentAction is CarryAction)
                {
                    _currentAction.Exit();
                }
                if (_currentAction is ThrowLvl4Action)
                {
                    _currentAction.Exit();
                }
            }
        }
    }

    private void Move(Vector2 movementInput)
    {
        if (!_isFrozen)
        {
            if (movementInput.magnitude > 0)
            {
                UpdateMovement(movementInput);
                UpdateDirection(movementInput);

                if (!_isWalking)
                {
                    _walkAction.Enter();
                    _isWalking = true;
                }
            }
            else
            {
                if (_isWalking)
                {
                    _walkAction.Exit();
                    _isWalking = false;
                }
            }
        }
    }

    private void UpdateMovement(Vector2 movementInput)
    {
        Vector3 movement = _moveSpeed * Time.fixedDeltaTime * new Vector3(movementInput.x, 0f, movementInput.y);
        _rigidbody.MovePosition(transform.position + movement);
    }

    private void UpdateDirection(Vector2 movementInput)
    {
        Vector3 direction = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public GameObject GetCurrentPlayer()
    {
        return this.gameObject;
    }

    public GameObject GetOppositePlayer()
    {
        if (_playerID == 1)
        {
            GameObject oppositePlayer = GameObject.FindGameObjectWithTag("Player2");
            return oppositePlayer;
        }
        else
        {
            GameObject oppositePlayer = GameObject.FindGameObjectWithTag("Player1");
            return oppositePlayer;
        }
    }

    public float GetFreezingTime()
    {
        return _freezingTime;
    }

    /// <param name="freezingTime">Tempo de congelamento. "-1" congela para sempre.</param>
    public void Freeze(float freezingTime)
    {
        _isFrozen = true;

        if (freezingTime != -1)
        {
            Invoke(nameof(Unfreeze), freezingTime);
        }
    }

    public void Unfreeze()
    {
        _isFrozen = false;
    }

    int GenerateEffect()
    {
        return Random.Range(1, 4);
    }

    void ApplyEffect(int value)
    {
        switch (value)
        {
            case (int)PowerUpEffect.SPEED:
                _moveSpeed = _doubleSpeed;
                Invoke(nameof(SetNormalSpeed), _effectTime);
                break;

            case (int)PowerUpEffect.SLOW:
                _moveSpeed = _halfSpeed;
                Invoke(nameof(SetNormalSpeed), _effectTime);
                break;

            case (int)PowerUpEffect.STUN:
                _isWalking = false;
                Freeze(_freezingTime);
                break;
        }
    }

    public void SetNormalSpeed()
    {
        _moveSpeed = _normalSpeed;
    }
}