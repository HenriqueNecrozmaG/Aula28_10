using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DungeonPlayer : NetworkBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private CharacterController controller;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Damage Settings")]
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private float blinkTime;

    [Networked, OnChangedRender(nameof(ApplyColor))]
    public byte TeamColor { get; set; }

    [Networked]
    public int points { get; set; }

    private Animator anim;
    private Vector3 moveInput;
    private Vector3 velocity;
    private bool isGrounded, jumpQueued, canMove;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            TeamColor = (byte)(Runner.IsSharedModeMasterClient ? 0 : 1);
            points = 0;
        }

        ApplyColor();
    }

    void ApplyColor()
    {
        if (meshRenderer == null) return;

        meshRenderer.material.color = TeamColor == 0 ? Color.blue : Color.red;
    }

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
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        DungeonMatch match = DungeonMatch.Instance;

        if (match == null || !match.CanPlay) return;

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
            controller.Move(finalMove * Runner.DeltaTime);

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
        if (hit.collider.tag == "Void" || hit.collider.tag == "Obstacle")
        {
            transform.position = new Vector3(-27f, 10f, 0f);
            StartCoroutine(Blink());
        }

        if (hit.collider.tag == "Flag")
        {
            GetPoint();
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

    public void GetPoint()
    {
        if (!HasStateAuthority) return;

        DungeonMatch match = DungeonMatch.Instance;

        if (match == null || !match.CanPlay) return;

        if (points >= 1) return;

        points += 1;

        Debug.Log("Jogador ganhou um ponto. Pontos: " + points);

        if (points >= 1)
        {
            match.RPC_ReportVictoriousPlayer(Object.StateAuthority);
        }
    }
}
