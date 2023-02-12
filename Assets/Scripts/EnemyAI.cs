using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public Transform target;
    
    public float speed = 200f;
    public float rotationSpeed = 3f;
    public float nextWaypointDistance = 2f;

    private int randomSpot;
    private float currWaitTime;
    public float startWaitTime;
    public Transform[] patrolPoints;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    private int health;
    public int maxHealth;
    public int attackDamage;

    Seeker seeker;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        health = maxHealth;

        currWaitTime = startWaitTime;
        randomSpot = Random.Range(0, patrolPoints.Length);
        target = patrolPoints[randomSpot];

        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    private void Update() {
        if (reachedEndOfPath) {
            if (currWaitTime <= 0) {
                randomSpot = Random.Range(0, patrolPoints.Length);
                target = patrolPoints[randomSpot];
                currWaitTime = startWaitTime;
            } else {
                currWaitTime -= Time.deltaTime;
            }
        }
        if (health <= 0) Die();
    }

    void FixedUpdate()
    {
        if (path == null) {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count) {
            reachedEndOfPath = true;
            return;
        } else {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);
        Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * rotationSpeed);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance) {
            currentWaypoint++;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            collision.GetComponentInParent<PlayerController>().TakeDamage(attackDamage);
        }
    }

    void UpdatePath() {
        if (seeker.IsDone()) {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p) {
        if (!p.error) {
            path = p;
            currentWaypoint = 0;
        }
    }

    public void TakeDamage(int amount) {
        health -= amount;
    }

    void Die() {
        Destroy(this);
    }
}
