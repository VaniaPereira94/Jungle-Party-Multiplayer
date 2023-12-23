using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationController : MonoBehaviour
{
    public static AuthenticationController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public async Task Connect()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Utilizador conectado com sucesso!");
        }
        catch (AuthenticationException exception)
        {
            Debug.LogException(exception);
        }
        catch (RequestFailedException exception)
        {
            Debug.LogException(exception);
        }
    }

    public async Task AuthenticateAnonymous()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Utilizador anónimo autenticado com sucesso!");
        }
        catch (AuthenticationException exception)
        {
            Debug.LogException(exception);
        }
        catch (RequestFailedException exception)
        {
            Debug.LogException(exception);
        }
    }
}