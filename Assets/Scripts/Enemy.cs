using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum HazardType { Straight, Zigzag, Fast, Split }

    public float minSpeed;
    public float maxSpeed;

    float speed;

    Player playerScript;

    public int damage;

    public GameObject explosion;

    [HideInInspector] public System.Action<GameObject> returnToPool;

    [HideInInspector] public HazardType hazardType = HazardType.Straight;

    [Header("Zigzag")]
    public float zigzagFrequency = 3f;
    public float zigzagAmplitude = 2f;

    [Header("Fast")]
    public float fastSpeedMultiplier = 2f;

    [Header("Split")]
    public float splitAngle = 30f;

    private Vector2 moveDirection = Vector2.down;
    private float baseX;
    private bool useCoroutineMovement = false;

    // Start is called before the first frame update
    void Awake()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        speed = Random.Range(minSpeed, maxSpeed);
        baseX = transform.position.x;
    }

    void OnEnable()
    {
        speed = Random.Range(minSpeed, maxSpeed);
        moveDirection = Vector2.down;
        baseX = transform.position.x;
        useCoroutineMovement = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (useCoroutineMovement) return;
        switch (hazardType)
        {
            case HazardType.Straight:
                transform.Translate(Vector2.down * speed * Time.deltaTime);
                break;

            case HazardType.Zigzag:
                float newX = baseX + Mathf.Sin(Time.time * zigzagFrequency) * zigzagAmplitude;
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
                transform.Translate(Vector2.down * speed * Time.deltaTime);
                break;

            case HazardType.Fast:
                transform.Translate(Vector2.down * speed * fastSpeedMultiplier * Time.deltaTime);
                break;

            case HazardType.Split:
                transform.Translate(Vector2.down * speed * Time.deltaTime);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D hitObject)
    {

        if(hitObject.tag == "Player") {
            if (playerScript != null && !playerScript.IsInvincible()) {
                playerScript.TakeDamage(damage);
            }
            Instantiate(explosion, transform.position, Quaternion.identity);
            ReturnToPool();
        }

        if (hitObject.tag == "Ground") {
            Instantiate(explosion, transform.position, Quaternion.identity);
            if (hazardType == HazardType.Split)
            {
                SpawnSplitChildren();
            }
            ReturnToPool();
        }

    }

    public void ResetState()
    {
        speed = Random.Range(minSpeed, maxSpeed);
        moveDirection = Vector2.down;
        baseX = transform.position.x;
        useCoroutineMovement = false;
        if (playerScript == null)
        {
            playerScript = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        }
    }

    public void ReturnToPool()
    {
        if (returnToPool != null)
        {
            returnToPool.Invoke(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SpawnSplitChildren()
    {
        float leftAngle = -splitAngle * Mathf.Deg2Rad;
        float rightAngle = splitAngle * Mathf.Deg2Rad;

        Vector2 leftDir = new Vector2(Mathf.Sin(leftAngle), -Mathf.Cos(leftAngle));
        Vector2 rightDir = new Vector2(Mathf.Sin(rightAngle), -Mathf.Cos(rightAngle));

        SpawnSplitChild(leftDir);
        SpawnSplitChild(rightDir);
    }

    private void SpawnSplitChild(Vector2 direction)
    {
        GameObject child = Instantiate(gameObject, transform.position, Quaternion.identity);
        // Strip pooled callback so child uses Destroy on cleanup
        Enemy childEnemy = child.GetComponent<Enemy>();
        childEnemy.returnToPool = null;
        childEnemy.hazardType = HazardType.Straight;
        childEnemy.moveDirection = direction;
        childEnemy.damage = damage;
        childEnemy.speed = speed;
        childEnemy.useCoroutineMovement = true;
        child.transform.localScale = transform.localScale * 0.6f;
        childEnemy.StartCoroutine(childEnemy.MoveInDirection(direction));
    }

    private System.Collections.IEnumerator MoveInDirection(Vector2 direction)
    {
        while (gameObject.activeInHierarchy)
        {
            transform.Translate(direction * speed * Time.deltaTime);
            yield return null;
        }
    }
}
