using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour {
    // Coordinates for spawning area
    [SerializeField] float minX = -8.5f;
    [SerializeField] float minY = -5f;
    [SerializeField] float maxX = 29f;
    [SerializeField] float maxY = 14f;

    [SerializeField] int foodMax = 10;
    [SerializeField] float spawnInterval = 5f;
    [SerializeField] GameObject foodPrefab;
    [SerializeField] GameObject foodParent;

    float spawnTimer = 0f;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        spawnTimer += Time.deltaTime;
        if (GameObject.FindGameObjectsWithTag("Food").Length < foodMax) { // Don't spawn food if the amount of food on the board is at max
            if (spawnTimer >= spawnInterval) {
                spawnTimer = 0f;
                SpawnFood(minX, maxX, minY, maxY); // Spawn food at random point
            }
        }
    }

    void SpawnFood(float minX, float maxX, float minY, float maxY) {
        Vector2 spawnPosition;
        Collider2D[] colliders;
        int attempts = 0;

        while (attempts < 10) {
            spawnPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            colliders = Physics2D.OverlapCircleAll(spawnPosition, 0.5f);
            if(colliders.Length == 0) {
                Instantiate(foodPrefab, spawnPosition, Quaternion.identity, foodParent.transform);
                break;
            }
            attempts++;
        }
        Debug.Log(attempts);
    }
}
