using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Target : MonoBehaviour
{
    [Header("Target Health")]
    public float maxHealth = 50f;
    private float health = 100;

    [Header("Damage Settings")]
    public float minImpactForDamage = 2f; // ignore tiny taps
    public float impulseToDamage = 1f;    // scales impulse -> damage

    [Header("Optional")]
    public int scoreValue = 100;          // points awarded when destroyed
    public GameObject destroyVFX;         // optional particle effect prefab

    void Start()
    {
        health = maxHealth;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Compute collision impulse magnitude (sum of all contact impulses)
        Vector3 totalImpulse = Vector3.zero;
        foreach (ContactPoint cp in collision.contacts) { /* no-op, we use collision.impulse */ }

        // Unity provides collision.impulse (physics engine)
        float impulseMag = collision.impulse.magnitude;

        if (impulseMag < minImpactForDamage) return;

        // Convert impulse to damage
        float damage = impulseMag * impulseToDamage;
        health -= damage;

        // Optional: small debug
        // Debug.Log($"{gameObject.name} got hit. Impulse: {impulseMag:F1}, damage: {damage:F1}, health: {health:F1}");

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (destroyVFX != null)
        {
            Instantiate(destroyVFX, transform.position, Quaternion.identity);
        }

        // Add to score
        ScoreManager.Instance?.AddScore(scoreValue);

        Destroy(gameObject);
    }
}
