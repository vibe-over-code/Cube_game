using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Находим менеджер и говорим ему, что уровень пройден
            FindObjectOfType<LevelManager>().FinishLevel();
        }
    }
}