using UnityEngine;


namespace Multiplayer
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    T[] objs = FindObjectsOfType<T>();
                    if (objs.Length > 0)
                    {
                        T instance = objs[0];
                        _instance = instance;
                    }
                    else
                    {
                        GameObject gs = new();
                        gs.name = typeof(T).Name;
                        _instance = gs.AddComponent<T>();
                        DontDestroyOnLoad(gs);
                    }
                }

                return _instance;
            }
        }
    }
}