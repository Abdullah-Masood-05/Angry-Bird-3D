// using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
// public class Target : MonoBehaviour
// {
//     [Header("Target Health")]
//     private float maxHealth = 10f;
//     private float health;

//     [Header("Damage Settings")]
//     public float minImpactForDamage = 2f; // ignore tinsdsdy taps
//     public float impulseToDamage = 1f;    // scales impulse -> damage

//     [Header("Optional")]
//     public int scoreValue = 100;          // points awarded when destroyed
//     public GameObject destroyVFX;         // optional particle effect prefab

//     [Header("Sound Effects")]
//     public AudioClip directHitSound;      // sound for direct bird hit
//     public AudioClip indirectHitSound;    // sound for indirect hit

//     private AudioSource audioSource;

//     void Start()
//     {
//         health = maxHealth;

//         // Get or add AudioSource component
//         audioSource = GetComponent<AudioSource>();
//         if (audioSource == null)
//         {
//             audioSource = gameObject.AddComponent<AudioSource>();
//             audioSource.playOnAwake = false;
//             audioSource.spatialBlend = 1f; // 3D sound
//         }
//     }

//     void OnCollisionEnter(Collision collision)
//     {
//         // Unity provides collision.impulse (physics engine)
//         float impulseMag = collision.impulse.magnitude;

//         if (impulseMag < minImpactForDamage) return;

//         // Check if direct hit from bird or indirect hit
//         bool isDirectHit = IsBirdDirectHit(collision);

//         // Convert impulse to damage
//         float baseDamage = impulseMag * impulseToDamage;

//         // Direct hit from bird = 100% damage, indirect hit = 50% damage
//         float damageMultiplier = isDirectHit ? 1.0f : 0.5f;
//         float damage = baseDamage * damageMultiplier;

//         health -= damage;

//         // Play appropriate sound based on hit type
//         PlayHitSound(isDirectHit);

//         // Debug output
//         string hitType = isDirectHit ? "DIRECT HIT" : "indirect hit";
//         Debug.Log($"{gameObject.name} got {hitType}! Impulse: {impulseMag:F1}, Damage: {damage:F1}, Health: {health:F1}/{maxHealth}");

//         if (health <= 0f)
//         {
//             Die();
//         }
//     }

//     /// <summary>
//     /// Check if collision is a direct hit from a bird
//     /// </summary>
//     private bool IsBirdDirectHit(Collision collision)
//     {
//         // Check if the colliding object has BirdLaunch component (it's a bird)
//         BirdLaunch bird = collision.gameObject.GetComponent<BirdLaunch>();

//         if (bird != null)
//         {
//             // Direct hit from bird
//             return true;
//         }

//         // Indirect hit (other object, debris, or falling)
//         return false;
//     }

//     /// <summary>
//     /// Play sound based on hit type
//     /// </summary>
//     private void PlayHitSound(bool isDirectHit)
//     {
//         if (audioSource == null) return;

//         AudioClip clipToPlay = isDirectHit ? directHitSound : indirectHitSound;

//         if (clipToPlay != null)
//         {
//             audioSource.PlayOneShot(clipToPlay);
//             Debug.Log("Attempting to play sound: " + (isDirectHit ? "DIRECT" : "INDIRECT"));
//         }
//     }

//     // void Die()
//     // {
//     //     Debug.Log($"{gameObject.name} DESTROYED!");

//     //     if (destroyVFX != null)
//     //     {
//     //         Instantiate(destroyVFX, transform.position, Quaternion.identity);
//     //     }

//     //     // Add to score
//     //     ScoreManager.Instance?.AddScore(scoreValue);
//     //     Debug.Log($"Score +{scoreValue}");

//     //     Destroy(gameObject);
//     // }
//     void Die()
//     {
//         Debug.Log($"{gameObject.name} DESTROYED!");

//         if (destroyVFX != null)
//         {
//             Instantiate(destroyVFX, transform.position, Quaternion.identity);
//         }

//         // Add to score
//         ScoreManager.Instance?.AddScore(scoreValue);
//         Debug.Log($"Score +{scoreValue}");

//         // Notify manager
//         TargetManager.Instance?.TargetDestroyed();

//         Destroy(gameObject);
//     }

// }




using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Target : MonoBehaviour
{
    private float maxHealth = 10f;
    private float health;

    public float minImpactForDamage = 2f;
    public float impulseToDamage = 1f;

    public int scoreValue = 100;
    public GameObject destroyVFX;

    public AudioClip directHitSound;
    public AudioClip indirectHitSound;

    private AudioSource audioSource;

    // Cooldown to prevent sound spam
    private float soundCooldown = 0.1f;
    private float nextSoundTime = 0f;

    void Start()
    {
        health = maxHealth;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void OnCollisionEnter(Collision collision)
    {
        ProcessHit(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        ProcessHit(collision);
    }

    private void ProcessHit(Collision collision)
    {
        float impulseMag = collision.impulse.magnitude;
        if (impulseMag < minImpactForDamage) return;

        bool isDirectHit = collision.gameObject.GetComponent<BirdLaunch>() != null;

        float damage = impulseMag * impulseToDamage * (isDirectHit ? 1f : 0.5f);
        health -= damage;

        PlayHitSound(isDirectHit);

        Debug.Log($"{gameObject.name} was hit ({(isDirectHit ? "DIRECT" : "indirect")}). Damage: {damage}, Health: {health}");

        if (health <= 0f)
            Die();
    }

    private void PlayHitSound(bool isDirect)
    {
        if (Time.time < nextSoundTime) return;
        nextSoundTime = Time.time + soundCooldown;

        AudioClip clip = isDirect ? directHitSound : indirectHitSound;

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log($"Playing sound: {(isDirect ? "DIRECT" : "INDIRECT")}");
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} DESTROYED!");

        if (destroyVFX != null)
            Instantiate(destroyVFX, transform.position, Quaternion.identity);

        ScoreManager.Instance?.AddScore(scoreValue);
        TargetManager.Instance?.TargetDestroyed();

        Destroy(gameObject);
    }
}
