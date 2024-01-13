using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Controla o relógio que existe em cada nível.
/// </summary>
public class TimerController : NetworkBehaviour
{
    /* ATRIBUTOS PRIVADOS */

    // variáveis para o tempo inicial e atual (o tempo é em segundos)
    [SerializeField] private float _startingTime;
    [SerializeField] private float _extraTime;
    private float _currentTime = 0f;

    // para o som do relógio
    private AudioSource _audioSource;
    public float clockSoundTime = 15f;
    private bool clockSoundPlayed = false;

    // referência para a barra de progresso do tempo visível no nível
    [SerializeField] private ProgressBarCircleController _progressBar;

    // para guardar uma instância única desta classe
    private static TimerController _instance;

    // para saber se o relógio está ativado ou em pausa
    private bool _isPause = false;


    /* PROPRIEDADES PÚBLICAS */

    public static TimerController Instance
    {
        get { return _instance; }
        set { _instance = value; }
    }

    public float CurrentTime
    {
        get { return _currentTime; }
        set { _currentTime = value; }
    }


    /* MÉTODOS */

    void Awake()
    {
        if (_instance != null)
        {
            return;
        }

        // guarda em memória apenas uma instância desta classe
        _instance = this;
    }

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        SetInitialTime();
    }

    public void SetInitialTime()
    {
        _currentTime = _startingTime;
        _progressBar.MaxValue = _currentTime;
        _progressBar.BarValue = _progressBar.MaxValue;
    }

    void Update()
    {
        if (IsFrozen() || IsOnPause())
        {
            return;
        }

        if (!clockSoundPlayed && _currentTime <= clockSoundTime)
        {
            PlayClockSound();
            clockSoundPlayed = true;
        }

        _currentTime -= Time.deltaTime;

        if (HasFinished())
        {
            _currentTime = 0f;

            StopClockSound();
            clockSoundPlayed = false;
        }

        // atualiza a barra de progresso
        int timeWithoutDecimals = GetTimeWithoutDecimals();
        _progressBar.UpdateValue(timeWithoutDecimals);
    }

    public int GetTimeWithoutDecimals()
    {
        return Mathf.FloorToInt(_currentTime);
    }

    public bool HasFinished()
    {
        return _currentTime <= 0f;
    }

    bool IsFrozen()
    {
        return Time.timeScale == 0f;
    }

    public bool IsOnPause()
    {
        return _isPause == true;
    }

    public void Play()
    {
        _isPause = false;
    }

    public void Pause()
    {
        _isPause = true;
    }

    public static void Freeze()
    {
        Time.timeScale = 0f;
    }

    public static void Unfreeze()
    {
        Time.timeScale = 1f;
    }

    private void PlayClockSound()
    {
        _audioSource.Play();
    }

    private void StopClockSound()
    {
        _audioSource.Stop();
    }

    public void SetExtraTime()
    {
        _currentTime = _extraTime;
        _progressBar.MaxValue = _extraTime;
        _progressBar.BarValue = _progressBar.MaxValue;
    }
}