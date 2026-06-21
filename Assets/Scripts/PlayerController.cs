using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public bool isPlayerOne;
    public float attackDamage = 0.1f;

    [Tooltip("Décalage vertical du point d'émission de poussière par rapport au centre du personnage (négatif = vers les pieds). À ajuster si la poussière apparaît trop haut/bas.")]
    public float dustYOffset = -0.9f;

    private Rigidbody rb;
    private bool isGrounded = true;
    private GameManager gameManager;
    private ParticleSystem dustPS;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindAnyObjectByType<GameManager>();

        if (rb != null)
        {
            // Empêche le personnage de basculer sur le côté lors des collisions
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            Debug.LogWarning($"{name} n'a pas de Rigidbody assigné !");
        }

        CreerEffetPoussiere();
    }

    // Crée un petit système de particules de poussière aux pieds du joueur, entièrement par code
    void CreerEffetPoussiere()
    {
        GameObject dustObj = new GameObject("DustEffect");
        dustObj.transform.SetParent(transform);
        dustObj.transform.localPosition = new Vector3(0f, dustYOffset, 0f);

        dustPS = dustObj.AddComponent<ParticleSystem>();

        var main = dustPS.main;
        main.loop = true;
        main.startLifetime = 0.4f;
        main.startSpeed = 0.6f;
        main.startSize = 0.25f;
        main.startColor = new Color(0.75f, 0.62f, 0.42f, 0.55f);
        main.gravityModifier = 0.3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = dustPS.emission;
        emission.rateOverTime = 0f; // pas de poussière par défaut, activée dynamiquement dans Move()

        var shape = dustPS.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.15f;

        var renderer = dustObj.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader != null)
            renderer.material = new Material(shader) { name = "Mat_Poussiere_" + name };
    }

    void Update()
    {
        if (Keyboard.current == null) return; // évite un crash silencieux si l'Input System n'est pas actif

        HandleJumpInput();
        Attack();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (Keyboard.current == null || rb == null) return;

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

        // On pilote la vélocité physique plutôt que de téléporter le transform :
        // ça permet aux colliders des murs de bloquer correctement le joueur.
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(move * moveSpeed, v.y, v.z);

        // Poussière au sol uniquement quand le joueur marche réellement
        if (dustPS != null)
        {
            var emission = dustPS.emission;
            bool souleverDePoussiere = isGrounded && Mathf.Abs(move) > 0.01f;
            emission.rateOverTime = souleverDePoussiere ? 14f : 0f;
        }
    }

    void HandleJumpInput()
    {
        bool jumpPressed = isPlayerOne
            ? Keyboard.current.wKey.wasPressedThisFrame
            : Keyboard.current.upArrowKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
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
        if (gameManager == null)
        {
            Debug.LogWarning($"{name} : GameManager introuvable dans la scène, l'attaque n'a aucun effet.");
            return;
        }

        if (isPlayerOne)
            gameManager.DamagePlayer2(attackDamage);
        else
            gameManager.DamagePlayer1(attackDamage);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "Ground")
        {
            if (!isGrounded && dustPS != null)
                dustPS.Emit(15); // petite explosion de poussière à l'atterrissage
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.name == "Ground")
            isGrounded = false;
    }
}