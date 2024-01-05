using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Multiplayer
{
    public class AuthController : Singleton<AuthController>
    {
        public string CurrentPlayerId { get; private set; }

        public async Task Connect()
        {
            try
            {
                await UnityServices.InitializeAsync();

                // ao usar um clone do projeto com o package ParrelSync,
                // é necessário mudar de perfil de autenticação,
                // para forçar o clone a entrar com um utilizador diferente
#if UNITY_EDITOR
                if (ParrelSync.ClonesManager.IsClone())
                {
                    string customArgument = ParrelSync.ClonesManager.GetArgument();
                    AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
                }
#endif

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
                CurrentPlayerId = AuthenticationService.Instance.PlayerId;
                Debug.Log("Utilizador anónimo autenticado com sucesso!" + " Player ID: " + CurrentPlayerId);
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
}