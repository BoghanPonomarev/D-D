using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    static LocalizationManager instance;

    public void Initialize()
    {
        instance = this;
    }

    public static string Get(string key) => instance != null ? instance.Lookup(key) : key;

    string Lookup(string key) => key;
}
