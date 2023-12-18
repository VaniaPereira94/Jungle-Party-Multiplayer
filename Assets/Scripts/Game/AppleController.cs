using UnityEngine;


/// <summary>
/// Esta classe controla a maçã existente no nível 2.
/// </summary>
public class AppleController : MonoBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    // referência do jogador que tem a maçã
    private GameObject _player;

    private bool _isGrabbed = false;
    private bool _hasThrown = false;
    private Vector3 _thrownDirection;

    public float _throwForce = 10f;

    private ThrowLvl2Action _throwLvl2Action;


    /* PROPRIEDADES PÚBLICAS */

    public bool IsGrabbed
    {
        get { return _isGrabbed; }
        set { _isGrabbed = value; }
    }

    public bool HasThrown
    {
        get { return _hasThrown; }
        set { _hasThrown = value; }
    }


    /* MÉTODOS */

    private void Update()
    {
        if (_hasThrown)
        {
            transform.position += _thrownDirection;
        }
    }

    public void SetPlayer(GameObject currentPlayer)
    {
        _player = currentPlayer;
    }

    public void SetLocalPosition(Vector3 position)
    {
        transform.localPosition = position;
    }

    public void SetLocalRotation(Quaternion rotation)
    {
        transform.localRotation = rotation;
    }

    public void SetLocalScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    public void SetPlayerAsParent(GameObject player)
    {
        transform.SetParent(player.transform);
    }

    public void ThrowApple()
    {
        _thrownDirection = _player.transform.forward * 0.1f;

        _throwLvl2Action = _player.GetComponent<ThrowLvl2Action>();
        _throwLvl2Action.SetThrownActions();

        _hasThrown = true;

        transform.SetParent(null);
        _player = null;
    }
}