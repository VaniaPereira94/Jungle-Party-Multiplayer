using UnityEngine;
using UnityEngine.UI;

namespace lobbyTutorial
{
    public class AuthenticateUI : MonoBehaviour
    {
        [SerializeField] private Button authenticateButton;

        private void Awake()
        {
            authenticateButton.onClick.AddListener(() =>
            {
                LobbyManager.Instance.Authenticate(EditPlayerName.Instance.GetPlayerName());
                Hide();
            });
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}