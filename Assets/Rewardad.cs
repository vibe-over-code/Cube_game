using UnityEngine;

public class YandexAdManager : MonoBehaviour
{
    private void Awake()
    {
        // Custom rewarded ads were removed in favor of PluginYourGames integration.
        enabled = false;
    }
}
