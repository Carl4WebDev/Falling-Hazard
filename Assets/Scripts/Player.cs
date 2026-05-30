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

    [Header("Teleport")]
    public float teleportDistance = 8f;
    public float teleportCooldown = 2f;
    public float teleportInvincibleTime = 0.15f;

    private float lastTeleportTime = -999f;
    private BoxCollider2D playerCollider;

    [Header("Camera Zoom")]
    public float slowMoZoomOffset = -0.5f;
    public float shockwaveZoomOffset = 1f;
    public float zoomSpeed = 8f;

    private float defaultZoom;
    private Coroutine zoomCoroutine;

    [Header("Invincibility")]
    public float invincibleDuration = 1.5f;
    public float flashInterval = 0.1f;

    private bool isInvincible = false;

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
        defaultZoom = Camera.main.orthographicSize;
        playerCollider = GetComponent<BoxCollider2D>();
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
        if (Input.GetKey(KeyCode.A) && !isSlowing && Time.unscaledTime >= lastSlowEndTime + slowCooldown)
        {
            isSlowing = true;
            slowStartTime = Time.unscaledTime;
            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = 0.02f * slowTimeScale;
            ZoomTo(defaultZoom + slowMoZoomOffset);
            foreach (SpriteRenderer sr in spriteRenderers)
                sr.color = new Color(0.5f, 0.5f, 1f, 1f); // blue tint
        }
        if (isSlowing && (Input.GetKeyUp(KeyCode.A) || Time.unscaledTime >= slowStartTime + 3f))
        {
            isSlowing = false;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            ZoomTo(defaultZoom);
            lastSlowEndTime = Time.unscaledTime;
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = originalColors[i];
        }

        // Shockwave input
        if (Input.GetKeyDown(KeyCode.S) && Time.unscaledTime >= lastShockwaveTime + shockwaveCooldown)
        {
            lastShockwaveTime = Time.unscaledTime;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius);
            Debug.Log("Shockwave hit " + hits.Length + " colliders");
            int destroyed = 0;
            foreach (Collider2D hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Instantiate(explosion, hit.transform.position, Quaternion.identity);
                    enemy.ReturnToPool();
                    destroyed++;
                }
            }
            if (destroyed > 0)
            {
                ScoreManager sm = FindFirstObjectByType<ScoreManager>();
                for (int i = 0; i < destroyed; i++)
                    if (sm != null) sm.AddKill();
            }
            Debug.Log("Shockwave destroyed " + destroyed + " hazards");
            // Visual pulse
            StartCoroutine(ShockwavePulse());
            StartCoroutine(ShakeCamera(0.15f, 0.1f));
            StartCoroutine(ZoomPulse(defaultZoom + shockwaveZoomOffset, defaultZoom, 0.3f));
        }

        // Teleport input
        if (Input.GetKeyDown(KeyCode.D) && Time.unscaledTime >= lastTeleportTime + teleportCooldown)
        {
            lastTeleportTime = Time.unscaledTime;
            int dir = (input != 0) ? (int)Mathf.Sign(input) : (transform.eulerAngles.y == 0 ? 1 : -1);
            float newX = transform.position.x + (dir * teleportDistance);
            // Clamp to screen bounds
            Vector3 camPos = Camera.main.transform.position;
            float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            newX = Mathf.Clamp(newX, camPos.x - halfWidth + 0.5f, camPos.x + halfWidth - 0.5f);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            // Brief invincibility so hazards pass through
            StartCoroutine(TeleportBlink());
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
        // Storing player's input (arrow keys only — A/W/D are used for skills)
        float left = Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f;
        float right = Input.GetKey(KeyCode.RightArrow) ? 1f : 0f;
        input = left + right;

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

    public bool IsInvincible()
    {
        return isInvincible;
    }

    private System.Collections.IEnumerator ShockwavePulse()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.3f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }

    private System.Collections.IEnumerator TeleportBlink()
    {
        // Disable collider so hazards pass through
        playerCollider.enabled = false;
        // Flash transparent
        foreach (SpriteRenderer sr in spriteRenderers)
            sr.color = new Color(1f, 1f, 1f, 0.3f);
        yield return new WaitForSeconds(teleportInvincibleTime);
        playerCollider.enabled = true;
        // Restore colors
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].color = originalColors[i];
    }

    private void ZoomTo(float target)
    {
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomRoutine(target));
    }

    private System.Collections.IEnumerator ZoomRoutine(float target)
    {
        while (Mathf.Abs(Camera.main.orthographicSize - target) > 0.01f)
        {
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, target, zoomSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        Camera.main.orthographicSize = target;
    }

    private System.Collections.IEnumerator ZoomPulse(float peak, float target, float holdTime)
    {
        // Cancel any in-flight zoom lerp
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        // Zoom out to peak instantly
        Camera.main.orthographicSize = peak;
        yield return new WaitForSecondsRealtime(holdTime);
        // Lerp back to target
        ZoomTo(target);
    }

    private System.Collections.IEnumerator ShakeCamera(float intensity, float duration)
    {
        Transform cam = Camera.main.transform;
        Vector3 originalPos = cam.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            cam.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cam.localPosition = originalPos;
    }

    private System.Collections.IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibleDuration)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = new Color(originalColors[i].r, originalColors[i].g, originalColors[i].b, 0.3f);
            yield return new WaitForSeconds(flashInterval);
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = originalColors[i];
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval * 2f;
        }
        isInvincible = false;
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
        scoreManager.OnDamageTaken();
        StartCoroutine(ShakeCamera(0.3f, 0.2f));
        StartCoroutine(InvincibilityFrames());

        if (health <= 0) {
            scoreManager.StopScore();
            losePanel.SetActive(true);
            Destroy(gameObject);
        }
    }

}
