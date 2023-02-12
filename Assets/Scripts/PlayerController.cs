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
    private Vector3 inputDirection;
    private Rigidbody2D rb;

    [SerializeField] Slider hungerBar;
    [SerializeField] TextMeshProUGUI hungerPercent;
    [SerializeField] float hungerSpeed = 1.0f;
    [SerializeField] AudioSource eatingSound;
    private float currentHunger;
    private float maxHunger = 100f;
    
    /* Click to move
    Vector2 mousePosition;
    */
    private bool isMousePressed;
    private bool onCooldown = false;
    private float holdLength = 0.0f;
    [SerializeField] float holdMax = 2.0f;
    [SerializeField] float lungeSpeed = 2.0f;
    [SerializeField] float lungeCost = 2.0f;
    [SerializeField] Slider lungeMeter;
    [SerializeField] GameObject lungeUI;
    [SerializeField] AudioSource lungeSound;

    [SerializeField] float maxSize = 1.5f;
    [SerializeField] float foodValue = 10.0f;

    private int health;
    private float damageTimer = 0;
    [SerializeField] AudioSource damageSound;
    [SerializeField] int maxHealth = 100;
    [SerializeField] float damageImmunity = .5f;
    [SerializeField] Slider healthBar;
    [SerializeField] TextMeshProUGUI healthDisplay;
    [SerializeField] AudioSource deathSound;

    [SerializeField] int attackDamage = 25;
    private bool isAttacking = false;
    private float attackTimer;

    [SerializeField] int killsToWin = 2;
    public int kills;

    public GameObject gameOverScreen;
    public GameObject victoryScreen;

    void Start() 
    {
        rb = this.GetComponent<Rigidbody2D>();
        currentHunger = maxHunger;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        /* Click to move
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        */
        if (kills >= killsToWin) victoryScreen.SetActive(true);

        damageTimer -= Time.deltaTime;

        isMousePressed = Input.GetMouseButton(0); // If mouse button 0 is held down or not

        if (Input.GetMouseButtonUp(0)) { // When stop holding down mouse button
            rb.AddForce(lookDirection * holdLength * lungeSpeed, ForceMode2D.Impulse); // Lunge force
            currentHunger -= lungeCost * holdLength; // Lose hunger based on how long lunge was charged for
            isAttacking = true; // Activate ability to deal damage
            attackTimer = holdLength; // Attack mode duration based on how long the lunge was charged for
            lungeSound.Play(); // Play sound
            onCooldown = true;
        }

        if (onCooldown) {
            holdLength = Mathf.Max(holdLength - Time.deltaTime, 0); // Cooldown lunge
            if (holdLength <= 0) onCooldown = false;
        }

        inputDirection = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); // Vector3 direction based on movement axis input

        if (isMousePressed && !onCooldown) {
            lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position; // Look towards mouse click
            holdLength = Mathf.Min(holdMax, holdLength + Time.deltaTime); // Charge lunge attack up to a max
            lungeUI.SetActive(true); // Show charge meter
        } else if (inputDirection != Vector3.zero) {
            lookDirection = inputDirection; // Look where WASD movement indicates
        }

        if (attackTimer > 0) {
            attackTimer -= Time.deltaTime;
        }
        else if (isAttacking) {
            isAttacking = false; // Disable ability to damage enemies
            lungeUI.SetActive(false); // Deactivate charge meter UI
        }


        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg; // Find the angle to look towards
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward); // Convert angle to rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime); // Slerp from current rotation to new rotation

        currentHunger = Mathf.Max(currentHunger - hungerSpeed * Time.deltaTime, 0); // Reduce hunger over time based on hungerSpeed to a minimum of 0
        hungerPercent.text = Mathf.CeilToInt(currentHunger) + "%";
        hungerBar.value = currentHunger; // Update hunger bar value

        lungeMeter.value = holdLength; // Update lunge meter value

        if (health <= 0 || currentHunger <= 0) Die();
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
            rb.AddForce(inputDirection * moveSpeed); // Move only when mouse isn't pressed
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Food")) {
            Destroy(other.gameObject); // Eat the food
            EatFood();
        } else if (other.CompareTag("Enemy")) {
            if (isAttacking) {
                DamageEnemy(other.gameObject);
                damageSound.Play();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        Vector2 bounceDir = -((Vector2)collision.transform.position - rb.position).normalized;
        rb.AddForce(bounceDir, ForceMode2D.Impulse); // Bounce away from object
    }

    void DamageEnemy(GameObject enemy) {
        enemy.GetComponentInParent<EnemyAI>().TakeDamage(attackDamage);
        
    }

    void EatFood() {
        currentHunger = currentHunger + foodValue; // Increment hunger value
        if (currentHunger > maxHunger) { // If hunger value goes above max
            Grow(currentHunger - maxHunger); // Grow based on how much above max
            currentHunger = maxHunger; // Reset hunger to max
        }
        health = Mathf.Min(health + (int)foodValue, maxHealth);
        healthBar.value = health;
        healthDisplay.text = "" + health;

        eatingSound.Play();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Enemy")) {
            Vector2 bounceDir = -((Vector2)collision.transform.position - rb.position).normalized;
            rb.AddForce(bounceDir * 2, ForceMode2D.Impulse); // Bounce away from object
            if (isAttacking) {
                DamageEnemy(collision.gameObject);
                damageSound.Play();
            }
        }
    }

    private void Grow(float excess) {
        Vector3 growth = new Vector3(excess / 50.0f, excess / 50.0f); // Growth amount based on how much excess food was eaten
        if(this.transform.localScale.x < maxSize) {this.transform.localScale += growth;} // Grow size if below max
        if(this.transform.localScale.x > maxSize) {this.transform.localScale = new Vector3(maxSize, maxSize);} // If overshot max size, reset to max size

        int healthDiff = maxHealth;
        maxHealth = 100 + (int)(transform.localScale.x * 100);
        healthBar.maxValue = maxHealth;
        healthDiff -= maxHealth;

        health += Mathf.Abs(healthDiff);
        healthBar.value = health;

        attackDamage = 25 + (int)(transform.localScale.x * 10);
    }

    public void TakeDamage(int amount) {
        if (damageTimer <= 0) {
            health = Mathf.Max(health - amount, 0);
            healthBar.value = health;
            healthDisplay.text = "" + health;

            damageTimer = damageImmunity;
        }
    }

    void Die() {
        Destroy(this);
        deathSound.Play();
        gameOverScreen.SetActive(true);
    }
}
