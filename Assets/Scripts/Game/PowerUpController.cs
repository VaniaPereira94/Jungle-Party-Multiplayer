using UnityEngine;


/// <summary>
/// Controla as power ups existentes e que são geradas em cada nível.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    /* ATRIBUTOS */

    private float _startHeight;
    private float _timeOffset;

    public float _bounceSpeed = 8;
    public float _bounceAmplitude = 0.05f;
    public float _rotationSpeed = 90;


    /* MÉTODOS */

    void Start()
    {
        _startHeight = transform.localPosition.y;
        _timeOffset = Random.value * Mathf.PI * 2;
    }

    void Update()
    {
        Animate();
        Spin();
    }

    void Animate()
    {
        float finalheight = _startHeight + Mathf.Sin(Time.time * _bounceSpeed + _timeOffset) * _bounceAmplitude;
        Vector3 position = transform.localPosition;
        position.y = finalheight;

        transform.localPosition = position;
    }

    void Spin()
    {
        Vector3 rotation = transform.localRotation.eulerAngles;
        rotation.y += _rotationSpeed * Time.deltaTime;

        transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    }
}