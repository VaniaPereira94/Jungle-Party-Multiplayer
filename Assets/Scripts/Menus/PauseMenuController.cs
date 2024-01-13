using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Trata das intera��es do utilizador com o menu de pausa,
/// existente em cada n�vel ao clicar no bot�o de pausa.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    /* ATRIBUTOS */

    // vari�veis para os objetos deste menu
    [SerializeField] private GameObject _buttonPause;
    [SerializeField] private GameObject _menuPause;
    [SerializeField] private GameObject _BackgroundMusicController;


    /* M�TODOS */

    public void Pause()
    {
        _buttonPause.SetActive(false);
        _menuPause.SetActive(true);
        _BackgroundMusicController.GetComponent<AudioSource>().Pause();
    }

    public void Resume()
    {
        _buttonPause.SetActive(true);
        _menuPause.SetActive(false);
        _BackgroundMusicController.GetComponent<AudioSource>().Play();
    }

    public void Quit()
    {
        _menuPause.SetActive(false);

        string sceneName = "MainMenuScene";
        SceneManager.LoadScene(sceneName);
    }
}