using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    public GameObject losePanel;

    public Text healthDisplay;
    public ScoreManager scoreManager;

    public float speed;
    private float input;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private float dashTimeLeft;
    private float lastDashTime = -999f;
    private int dashDirection = 1;

    [Header("Slow Motion")]
    public float slowTimeScale = 0.3f;
    public float slowCooldown = 5f;

    private bool isSlowing = false;
    private float lastSlowEndTime = -999f;
    private float slowStartTime = -999f;
    private Color[] originalColors;
    private SpriteRenderer[] spriteRenderers;

    [Header("Shockwave")]
    public float shockwaveRadius = 5f;
    public float shockwaveCooldown = 3f;
    public GameObject explosion;

    private float lastShockwaveTime = -999f;

    Rigidbody2D rb;
    Animator anim;
    AudioSource source;

    public int health;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
        healthDisplay.text = health.ToString();
    }

    private void Update()
    {
        // Dash input
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && Time.unscaledTime >= lastDashTime + dashCooldown)
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            lastDashTime = Time.unscaledTime;
            dashDirection = (input != 0) ? (int)Mathf.Sign(input) : (transform.eulerAngles.y == 0 ? 1 : -1);
            // TODO: Add dash animation to animator and uncomment:
            // anim.SetTrigger("dash");
        }

        // Slow Motion input (hold)
        if (Input.GetKey(KeyCode.LeftShift) && !isSlowing && Time.unscaledTime >= lastSlowEndTime + slowCooldown)
        {
            isSlowing = true;
            slowStartTime = Time.unscaledTime;
            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = 0.02f * slowTimeScale;
            foreach (SpriteRenderer sr in spriteRenderers)
                sr.color = new Color(0.5f, 0.5f, 1f, 1f); // blue tint
        }
        if (isSlowing && (Input.GetKeyUp(KeyCode.LeftShift) || Time.unscaledTime >= slowStartTime + 3f))
        {
            isSlowing = false;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            lastSlowEndTime = Time.unscaledTime;
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = originalColors[i];
        }

        // Shockwave input
        if (Input.GetKeyDown(KeyCode.E) && Time.unscaledTime >= lastShockwaveTime + shockwaveCooldown)
        {
            lastShockwaveTime = Time.unscaledTime;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius);
            Debug.Log("Shockwave hit " + hits.Length + " colliders");
            int destroyed = 0;
            foreach (Collider2D hit in hits)
            {
                if (hit.GetComponent<Enemy>() != null)
                {
                    Instantiate(explosion, hit.transform.position, Quaternion.identity);
                    Destroy(hit.gameObject);
                    destroyed++;
                }
            }
            Debug.Log("Shockwave destroyed " + destroyed + " hazards");
            // Visual pulse
            StartCoroutine(ShockwavePulse());
        }

        if (input != 0)
        {
            anim.SetBool("isRunning", true);
        }
        else {
            anim.SetBool("isRunning", false);
        }

        if (input > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (input < 0) {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Storing player's input
        input = Input.GetAxisRaw("Horizontal");

        if (isDashing)
        {
            // Dash movement
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            dashTimeLeft -= Time.fixedUnscaledDeltaTime;
            if (dashTimeLeft <= 0f)
            {
                isDashing = false;
            }
        }
        else
        {
            // Normal movement
            rb.linearVelocity = new Vector2(input * speed, rb.linearVelocity.y);
        }

    }

    public bool IsDashing()
    {
        return isDashing;
    }

    private System.Collections.IEnumerator ShockwavePulse()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.3f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }

    public void TakeDamage(int damageAmount) {
        source.Play();
        health -= damageAmount;
        healthDisplay.text = health.ToString();

        if (health <= 0) {
            scoreManager.StopScore();
            losePanel.SetActive(true);
            Destroy(gameObject);
        }
    }

}
