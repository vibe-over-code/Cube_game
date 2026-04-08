using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Commutator : MonoBehaviour
{
    private const string PointsKey = "PlayerPoints";
    private const string ExtraLifeUpgradeLevelKey = "ExtraLifeUpgradeLevel";

    public float count;

    public TextMeshProUGUI YourText;
    public TextMeshProUGUI shopPointsText;
    public TextMeshProUGUI upgradeInfoText;

    public GameObject GameOver;
    public GameObject Game;
    public GameObject Menu;
    public GameObject Player;

    public AudioClip levelup;
    AudioSource audio;
    private LevelManager levelManager;
    private int points;
    private int extraLifeUpgradeLevel;
    private int extraLivesRemaining;
    private float pointTimer;

    void Start()
    {
        audio = GetComponent<AudioSource>();
        levelManager = GetComponent<LevelManager>();
        points = PlayerPrefs.GetInt(PointsKey, 0);
        extraLifeUpgradeLevel = PlayerPrefs.GetInt(ExtraLifeUpgradeLevelKey, 0);
        extraLivesRemaining = GetExtraLivesForUpgradeLevel(extraLifeUpgradeLevel);
        RefreshShopUI();
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (YourText != null)
        {
            YourText.text = count.ToString();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
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

            while (pointTimer >= 1f)
            {
                pointTimer -= 1f;
                points++;
                PlayerPrefs.SetInt(PointsKey, points);
                RefreshShopUI();
            }
        }
    }

    public void start()
    {
        extraLivesRemaining = GetExtraLivesForUpgradeLevel(extraLifeUpgradeLevel);
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
        extraLivesRemaining = GetExtraLivesForUpgradeLevel(extraLifeUpgradeLevel);

        PlayerPrefs.SetInt(PointsKey, points);
        PlayerPrefs.SetInt(ExtraLifeUpgradeLevelKey, extraLifeUpgradeLevel);
        PlayerPrefs.Save();

        Debug.Log("[Commutator] Bought extra life upgrade level: " + extraLifeUpgradeLevel);
        RefreshShopUI();
    }

    public bool TryUseExtraLife()
    {
        if (extraLivesRemaining <= 0)
        {
            return false;
        }

        extraLivesRemaining--;
        Debug.Log("[Commutator] Extra life used. Remaining: " + extraLivesRemaining);
        RefreshShopUI();
        return true;
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
            shopPointsText.text = "Points: " + points;
        }

        if (upgradeInfoText == null)
        {
            return;
        }

        if (extraLifeUpgradeLevel >= 2)
        {
            upgradeInfoText.text = "Extra Lives Lv.MAX (+2)";
            return;
        }

        int nextLevel = extraLifeUpgradeLevel + 1;
        int cost = GetUpgradeCost(nextLevel);
        int bonusLives = GetExtraLivesForUpgradeLevel(nextLevel);
        upgradeInfoText.text = "Upgrade Lives Lv." + nextLevel + " (+" + bonusLives + ") Cost: " + cost;
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
}
