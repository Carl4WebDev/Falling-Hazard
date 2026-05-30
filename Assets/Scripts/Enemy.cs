using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float minSpeed;
    public float maxSpeed;

    float speed;

    Player playerScript;

    public int damage;

    public GameObject explosion;

    [HideInInspector] public System.Action<GameObject> returnToPool;

    // Start is called before the first frame update
    void Awake()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        speed = Random.Range(minSpeed, maxSpeed);
    }

    void OnEnable()
    {
        speed = Random.Range(minSpeed, maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
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
            ReturnToPool();
        }

    }

    public void ResetState()
    {
        speed = Random.Range(minSpeed, maxSpeed);
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
}
