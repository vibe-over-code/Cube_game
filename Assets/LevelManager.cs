using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public List<GameObject> levels;

    [Header("Settings")]
    public Vector3 levelOffsetFromPlayer = Vector3.zero;
    public float transitionDelay = 0.1f;

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
        StartCoroutine(LoadLevelSequence(currentLevelIndex));
    }

    private IEnumerator LoadLevelSequence(int index)
    {
        isTransitioning = true;
        Debug.Log("[LevelManager] Loading level index: " + index);

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
