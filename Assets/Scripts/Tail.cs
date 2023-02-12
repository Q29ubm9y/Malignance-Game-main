using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour
{
    [SerializeField] int length;
    [SerializeField] LineRenderer line;
    [SerializeField] Vector3[] segmentPositions;
    private Vector3[] segmentV;

    [SerializeField] Transform targetDir;
    [SerializeField] float targetDist;
    [SerializeField] float smoothSpeed;

    [SerializeField] Transform tailEnd;
    [SerializeField] Transform[] bodyParts;
    [SerializeField] Transform player;

    // Start is called before the first frame update
    void Start()
    {
        line.positionCount = length;
        segmentPositions = new Vector3[length];
        segmentV = new Vector3[length];
    }

    // Update is called once per frame
    void Update()
    {
        segmentPositions[0] = targetDir.position; // Define first segment at starting position

        float targetDistScale = player.localScale.x * 0.66f; // Increase segment distance based on player size

        for(int i = 1; i < segmentPositions.Length; i++) {
            Vector3 targetPosition = segmentPositions[i - 1] + (segmentPositions[i] - segmentPositions[i - 1]).normalized * targetDist * targetDistScale; // Direction from i-1 position to i position
            segmentPositions[i] = Vector3.SmoothDamp(segmentPositions[i], targetPosition, ref segmentV[i], smoothSpeed); // Smooth vector change to target vector
            bodyParts[i - 1].transform.position = segmentPositions[i]; // Body part sprites set to line segment positions
        }
        line.SetPositions(segmentPositions); // Update all segment positions to the lineRenderer

        tailEnd.position = segmentPositions[segmentPositions.Length - 1]; // Put tail sprite at last line segment
    }
}
