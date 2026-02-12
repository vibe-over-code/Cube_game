using UnityEngine;
using System.Collections;

public class Geometrydashcontroller : MonoBehaviour
{
    public float jumpForce = 10f;
    public float speed = 10f;

    private bool isJumping = false;
    private Rigidbody2D rb;
    public ParticleSystem fall;
    public ParticleSystem explosion;  // Взрыв

    private Commutator com;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        com = GameObject.FindGameObjectWithTag("GameController").GetComponent<Commutator>();
    }

    void Update()
    {
        if (com.Game.activeSelf)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);

            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                Jump();
            }
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void Jump()
    {
        isJumping = true;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null)
        {
            fall.Play();
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        fall.Stop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Deadly"))
        {
            StartCoroutine(DeathSequence(collision.gameObject));
        }
    }

    private IEnumerator DeathSequence(GameObject deadlyObject)
    {
        // Запуск взрыва в позиции игрока
        if (explosion != null)
        {
            explosion.transform.position = transform.position;
            explosion.Play();
        }

        // Удаление всех платформ
        GameObject[] floors = GameObject.FindGameObjectsWithTag("Ground");
        foreach (GameObject floor in floors)
        {
            Destroy(floor);
        }

        // Удаление врага (объекта столкновения)
        Destroy(deadlyObject);

        // Останавливаем движение и прыжки
        rb.velocity = Vector2.zero;
        enabled = false;

        // Ждём 2 секунды, чтобы увидеть взрыв
        yield return new WaitForSecondsRealtime(2f);

        // Показываем GameOver и останавливаем игру
        com.GameOver.SetActive(true);
        Time.timeScale = 0f;
    }
}
