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
