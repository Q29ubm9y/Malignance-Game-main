using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;

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
    private float damageTimer = 0;
    public Slider healthBar;
    public int maxHealth = 100;
    public int attackDamage;
    public float damageImmunity = .5f;
    public float foodValue = 10.0f;
    public AudioSource eatingSound;
    public AudioSource deathSound;

    public float viewRadius = 3;
    /*[Range(0, 360)]
    public float viewAngle = 90;

    
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();*/

    public Transform player;

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
        //StartCoroutine("FindTargetsWithDelay", .2f);
    }

    /*IEnumerator FindTargetsWithDelay(float delay) {
        while (true) {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets() {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(this.transform.position, viewRadius, targetMask);
        Debug.Log(targetsInViewRadius.Length + " targets spotted.");
        for (int i = 0; i < targetsInViewRadius.Length; i++) {
            Transform foundTarget = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (foundTarget.position - transform.position).normalized;

            if (Vector3.Angle(transform.up, dirToTarget) < viewAngle / 2) {
                float distToTarget = Vector3.Distance(transform.position, foundTarget.position);

                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask)) {
                    visibleTargets.Add(foundTarget);
                }
            }
        }
        foreach(Transform t in visibleTargets) {
            if (t.CompareTag("Player")) {
                target = t;
            }
        }
        visibleTarget = visibleTargets.Count > 0 ? visibleTargets[0] : null;
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
        if (!angleIsGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }*/

    private void Update() {
        damageTimer -= Time.deltaTime;

        if(Vector3.Distance(player.position, transform.position) < viewRadius) {
            target = player;
        } else {
            target = patrolPoints[randomSpot];
        }

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

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponentInParent<PlayerController>().TakeDamage(attackDamage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Food")) {
            Destroy(other.gameObject); // Eat the food
            EatFood();
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

    void EatFood() {
        health = Mathf.Min(health + (int)foodValue, maxHealth);
        healthBar.value = health;
        eatingSound.Play();
    }

    public void TakeDamage(int amount) {
        if (damageTimer <= 0) {
            health = Mathf.Max(health - amount, 0);
            healthBar.value = health;

            damageTimer = damageImmunity;
        }
    }

    void Die() {
        player.GetComponent<PlayerController>().kills++;
        deathSound.Play();
        Destroy(this);
    }
}
