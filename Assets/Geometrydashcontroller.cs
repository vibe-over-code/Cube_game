using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Geometrydashcontroller : MonoBehaviour
{
    public float jumpForce = 10f;
    public float speed = 10f;
    public float sideDeathNormalThreshold = 0.6f;
    public float deathDuration = 0.45f;
    public SpriteRenderer targetSpriteRenderer;
    public Shader deathDissolveShader;

    private bool isJumping = false;
    private Rigidbody2D rb;
    public ParticleSystem fall;

    private Commutator com;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Material runtimeDeathMaterial;
    private Material originalSharedMaterial;
    private readonly List<GameObject> temporarilyHiddenKillers = new List<GameObject>();
    private static readonly int ProgressId = Shader.PropertyToID("_Progress");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = targetSpriteRenderer != null ? targetSpriteRenderer : GetComponentInChildren<SpriteRenderer>();
        com = GameObject.FindGameObjectWithTag("GameController").GetComponent<Commutator>();

        if (spriteRenderer != null)
        {
            originalSharedMaterial = spriteRenderer.sharedMaterial;
            Shader deathShader = ResolveDeathShader();
            if (deathShader != null)
            {
                runtimeDeathMaterial = new Material(deathShader);
                runtimeDeathMaterial.hideFlags = HideFlags.DontSave;
            }
            else
            {
                Debug.LogWarning("[Geometrydashcontroller] Death dissolve shader is not assigned or was stripped from build.");
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
                StartCoroutine(DeathSequence(collision.gameObject));
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

        RestoreTemporarilyHiddenKillers();

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

        if (com != null && com.TryUseExtraLife())
        {
            yield return StartCoroutine(PlayObjectDissolve(deadlyObject));
            ReviveAfterExtraLife();
            yield break;
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
    }

    private IEnumerator PlayObjectDissolve(GameObject targetObject)
    {
        if (targetObject == null)
        {
            yield return new WaitForSecondsRealtime(deathDuration);
            yield break;
        }

        if (targetObject.GetComponent<nodeath>() != null || targetObject.GetComponentInParent<nodeath>() != null)
        {
            yield return new WaitForSecondsRealtime(deathDuration);
            yield break;
        }

        SpriteRenderer targetRenderer = targetObject.GetComponentInChildren<SpriteRenderer>();
        Collider2D targetCollider = targetObject.GetComponent<Collider2D>();

        if (targetCollider != null)
        {
            targetCollider.enabled = false;
        }

        if (targetRenderer == null)
        {
            targetObject.SetActive(false);
            yield return new WaitForSecondsRealtime(deathDuration);
            yield break;
        }

        Shader deathShader = ResolveDeathShader();
        if (deathShader == null)
        {
            targetObject.SetActive(false);
            yield return new WaitForSecondsRealtime(deathDuration);
            yield break;
        }

        Material dissolveMaterial = new Material(deathShader);
        dissolveMaterial.hideFlags = HideFlags.DontSave;

        Material previousMaterial = targetRenderer.sharedMaterial;
        targetRenderer.material = dissolveMaterial;
        dissolveMaterial.SetFloat(ProgressId, 0f);

        float elapsed = 0f;
        while (elapsed < deathDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / deathDuration);
            dissolveMaterial.SetFloat(ProgressId, progress);
            yield return null;
        }

        dissolveMaterial.SetFloat(ProgressId, 1f);
        targetObject.SetActive(false);
        temporarilyHiddenKillers.Add(targetObject);
        targetRenderer.sharedMaterial = previousMaterial;
        Destroy(dissolveMaterial);
    }

    private void ReviveAfterExtraLife()
    {
        isDead = false;
        isJumping = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sharedMaterial = originalSharedMaterial;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void RestoreTemporarilyHiddenKillers()
    {
        if (temporarilyHiddenKillers.Count == 0)
        {
            return;
        }

        for (int i = 0; i < temporarilyHiddenKillers.Count; i++)
        {
            GameObject hiddenKiller = temporarilyHiddenKillers[i];
            if (hiddenKiller != null)
            {
                hiddenKiller.SetActive(true);
            }
        }

        temporarilyHiddenKillers.Clear();
    }

    private Shader ResolveDeathShader()
    {
        if (deathDissolveShader != null)
        {
            return deathDissolveShader;
        }

        return Shader.Find("Custom/SpritePixelDisintegrate");
    }

    private void OnDestroy()
    {
        if (runtimeDeathMaterial != null)
        {
            Destroy(runtimeDeathMaterial);
        }
    }
}
