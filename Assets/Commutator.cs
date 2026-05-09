using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class Commutator : MonoBehaviour
{
    private const string PointsKey = "PlayerPoints";
    private const string ExtraLifeUpgradeLevelKey = "ExtraLifeUpgradeLevel";
    private const string TutorialCompletedKey = "TutorialCompleted";
    private const string MusicMutedKey = "MusicMuted";
    private const int TutorialRequiredSpacePresses = 3;

    public float count;

    public TextMeshProUGUI YourText;
    public TextMeshProUGUI shopPointsText;
    public TextMeshProUGUI upgradeInfoText;
    public TextMeshProUGUI tutorialText;
    public Toggle musicToggle;
    public RectTransform livesUiParent;
    public Vector2 lifeHeartSize = new Vector2(72f, 72f);
    public float lifeHeartSpacing = 76f;
    public float tutorialPulseInterval = 0.5f;
    public AudioSource musicSource;

    public GameObject GameOver;
    public GameObject Game;
    public GameObject Menu;
    public GameObject Player;

    public AudioClip levelup;
    AudioSource audio;
    private LevelManager levelManager;
    private int points;
    private int extraLifeUpgradeLevel;
    private int livesRemaining;
    private float pointTimer;
    private RectTransform livesContainer;
    private readonly List<Image> lifeHeartImages = new List<Image>();
    private Sprite generatedHeartSprite;
    private bool pendingResetAfterAd;
    private float cachedAudioVolume = 1f;
    private Coroutine tutorialPulseRoutine;
    private bool isTutorialActive;
    private int tutorialSpacePressCount;
    private bool isMusicMuted;

    void Start()
    {
        audio = GetComponent<AudioSource>();
        levelManager = GetComponent<LevelManager>();
        points = PlayerPrefs.GetInt(PointsKey, 0);
        extraLifeUpgradeLevel = PlayerPrefs.GetInt(ExtraLifeUpgradeLevelKey, 0);
        isMusicMuted = PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;
        EnsureLivesUi();
        RestoreExtraLives();
        RefreshShopUI();
        InitializeTutorial();
        InitializeMusicToggle();
        Time.timeScale = 1f;
    }

    void OnEnable()
    {
#if InterstitialAdv_yg
        YG2.onCloseInterAdv += HandleInterstitialClosed;
        YG2.onErrorInterAdv += HandleInterstitialClosed;
#endif
    }

    void OnDisable()
    {
#if InterstitialAdv_yg
        YG2.onCloseInterAdv -= HandleInterstitialClosed;
        YG2.onErrorInterAdv -= HandleInterstitialClosed;
#endif
    }

    void Update()
    {
        ClearUiSelection();

        if (YourText != null)
        {
            YourText.text = count.ToString();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleTutorialSpacePress();

            if (Menu.activeSelf)
            {
                start();
            }
            else if (GameOver.activeSelf)
            {
                reset();
            }
        }

        if (IsGameplayRunning())
        {
            pointTimer += Time.deltaTime;

            while (pointTimer >= 3f)
            {
                pointTimer -= 3f;
                points++;
                PlayerPrefs.SetInt(PointsKey, points);
                RefreshShopUI();
            }
        }

        UpdateLifeHeartVisibility();
    }

    public void start()
    {
        RestoreExtraLives();
        pointTimer = 0f;

        if (Player != null)
        {
            Rigidbody2D rb = Player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 1.0f;
            }
        }

        if (Game != null)
        {
            Game.SetActive(true);
        }

        if (Menu != null)
        {
            Menu.SetActive(false);
        }

        Debug.Log("[Commutator] Game started by Space.");

        if (levelManager != null)
        {
            levelManager.StartCurrentLevel();
        }

        Time.timeScale = 1f;
    }

    public void reset()
    {
        Debug.Log("[Commutator] Reset scene.");

#if InterstitialAdv_yg
        if (pendingResetAfterAd)
        {
            return;
        }

        if (!YG2.nowAdsShow && YG2.isTimerAdvCompleted)
        {
            pendingResetAfterAd = true;
            cachedAudioVolume = AudioListener.volume;
            AudioListener.volume = 0f;
            Debug.Log("[Commutator] Showing interstitial ad on reset.");
            YG2.InterstitialAdvShow();
            return;
        }

        Debug.Log("[Commutator] Interstitial ad is not ready. Resetting immediately.");
#endif

        CompleteReset();
    }

    private void CompleteReset()
    {
        pendingResetAfterAd = false;
        AudioListener.volume = cachedAudioVolume;
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void levelUp()
    {
        if (audio != null && levelup != null)
        {
            audio.PlayOneShot(levelup);
        }
    }

    public void BuyExtraLifeUpgrade()
    {
        if (extraLifeUpgradeLevel >= 2)
        {
            RefreshShopUI();
            return;
        }

        int nextLevel = extraLifeUpgradeLevel + 1;
        int cost = GetUpgradeCost(nextLevel);

        if (points < cost)
        {
            RefreshShopUI();
            return;
        }

        points -= cost;
        extraLifeUpgradeLevel = nextLevel;
        PlayerPrefs.SetInt(PointsKey, points);
        PlayerPrefs.SetInt(ExtraLifeUpgradeLevelKey, extraLifeUpgradeLevel);
        PlayerPrefs.Save();

        Debug.Log("[Commutator] Bought extra life upgrade level: " + extraLifeUpgradeLevel);
        RestoreExtraLives();
        RefreshShopUI();
    }

    public bool TryUseExtraLife()
    {
        if (livesRemaining <= 1)
        {
            return false;
        }

        livesRemaining--;
        Debug.Log("[Commutator] Life used. Remaining: " + livesRemaining);
        RefreshLifeHearts();
        return true;
    }

    public void RestoreExtraLives()
    {
        livesRemaining = GetTotalLivesForUpgradeLevel(extraLifeUpgradeLevel);
        Debug.Log("[Commutator] Lives restored. Current: " + livesRemaining);
        RefreshLifeHearts();
    }

    public void ReloadCurrentLevel()
    {
        if (levelManager != null)
        {
            levelManager.ReloadCurrentLevel();
        }
    }

    private bool IsGameplayRunning()
    {
        return Game != null
            && Game.activeSelf
            && (Menu == null || !Menu.activeSelf)
            && (GameOver == null || !GameOver.activeSelf)
            && Time.timeScale > 0f;
    }

    private void RefreshShopUI()
    {
        if (shopPointsText != null)
        {
            shopPointsText.text = points.ToString();
        }

        if (upgradeInfoText != null)
        {
            if (extraLifeUpgradeLevel >= 2)
            {
                upgradeInfoText.text = "---";
            }
            else
            {
                int nextLevel = extraLifeUpgradeLevel + 1;
                int cost = GetUpgradeCost(nextLevel);
                int bonusLives = GetExtraLivesForUpgradeLevel(nextLevel);
                upgradeInfoText.text = cost.ToString();
            }
        }

        RefreshLifeHearts();
    }

    private int GetUpgradeCost(int level)
    {
        if (level == 1)
        {
            return 10;
        }

        if (level == 2)
        {
            return 25;
        }

        return int.MaxValue;
    }

    private int GetExtraLivesForUpgradeLevel(int level)
    {
        if (level <= 0)
        {
            return 0;
        }

        if (level == 1)
        {
            return 1;
        }

        return 2;
    }

    private int GetTotalLivesForUpgradeLevel(int level)
    {
        return 1 + GetExtraLivesForUpgradeLevel(level);
    }

    private void EnsureLivesUi()
    {
        if (generatedHeartSprite == null)
        {
            generatedHeartSprite = CreateHeartSprite();
        }

        if (livesContainer != null)
        {
            return;
        }

        RectTransform parent = livesUiParent;
        if (parent == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                parent = canvas.GetComponent<RectTransform>();
            }
        }

        if (parent == null)
        {
            return;
        }

        GameObject containerObject = new GameObject("LivesUI", typeof(RectTransform));
        livesContainer = containerObject.GetComponent<RectTransform>();
        livesContainer.SetParent(parent, false);
        livesContainer.anchorMin = new Vector2(0f, 1f);
        livesContainer.anchorMax = new Vector2(0f, 1f);
        livesContainer.pivot = new Vector2(0f, 1f);
        livesContainer.anchoredPosition = new Vector2(22f, -22f);
        livesContainer.sizeDelta = new Vector2(420f, 96f);
    }

    private void InitializeMusicToggle()
    {
        ResolveMusicSource();

        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(HandleMusicToggleChanged);
            musicToggle.isOn = !isMusicMuted;
            musicToggle.onValueChanged.AddListener(HandleMusicToggleChanged);
        }

        ApplyMusicState();
    }

    private void ResolveMusicSource()
    {
        if (musicSource != null)
        {
            return;
        }

        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        for (int i = 0; i < allAudioSources.Length; i++)
        {
            AudioSource candidate = allAudioSources[i];
            if (candidate == null || candidate == audio)
            {
                continue;
            }

            if (candidate.loop)
            {
                musicSource = candidate;
                return;
            }
        }
    }

    private void HandleMusicToggleChanged(bool isOn)
    {
        isMusicMuted = !isOn;
        PlayerPrefs.SetInt(MusicMutedKey, isMusicMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicState();
    }

    private void ApplyMusicState()
    {
        ResolveMusicSource();

        if (musicSource != null)
        {
            musicSource.mute = isMusicMuted;
        }
    }

    private void InitializeTutorial()
    {
        isTutorialActive = PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 0;

        if (!isTutorialActive || tutorialText == null)
        {
            SetTutorialVisible(false);
            return;
        }
        tutorialSpacePressCount = 0;
        SetTutorialVisible(true);

        if (tutorialPulseRoutine != null)
        {
            StopCoroutine(tutorialPulseRoutine);
        }

        tutorialPulseRoutine = StartCoroutine(PulseTutorialText());
    }

    private void HandleTutorialSpacePress()
    {
        if (!isTutorialActive)
        {
            return;
        }

        tutorialSpacePressCount++;

        if (tutorialSpacePressCount < TutorialRequiredSpacePresses)
        {
            return;
        }

        CompleteTutorial();
    }

    private IEnumerator PulseTutorialText()
    {
        while (isTutorialActive)
        {
            SetTutorialVisible(true);
            yield return new WaitForSecondsRealtime(tutorialPulseInterval);
            SetTutorialVisible(false);
            yield return new WaitForSecondsRealtime(tutorialPulseInterval);
        }
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;
        PlayerPrefs.SetInt(TutorialCompletedKey, 1);
        PlayerPrefs.Save();

        if (tutorialPulseRoutine != null)
        {
            StopCoroutine(tutorialPulseRoutine);
            tutorialPulseRoutine = null;
        }

        SetTutorialVisible(false);
    }

    private void SetTutorialVisible(bool isVisible)
    {
        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(isVisible);
        }
    }

    private void RefreshLifeHearts()
    {
        EnsureLivesUi();

        if (livesContainer == null)
        {
            return;
        }

        for (int i = lifeHeartImages.Count - 1; i >= 0; i--)
        {
            if (lifeHeartImages[i] == null)
            {
                lifeHeartImages.RemoveAt(i);
            }
        }

        while (lifeHeartImages.Count > livesRemaining)
        {
            Image image = lifeHeartImages[lifeHeartImages.Count - 1];
            lifeHeartImages.RemoveAt(lifeHeartImages.Count - 1);

            if (image != null)
            {
                Destroy(image.gameObject);
            }
        }

        while (lifeHeartImages.Count < livesRemaining)
        {
            lifeHeartImages.Add(CreateHeartImage(lifeHeartImages.Count));
        }

        for (int i = 0; i < lifeHeartImages.Count; i++)
        {
            if (lifeHeartImages[i] == null)
            {
                continue;
            }

            RectTransform rect = lifeHeartImages[i].rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(i * lifeHeartSpacing, 0f);
            rect.sizeDelta = lifeHeartSize;
            lifeHeartImages[i].enabled = IsGameplayRunning() && livesRemaining > 0;
        }
    }

    private void UpdateLifeHeartVisibility()
    {
        bool shouldShow = IsGameplayRunning() && livesRemaining > 0;

        for (int i = 0; i < lifeHeartImages.Count; i++)
        {
            if (lifeHeartImages[i] != null)
            {
                lifeHeartImages[i].enabled = shouldShow;
            }
        }
    }

    private Image CreateHeartImage(int index)
    {
        GameObject heartObject = new GameObject("LifeHeart_" + index, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = heartObject.GetComponent<RectTransform>();
        rect.SetParent(livesContainer, false);

        Image image = heartObject.GetComponent<Image>();
        image.sprite = generatedHeartSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private Sprite CreateHeartSprite()
    {
        string[] pattern =
        {
            "..XX.XX..",
            ".XXXXXXXX.",
            "XXXXXXXXXX",
            "XXXXXXXXXX",
            ".XXXXXXXX.",
            "..XXXXXX..",
            "...XXXX...",
            "....XX...."
        };

        int width = pattern[0].Length;
        int height = pattern.Length;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color dark = new Color32(92, 33, 140, 255);
        Color mid = new Color32(164, 73, 255, 255);
        Color light = new Color32(224, 170, 255, 255);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char pixel = pattern[height - 1 - y][x];
                if (pixel != 'X')
                {
                    texture.SetPixel(x, y, clear);
                    continue;
                }

                bool edge = x == 0 || x == width - 1 || y == 0 || y == height - 1
                    || pattern[height - 1 - y][Mathf.Max(0, x - 1)] == '.'
                    || pattern[height - 1 - y][Mathf.Min(width - 1, x + 1)] == '.';

                if (edge)
                {
                    texture.SetPixel(x, y, dark);
                }
                else if (y > height / 2)
                {
                    texture.SetPixel(x, y, light);
                }
                else
                {
                    texture.SetPixel(x, y, mid);
                }
            }
        }

        texture.Apply();
        texture.name = "GeneratedLifeHeart";

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            8f
        );
    }

    private void OnDestroy()
    {
        if (tutorialPulseRoutine != null)
        {
            StopCoroutine(tutorialPulseRoutine);
            tutorialPulseRoutine = null;
        }

        AudioListener.volume = cachedAudioVolume;

        if (generatedHeartSprite != null)
        {
            Destroy(generatedHeartSprite.texture);
            Destroy(generatedHeartSprite);
        }
    }

    private void HandleInterstitialClosed()
    {
        AudioListener.volume = cachedAudioVolume;

        if (!pendingResetAfterAd)
        {
            return;
        }

        CompleteReset();
    }

    private void ClearUiSelection()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        if (EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
