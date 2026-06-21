using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public bool isPlayerOne;
    public float attackDamage = 0.1f;

    private Rigidbody rb;
    private bool isGrounded = true;
    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        Move();
        Attack();
        Jump();
    }

    void Move()
    {
        float move = 0f;

        if (isPlayerOne)
        {
            if (Keyboard.current.aKey.isPressed) move = -1f;
            if (Keyboard.current.dKey.isPressed) move = 1f;
        }
        else
        {
            if (Keyboard.current.leftArrowKey.isPressed) move = -1f;
            if (Keyboard.current.rightArrowKey.isPressed) move = 1f;
        }

        transform.Translate(Vector3.right * move * moveSpeed * Time.deltaTime);
    }

    void Jump()
    {
        if (isPlayerOne && Keyboard.current.wKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
        if (!isPlayerOne && Keyboard.current.upArrowKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void Attack()
    {
        if (isPlayerOne && Keyboard.current.jKey.wasPressedThisFrame)
            DealDamage();
        if (!isPlayerOne && Keyboard.current.numpad1Key.wasPressedThisFrame)
            DealDamage();
    }

    void DealDamage()
    {
        if (isPlayerOne)
            gameManager.DamagePlayer2(attackDamage);
        else
            gameManager.DamagePlayer1(attackDamage);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "Ground")
            isGrounded = true;
    }
}