using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Trata das interações do utilizador com o menu de pausa,
/// existente em cada nível ao clicar no botão de pausa.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    /* ATRIBUTOS */

    // variáveis para os objetos deste menu
    [SerializeField] private GameObject _buttonPause;
    [SerializeField] private GameObject _menuPause;
    [SerializeField] private GameObject _BackgroundMusicController;


    /* MÉTODOS */

    public void Pause()
    {
        TimerController.Freeze();

        _buttonPause.SetActive(false);
        _menuPause.SetActive(true);
        _BackgroundMusicController.GetComponent<AudioSource>().Pause();
    }

    public void Resume()
    {
        TimerController.Unfreeze();

        _buttonPause.SetActive(true);
        _menuPause.SetActive(false);
        _BackgroundMusicController.GetComponent<AudioSource>().Play();
    }

    public void Quit()
    {
        TimerController.Unfreeze();
        _menuPause.SetActive(false);

        string sceneName = "MenuScene";
        SceneManager.LoadScene(sceneName);
    }
}