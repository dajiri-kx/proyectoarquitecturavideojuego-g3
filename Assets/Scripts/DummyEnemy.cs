using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class DummyEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private Sprite deathSprite;

    private int currentHealth;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (idleSprite != null && sr.sprite == null)
        {
            sr.sprite = idleSprite;
        }
    }

    public void TakeDamage(int amount, Vector2 hitSource)
    {
        if (isDead) return;

        currentHealth -= amount;

        // Knockback
        if (rb != null)
        {
            Vector2 dir = ((Vector2)transform.position - hitSource).normalized;
            dir.y = 0.5f;
            rb.velocity = Vector2.zero;
            rb.AddForce(dir.normalized * 4f, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        if (sr != null && hitSprite != null)
        {
            sr.sprite = hitSprite;
        }
        yield return new WaitForSeconds(0.2f);
        if (!isDead && sr != null && idleSprite != null)
        {
            sr.sprite = idleSprite;
        }
    }

    private void Die()
    {
        isDead = true;
        if (sr != null && deathSprite != null)
        {
            sr.sprite = deathSprite;
        }
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.velocity = Vector2.zero;
    }
}
