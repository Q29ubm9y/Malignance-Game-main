using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float rotationSpeed;
    private Vector2 lookDirection;
    private Vector3 movementDirection;
    private Rigidbody2D rb;

    [SerializeField] Slider hungerBar;
    [SerializeField] TextMeshProUGUI hungerPercent;
    [SerializeField] float hungerSpeed = 1.0f;
    private float currentHunger;
    private float maxHunger = 100f;
    
    /* Click to move
    Vector2 mousePosition;
    */
    private bool isMousePressed;
    private float holdLength = 0.0f;
    [SerializeField] float holdMax = 2.0f;
    [SerializeField] float lungeStrength = 2.0f;
    [SerializeField] float lungeCost = 2.0f;
    [SerializeField] Slider lungeMeter;

    [SerializeField] float maxSize = 1.5f;
    [SerializeField] float foodValue = 10.0f;



    void Start() 
    {
        rb = this.GetComponent<Rigidbody2D>();
        currentHunger = maxHunger;
    }

    // Update is called once per frame
    void Update()
    {
        /* Click to move
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        */

        isMousePressed = Input.GetMouseButton(0); // If mouse button 0 is held down or not

        if (Input.GetMouseButtonUp(0)) { // When stop holding down mouse button
            rb.AddForce(lookDirection * holdLength * lungeStrength, ForceMode2D.Impulse); // Lunge force
            currentHunger -= lungeCost * holdLength; // Lose hunger based on how long lunge was charged for
            holdLength = 0.0f; // Reset lunge charge
        }
        movementDirection = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); // Vector3 direction based on movement axis input
        if (isMousePressed) {
            lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position; // Look towards mouse click
            holdLength = Mathf.Min(holdMax, holdLength + Time.deltaTime); // Charge lunge attack up to a max
        } else if (movementDirection != Vector3.zero) {
            lookDirection = movementDirection; // Look where WASD movement indicates
        }
        

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg; // Find the angle to look towards
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward); // Convert angle to rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime); // Slerp from current rotation to new rotation

        currentHunger = Mathf.Max(currentHunger - hungerSpeed * Time.deltaTime, 0); // Reduce hunger over time based on hungerSpeed to a minimum of 0
        hungerPercent.text = Mathf.CeilToInt(currentHunger) + "%";
        hungerBar.value = currentHunger; // Update hunger bar value

        lungeMeter.value = holdLength; // Update lunge meter value
    }

    private void FixedUpdate()
    {
        /* Click to move
        Vector2 facingDirection = mousePosition - rb.position;
        float facingAngle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = facingAngle;
        if (isMoving){rb.velocity = new Vector2(facingDirection.x, facingDirection.y).normalized * moveSpeed;}
        else {rb.velocity = new Vector2(0, 0);}
        */

        if(!isMousePressed) {
            rb.AddForce(movementDirection * moveSpeed); // Move only when mouse isn't pressed
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Food")) {
            Destroy(other.gameObject); // Eat the food
            currentHunger = currentHunger + foodValue; // Increment hunger value
            if(currentHunger > maxHunger) { // If hunger value goes above max
                Grow(currentHunger - maxHunger); // Grow based on how much above max
                currentHunger = maxHunger; // Reset hunger to max
            }
        }
    }

    private void Grow(float excess) {
        Vector3 growth = new Vector3(excess / 50.0f, excess / 50.0f); // Growth amount based on how much excess food was eaten
        if(this.transform.localScale.x < maxSize) {this.transform.localScale += growth;} // Grow size if below max
        if(this.transform.localScale.x > maxSize) {this.transform.localScale = new Vector3(maxSize, maxSize);} // If overshot max size, reset to max size
    }
}
