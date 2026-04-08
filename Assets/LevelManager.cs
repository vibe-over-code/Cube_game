using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public List<GameObject> levels;
    public TextMeshProUGUI levelCompleteText;

    [Header("Settings")]
    public Vector3 levelOffsetFromPlayer = Vector3.zero;
    public float transitionDelay = 0.1f;
    public float levelCompleteTextDuration = 1f;
    public string levelCompleteMessage = "Level Complete!";

#if UNITY_EDITOR
    [Header("Debug (Unity Editor Only)")]
    public bool useManualLevelSelection;
    [Min(0)] public int manualLevelIndex;
#endif

    private int currentLevelIndex;
    private bool isTransitioning;
    private const string SAVE_KEY = "PlayerProgress";

    void Awake()
    {
        DeactivateAllLevels();
        HideLevelCompleteText();
        currentLevelIndex = PlayerPrefs.GetInt(SAVE_KEY, 0);

        if (levels == null || levels.Count == 0)
        {
            currentLevelIndex = 0;
            Debug.Log("[LevelManager] Levels list is empty. Nothing to load.");
            return;
        }

#if UNITY_EDITOR
        if (useManualLevelSelection)
        {
            currentLevelIndex = Mathf.Clamp(manualLevelIndex, 0, levels.Count - 1);
            Debug.Log("[LevelManager] Manual debug level selected: " + currentLevelIndex);
            return;
        }
#endif

        if (currentLevelIndex >= levels.Count)
        {
            currentLevelIndex = 0;
        }

        Debug.Log("[LevelManager] Loaded progress index: " + currentLevelIndex);
    }

    private void DeactivateAllLevels()
    {
        if (levels == null)
        {
            return;
        }

        foreach (GameObject lvl in levels)
        {
            if (lvl != null)
            {
                lvl.SetActive(false);
            }
        }
    }

    public void StartCurrentLevel()
    {
        if (isTransitioning)
        {
            Debug.Log("[LevelManager] StartCurrentLevel skipped because transition is already running.");
            return;
        }

        if (levels == null || levels.Count == 0)
        {
            Debug.Log("[LevelManager] StartCurrentLevel skipped because levels are not assigned.");
            return;
        }

        Debug.Log("[LevelManager] Starting level index: " + currentLevelIndex);
        StartCoroutine(LoadLevelSequence(currentLevelIndex));
    }

    public void ReloadCurrentLevel()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.Log("[LevelManager] ReloadCurrentLevel skipped because levels are not assigned.");
            return;
        }

        if (isTransitioning)
        {
            Debug.Log("[LevelManager] ReloadCurrentLevel skipped because transition is already running.");
            return;
        }

        Debug.Log("[LevelManager] Reloading current level index: " + currentLevelIndex);
        StartCoroutine(LoadLevelSequence(currentLevelIndex));
    }

    public void FinishLevel()
    {
        if (isTransitioning)
        {
            Debug.Log("[LevelManager] FinishLevel skipped because transition is already running.");
            return;
        }

        if (levels == null || levels.Count == 0)
        {
            Debug.Log("[LevelManager] FinishLevel skipped because levels are not assigned.");
            return;
        }

#if UNITY_EDITOR
        if (useManualLevelSelection)
        {
            Debug.Log("[LevelManager] FinishLevel ignored because manual debug level selection is enabled.");
            return;
        }
#endif

        int finishedLevelIndex = currentLevelIndex;
        currentLevelIndex++;

        if (currentLevelIndex >= levels.Count)
        {
            currentLevelIndex = 0;
        }

        PlayerPrefs.SetInt(SAVE_KEY, currentLevelIndex);
        PlayerPrefs.Save();

        Debug.Log("[LevelManager] Level " + finishedLevelIndex + " finished. Saved next level index: " + currentLevelIndex);
        StartCoroutine(FinishAndLoadNextLevel(currentLevelIndex));
    }

    private IEnumerator LoadLevelSequence(int index)
    {
        isTransitioning = true;
        Debug.Log("[LevelManager] Loading level index: " + index);

        if (player != null)
        {
            Geometrydashcontroller controller = player.GetComponent<Geometrydashcontroller>();
            if (controller != null)
            {
                controller.RestoreTemporarilyHiddenKillers();
            }
        }

        DeactivateAllLevels();

        yield return new WaitForSecondsRealtime(transitionDelay);

        if (levels[index] != null)
        {
            if (player != null)
            {
                levels[index].transform.position = player.transform.position + levelOffsetFromPlayer;
            }

            levels[index].SetActive(true);
            Debug.Log("[LevelManager] Activated level object: " + levels[index].name);
        }
        else
        {
            Debug.LogWarning("[LevelManager] Level at index " + index + " is null.");
        }

        isTransitioning = false;
    }

    private IEnumerator FinishAndLoadNextLevel(int nextIndex)
    {
        isTransitioning = true;

        Commutator com = GetComponent<Commutator>();
        if (com != null)
        {
            com.RestoreExtraLives();
        }

        if (levelCompleteText != null)
        {
            levelCompleteText.text = levelCompleteMessage;
            levelCompleteText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(levelCompleteTextDuration);

        HideLevelCompleteText();

        yield return StartCoroutine(LoadLevelSequence(nextIndex));
    }

    private void HideLevelCompleteText()
    {
        if (levelCompleteText != null)
        {
            levelCompleteText.gameObject.SetActive(false);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/Clear Saved Progress")]
    private void ClearSavedProgress()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[LevelManager] Saved progress cleared.");
    }
#endif
}
