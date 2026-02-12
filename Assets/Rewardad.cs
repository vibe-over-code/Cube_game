using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class YandexAdManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ShowYandexRewarded();

    public Button adButton; // назначь в инспекторе

    void Start()
    {
        if (adButton != null)
            adButton.onClick.AddListener(ShowAd);
    }

    public void ShowAd()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("Requesting rewarded ad...");
        ShowYandexRewarded();
#else
        Debug.Log("Rewarded ad only works in WebGL build on Yandex Games");
#endif
    }

    // Этот метод вызывается из JS при onRewarded
    public void OnRewarded()
    {
        Debug.Log("✅ Пользователь посмотрел рекламу — выдаём награду!");
        // Здесь добавь логику награды:
        // например, playerCoins += 100;
    }
}


