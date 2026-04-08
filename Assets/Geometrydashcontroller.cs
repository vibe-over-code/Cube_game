using UnityEngine;
using System.Collections;

public class Geometrydashcontroller : MonoBehaviour
{
    public float jumpForce = 10f;
    public float speed = 10f;
    public float sideDeathNormalThreshold = 0.6f;
    public float deathDuration = 0.45f;
    public SpriteRenderer targetSpriteRenderer;

    private bool isJumping = false;
    private Rigidbody2D rb;
    public ParticleSystem fall;

    private Commutator com;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Material runtimeDeathMaterial;
    private static readonly int ProgressId = Shader.PropertyToID("_Progress");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = targetSpriteRenderer != null ? targetSpriteRenderer : GetComponentInChildren<SpriteRenderer>();
        com = GameObject.FindGameObjectWithTag("GameController").GetComponent<Commutator>();

        if (spriteRenderer != null)
        {
            Shader deathShader = Shader.Find("Custom/SpritePixelDisintegrate");
            if (deathShader != null)
            {
                runtimeDeathMaterial = new Material(deathShader);
                runtimeDeathMaterial.hideFlags = HideFlags.DontSave;
            }
        }
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
        if (isDead)
        {
            return;
        }

        if (collision != null)
        {
            fall.Play();
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            bool hitSide = false;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) >= sideDeathNormalThreshold)
                {
                    hitSide = true;
                    break;
                }
            }

            if (hitSide)
            {
                StartCoroutine(DeathSequence(null));
                return;
            }

            isJumping = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        fall.Stop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
        {
            return;
        }

        if (collision.CompareTag("Deadly"))
        {
            StartCoroutine(DeathSequence(collision.gameObject));
        }
    }

    private IEnumerator DeathSequence(GameObject deadlyObject)
    {
        if (isDead)
        {
            yield break;
        }

        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        if (fall != null)
        {
            fall.Stop();
        }

        yield return StartCoroutine(PlayDeathDissolve());

        enabled = false;

        yield return new WaitForSecondsRealtime(2f);

        com.GameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    private IEnumerator PlayDeathDissolve()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        if (runtimeDeathMaterial == null)
        {
            spriteRenderer.enabled = false;
            yield break;
        }

        Material previousMaterial = spriteRenderer.material;
        spriteRenderer.material = runtimeDeathMaterial;
        runtimeDeathMaterial.SetFloat(ProgressId, 0f);

        float elapsed = 0f;
        while (elapsed < deathDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / deathDuration);
            runtimeDeathMaterial.SetFloat(ProgressId, progress);
            yield return null;
        }

        runtimeDeathMaterial.SetFloat(ProgressId, 1f);
        spriteRenderer.enabled = false;

        if (previousMaterial != null && previousMaterial != runtimeDeathMaterial)
        {
            Destroy(previousMaterial);
        }
    }

    private void OnDestroy()
    {
        if (runtimeDeathMaterial != null)
        {
            Destroy(runtimeDeathMaterial);
        }
    }
}
