using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Configura o componente de UI da barra de progresso circular.
/// </summary>
public class ProgressBarCircleController : MonoBehaviour
{
    /* ATRIBUTOS PRIVADOS MAS PÚBLICOS NO EDITOR */

    [Header("Título")]
    [SerializeField] private string _title;
    [SerializeField] private Color _titleColor;
    [SerializeField] private Font _titleFont;

    [Header("Barra de Progresso")]
    [SerializeField] private float _maxValue;
    [SerializeField] private Color _barColor;
    [SerializeField] private Sprite _barBackgroundSprite;
    [SerializeField] private Color _barBackgroundColor;
    [SerializeField] private Color _textBackgroundColor;
    [SerializeField] private int _alertTime = 15;
    [SerializeField] private Color _barAlertColor;


    /* ATRIBUTOS PRIVADOS */

    private float _barValue;
    private Image _bar;
    private Image _barBackground;
    private Image _mask;
    private Text _textTitle;


    /* PROPRIEDADES PÚBLICAS */

    public float MaxValue
    {
        get { return _maxValue; }
        set { _maxValue = value; }
    }

    public float BarValue
    {
        get { return _barValue; }
        set
        {
            value = Mathf.Clamp(value, 0, _maxValue);
            _barValue = value;
            UpdateValue(_barValue);
        }
    }


    /* MÉTODOS */

    void Awake()
    {
        _barBackground = transform.Find("BarBackgroundCircle").GetComponent<Image>();
        _bar = transform.Find("BarCircle").GetComponent<Image>();
        _mask = transform.Find("Mask").GetComponent<Image>();
        _textTitle = transform.Find("Text").GetComponent<Text>();
    }

    void Start()
    {
        _textTitle.text = _title;
        _textTitle.color = _titleColor;
        _textTitle.font = _titleFont;

        _bar.color = _barColor;
        _mask.color = _textBackgroundColor;
        _barBackground.color = _barBackgroundColor;
        _barBackground.sprite = _barBackgroundSprite;
    }

    public void UpdateValue(float currentValue)
    {
        _bar.fillAmount = -(currentValue / _maxValue) + 1f;

        _textTitle.text = _title + currentValue;

        if (_alertTime >= currentValue)
        {
            _barBackground.color = _barAlertColor;
        }
        else
        {
            _barBackground.color = _barBackgroundColor;
        }
    }
}