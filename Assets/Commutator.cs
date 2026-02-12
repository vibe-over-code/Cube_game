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

    void Start()
    {
        audio = GetComponent<AudioSource>();
        Time.timeScale = 1f; // Убедимся, что игра идёт при старте
    }

    void Update()
    {
        YourText.text = count.ToString();
    }

    public void start()
    {
        Player.GetComponent<Rigidbody2D>().gravityScale = 1.0f;
        Game.SetActive(true);
        Menu.SetActive(false);
        Time.timeScale = 1f; // Запускаем игру
    }

    public void reset()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f; // Сбрасываем время при рестарте
    }

    public void levelUp()
    {
        audio.PlayOneShot(levelup);
    }
}
