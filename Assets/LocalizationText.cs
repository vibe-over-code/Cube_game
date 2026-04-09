using UnityEngine;
using TMPro;
using YG;

public class LocalizationText : MonoBehaviour
{
    [Header("Перевод")]
    [TextArea] public string russianText;
    [TextArea] public string englishText;

    private TextMeshProUGUI _textElement;

    private void Awake()
    {
        _textElement = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        YG2.onSwitchLang += HandleLanguageChanged;
        YG2.onGetSDKData += HandleSDKDataReceived;
        UpdateLanguage();
    }

    private void OnDisable()
    {
        YG2.onSwitchLang -= HandleLanguageChanged;
        YG2.onGetSDKData -= HandleSDKDataReceived;
    }

    public void UpdateLanguage()
    {
        if (_textElement == null)
        {
            return;
        }

        if (YG2.lang == "ru")
        {
            _textElement.text = russianText;
        }
        else
        {
            _textElement.text = englishText;
        }
    }

    private void HandleLanguageChanged(string _)
    {
        UpdateLanguage();
    }

    private void HandleSDKDataReceived()
    {
        UpdateLanguage();
    }
}
