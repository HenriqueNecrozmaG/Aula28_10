using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTeste : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Damage Settings")]
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private float blinkTime;

    private CharacterController controller;
    private Animator anim;
    private Vector3 moveInput;
    private Vector3 velocity;
    private bool isGrounded, jumpQueued, canMove;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        canMove = true;
    }
    void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
    }
    void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
    }
    void Update()
    {
        ReadInput();
        UpdateGroundedState();
        HandleJump();
        ApplyGravity();
        ApplyMovement();
        SetAnimations();
    }

    private void ReadInput()
    {
        if (!canMove) return;
        moveInput = moveAction.action.ReadValue<Vector2>().normalized;
        if (jumpAction.action.WasPressedThisFrame())
            jumpQueued = true;
    }

    private void UpdateGroundedState()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }

    private void HandleJump()
    {
        if (!jumpQueued) return;
        jumpQueued = false;
        if (!isGrounded) return;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void ApplyGravity()
    {
        if (!isGrounded || velocity.y > 0f)
            velocity.y += gravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        {
            Vector3 move = new Vector3(moveInput.x, 0, 0);
            Vector3 horizontal = move * moveSpeed;
            Vector3 finalMove = horizontal + Vector3.up * velocity.y;
            controller.Move(finalMove * Time.deltaTime);

            if (horizontal.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 90, 0);
            }
            else if (horizontal.x < 0)
            {
                transform.eulerAngles = new Vector3(0, -90, 0);
            }
        }
    }

    private void SetAnimations()
    {
        anim.SetInteger("pMove", (int)moveInput.x);
        anim.SetBool("pGround", isGrounded);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.tag == "Void" ||  hit.collider.tag == "Obstacle")
        {
            transform.position = new Vector3(-27f, 10f, 0f);
            StartCoroutine(Blink());
        }

        if (hit.collider.tag == "Flag")
        {
            Time.timeScale = 0;
        }
    }

    IEnumerator Blink()
    {
        meshRenderer.enabled = false;
        canMove = false;
        yield return new WaitForSeconds(blinkTime);
        meshRenderer.enabled = true;
        canMove = true;
    }
}
