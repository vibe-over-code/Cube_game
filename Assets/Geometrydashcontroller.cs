using UnityEngine;
using System.Collections;

public class Geometrydashcontroller : MonoBehaviour
{
    public float jumpForce = 10f;
    public float speed = 10f;

    private bool isJumping = false;
    private Rigidbody2D rb;
    public ParticleSystem fall;
    public ParticleSystem explosion;

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
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                Jump();
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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
        if (explosion != null)
        {
            explosion.transform.position = transform.position;
            explosion.Play();
        }

        Destroy(deadlyObject);

        rb.linearVelocity = Vector2.zero;
        enabled = false;

        yield return new WaitForSecondsRealtime(2f);

        com.GameOver.SetActive(true);
        Time.timeScale = 0f;
    }
}
