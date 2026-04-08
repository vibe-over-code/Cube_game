using UnityEngine;
using TMPro;
using YG; // Подключаем плагин Яндекс Игр 2

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
        // Подписываемся на событие смены языка (если оно есть в плагине) 
        // или просто обновляем при включении объекта
        UpdateLanguage();
    }

    public void UpdateLanguage()
    {
        if (_textElement == null) return;

        // Проверяем язык через PluginYG2
        // "ru" и "en" — стандартные коды Яндекса
        if (YG2.lang == "ru")
        {
            _textElement.text = russianText;
        }
        else
        {
            // По умолчанию ставим английский для всех остальных стран
            _textElement.text = englishText;
        }
    }
}
