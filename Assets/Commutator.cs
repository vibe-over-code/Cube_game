using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Commutator : MonoBehaviour
{
    public float count;

    public TextMeshProUGUI YourText;

    public GameObject GameOver;
    public GameObject Game;
    public GameObject Menu;
    public GameObject Player;

    public AudioClip levelup;
    AudioSource audio;
    private LevelManager levelManager;

    void Start()
    {
        audio = GetComponent<AudioSource>();
        levelManager = GetComponent<LevelManager>();
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
    }

    public void start()
    {
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
}
