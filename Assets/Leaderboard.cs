using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

public class YandexSDK : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void YandexInitSDK();
    [DllImport("__Internal")] private static extern void YandexAuthPlayer();
    [DllImport("__Internal")] private static extern void YandexSendScore(string leaderboard, int score);
    [DllImport("__Internal")] private static extern void YandexGetLeaderboard(string leaderboard);
#endif

    [Header("UI Buttons")]
    public Button authButton;
    public Button sendScoreButton;
    public Button showTopButton;
    public TMP_Text leaderboardText;

    [Header("Settings")]
    public string leaderboardName = "BestScore";
    public int testScore = 120;

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        YandexInitSDK();
#endif
        if (authButton != null) authButton.onClick.AddListener(Authorize);
        if (sendScoreButton != null) sendScoreButton.onClick.AddListener(() => SendScore(testScore));
        if (showTopButton != null) showTopButton.onClick.AddListener(ShowLeaderboardTop);
    }

    public void Authorize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        YandexAuthPlayer();
#else
        Debug.Log("Авторизация работает только в WebGL на Яндекс Играх");
#endif
    }

    public void SendScore(int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        YandexSendScore(leaderboardName, score);
#else
        Debug.Log("Отправка очков работает только в WebGL на Яндекс Играх");
#endif
    }

    public void ShowLeaderboardTop()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        YandexGetLeaderboard(leaderboardName);
#else
        Debug.Log("Топ игроков работает только в WebGL на Яндекс Играх");
#endif
    }

    // Этот метод вызывается из JS через SendMessage
    public void ReceiveLeaderboardData(string data)
    {
        if (leaderboardText != null)
        {
            var lines = data.Split('\n');
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                sb.AppendLine($"{i + 1}. {lines[i]}");
            }
            leaderboardText.text = sb.ToString();
        }
        else
        {
            Debug.Log(data);
        }
    }
}
