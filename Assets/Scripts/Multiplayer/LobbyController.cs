using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    private string _playerName;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        _playerName = "CodeMonkey" + UnityEngine.Random.Range(10, 99);
        Debug.Log("Current player name:" + _playerName);
    }
}