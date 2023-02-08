using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VisionCone : MonoBehaviour
{
    private Vector3 origin;
    float startingAngle = 0f;
    float fov = 90f;
    EnemyFollow parentEnemy = null;

    private void Start()
    {
        parentEnemy = transform.parent.GetComponentInChildren<EnemyFollow>();
        Debug.Log(parentEnemy);
    }

    private void LateUpdate()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        int rayCount = 50;
        float angleIncrease = fov / rayCount;
        float viewDistance = 5f;
        Vector3 offsetPos = transform.parent.position + new Vector3(0, 0f, 0);

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin - offsetPos;
        float angle = startingAngle;
        int vertexIndex = 1;
        int triangleIndex = 0;
        bool seeTarget = false;
        for (int i = 0; i < rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D rayHit = Physics2D.Raycast(origin, GetVectorFromAngle(angle), viewDistance);
            if (rayHit.collider == null)
            {
                vertex = (origin - offsetPos) + GetVectorFromAngle(angle) * viewDistance;
            }
            else
            {
                if (rayHit.collider.gameObject.tag == "Player")
                {
                    seeTarget = true;
                    parentEnemy.SeeTarget(rayHit.collider);
                }
                vertex = rayHit.point - (Vector2)offsetPos;
            }
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        if (!seeTarget)
        {
            parentEnemy.LoseTarget();
        }
    }

    Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    float getAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        return n;
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        startingAngle = getAngleFromVectorFloat(aimDirection) + fov / 2f;
    }
}
