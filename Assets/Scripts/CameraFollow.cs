using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float followSpeed = 2f;
    [SerializeField] float xMax = 40;
    [SerializeField] float yMax = 20;
    [SerializeField] float xMin = -5;
    [SerializeField] float yMin = -3;
    [SerializeField] Transform target;

    public float zoomSpeed = 10;
    public float currentSize;
    public float zoomedOut = 3.5f;
    public float zoomedIn;

    void Start() {
        currentSize = GetComponent<Camera>().orthographicSize;
        zoomedIn = currentSize;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x, target.position.y, -10f);
        newPos.x = Mathf.Clamp(newPos.x, xMin, xMax);
        newPos.y = Mathf.Clamp(newPos.y, yMin, yMax);

        transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);

        float xMovement = Mathf.Abs(Input.GetAxis("Horizontal"));
        float yMovement = Mathf.Abs(Input.GetAxis("Vertical"));
        if ((xMovement > 0.25f || yMovement > 0.25f) && !Input.GetMouseButton(0)) {
            currentSize = Mathf.Lerp(currentSize, zoomedOut, Time.deltaTime);
        } else {
            currentSize = Mathf.Lerp(currentSize, zoomedIn, Time.deltaTime);
        }
        GetComponent<Camera>().orthographicSize = currentSize + target.localScale.x;

    }
}
