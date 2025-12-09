using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Target : MonoBehaviour
{
    [Header("Target Health")]
    private float maxHealth = 10f;
    private float health;

    [Header("Damage Settings")]
    public float minImpactForDamage = 2f; // ignore tinsdsdy taps
    public float impulseToDamage = 1f;    // scales impulse -> damage

    [Header("Optional")]
    public int scoreValue = 100;          // points awarded when destroyed
    public GameObject destroyVFX;         // optional particle effect prefab

    [Header("Sound Effects")]
    public AudioClip directHitSound;      // sound for direct bird hit
    public AudioClip indirectHitSound;    // sound for indirect hit

    private AudioSource audioSource;

    void Start()
    {
        health = maxHealth;
        
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Unity provides collision.impulse (physics engine)
        float impulseMag = collision.impulse.magnitude;

        if (impulseMag < minImpactForDamage) return;

        // Check if direct hit from bird or indirect hit
        bool isDirectHit = IsBirdDirectHit(collision);

        // Convert impulse to damage
        float baseDamage = impulseMag * impulseToDamage;

        // Direct hit from bird = 100% damage, indirect hit = 50% damage
        float damageMultiplier = isDirectHit ? 1.0f : 0.5f;
        float damage = baseDamage * damageMultiplier;

        health -= damage;

        // Debug output
        string hitType = isDirectHit ? "DIRECT HIT" : "indirect hit";
        Debug.Log($"{gameObject.name} got {hitType}! Impulse: {impulseMag:F1}, Damage: {damage:F1}, Health: {health:F1}/{maxHealth}");

        if (health <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Check if collision is a direct hit from a bird
    /// </summary>
    private bool IsBirdDirectHit(Collision collision)
    {
        // Check if the colliding object has BirdLaunch component (it's a bird)
        BirdLaunch bird = collision.gameObject.GetComponent<BirdLaunch>();

        if (bird != null)
        {
            // Direct hit from bird
            return true;
        }

        // Indirect hit (other object, debris, or falling)
        return false;
    }

    // void Die()
    // {
    //     Debug.Log($"{gameObject.name} DESTROYED!");

    //     if (destroyVFX != null)
    //     {
    //         Instantiate(destroyVFX, transform.position, Quaternion.identity);
    //     }

    //     // Add to score
    //     ScoreManager.Instance?.AddScore(scoreValue);
    //     Debug.Log($"Score +{scoreValue}");

    //     Destroy(gameObject);
    // }
    void Die()
    {
        Debug.Log($"{gameObject.name} DESTROYED!");

        if (destroyVFX != null)
        {
            Instantiate(destroyVFX, transform.position, Quaternion.identity);
        }

        // Add to score
        ScoreManager.Instance?.AddScore(scoreValue);
        Debug.Log($"Score +{scoreValue}");

        // Notify manager
        TargetManager.Instance?.TargetDestroyed();

        Destroy(gameObject);
    }

}
