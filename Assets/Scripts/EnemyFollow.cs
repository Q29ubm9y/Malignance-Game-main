using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public float moveSpeed = 1.25f;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] VisionCone visionCone;
    private Transform target;
    private bool targetInVision = false;

    void Update()
    {
        if (target != null)
        {
            Vector2 targetPos = new Vector2(target.position.x - rb.position.x, target.position.y - rb.position.y);
            Vector3 direction = (target.position - transform.position);
            Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3);
            rb.velocity = new Vector2(targetPos.x, targetPos.y).normalized * moveSpeed;
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
        }

        visionCone.SetOrigin(gameObject.transform.position - visionCone.transform.localPosition);
        Vector3 targetPosition = transform.position + transform.up + new Vector3(0, 0, -10);
        visionCone.SetAimDirection((targetPosition - transform.position).normalized);
    }

    public void SeeTarget(Collider2D other)
    {
        if (!targetInVision && other.gameObject.tag == "Player")
        {
            targetInVision = true;
            target = other.transform;
        }
    }

    public void LoseTarget()
    {
        target = null;
        targetInVision = false;
    }
}
