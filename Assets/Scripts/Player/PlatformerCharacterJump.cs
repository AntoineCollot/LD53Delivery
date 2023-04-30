using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlatformerCharacterJump : PlayerController
{
    [SerializeField, Range(1, 10)] float smallJumpGravMult = 2;
    [SerializeField, Range(1, 10)] float downwardMovementMult = 3;
    [SerializeField, Range(0, 0.5f)] float jumpBuffer = 0.3f;
    [SerializeField, Range(0, 0.5f)] float coyoteTime = 0.1f;
    [SerializeField, Range(0.5f, 5)] float jumpHeight = 2.2f;
    [SerializeField, Range(0.2f, 2)] float timeToJumpApex = 0.5f;
    [SerializeField, Range(5, 20)] float terminalFallVelocity = 20;
    [SerializeField, Range(0, 0.3f)] float snapToFullJumpTime = 0.15f;
    public int airbornJumpCount { get; private set; }

    bool desireJump;
    bool isPressingJump;
    bool isJumping;
    bool forceFullJump;
    Vector2 velocity;
    float gravityMult;
    float coyoteTimeCounter01;
    float jumpBufferCounter01;
    float jumpSpeed;
    float gravityScale;
    CompositeStateToken freezeGroundDetectionToken = new CompositeStateToken();

    //Snap to full jump
    float lastPressJumpTime;
    bool snapFullJump;

    [Header("Components")]
    Rigidbody2D body;
    GroundCaster ground;

    public bool IsGoingUp => body.velocity.y > 0.01f;
    public bool IsFalling => body.velocity.y < -0.01f;
    public float InitialJumpSpeed => Mathf.Sqrt(-2f * Physics2D.gravity.y * gravityScale * jumpHeight);
    public float TerminalFallVelocity => terminalFallVelocity;
    public float VerticalVelocity => velocity.y;
    public bool IsGrounded => ground.isGrounded;

    public UnityEvent onJump = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        ground = GetComponentInChildren<GroundCaster>();
        ground.onGroundedStateChanged.AddListener(OnGroundedStateChanged);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        inputs.Gameplay.Jump.performed += OnJump;
        inputs.Gameplay.Jump.canceled += OnJump;

        if (PlayerState.Instance != null)
            PlayerState.Instance.freezeGroundDetectionState.Add(freezeGroundDetectionToken);
    }
    protected override void OnDisable()
    {
        base.OnEnable();
        inputs.Gameplay.Jump.performed -= OnJump;
        inputs.Gameplay.Jump.canceled -= OnJump;

        if (PlayerState.Instance != null)
            PlayerState.Instance.freezeGroundDetectionState.Remove(freezeGroundDetectionToken);
    }

    private void Update()
    {
        //Check if we should freeze the inputs
        if (PlayerState.Instance.freezeInputsState.IsOn)
            desireJump = false;

        //Coyote time
        if (isJumping)
        {
            //Max out coyote time when jumping as we can't use it anymore
            coyoteTimeCounter01 = 1;
        }
        else if (!ground.isGrounded)
        {
            coyoteTimeCounter01 += Time.deltaTime / coyoteTime;
        }
        else
        {
            coyoteTimeCounter01 = 0;
        }

        if (desireJump)
        {
            jumpBufferCounter01 += Time.deltaTime / jumpBuffer;
            if (jumpBufferCounter01 >= 1)
            {
                //Cancel the buffer
                desireJump = false;
                jumpBufferCounter01 = 0;
            }
        }

        //Full jump detection
        if (isPressingJump && Time.time > lastPressJumpTime + snapToFullJumpTime)
            snapFullJump = true;
    }

    void FixedUpdate()
    {
        velocity = body.velocity;

        //Compute the gravityscale to get the correct jump duration
        gravityScale = (-2 * jumpHeight) / (timeToJumpApex * timeToJumpApex * Physics2D.gravity.y);

        if (desireJump)
        {
            TryJump();
            body.velocity = velocity;
            return;
        }

        if (IsGoingUp)
        {
            if ((isPressingJump && isJumping) || snapFullJump || forceFullJump)
            {
                gravityMult = 1;
            }
            else
            {
                gravityMult = smallJumpGravMult;
            }
        }
        else if (IsFalling)
        {
            //Clamp falling velocity
            velocity.y = Mathf.Clamp(velocity.y, -terminalFallVelocity, Mathf.Infinity);

            gravityMult = downwardMovementMult;

            isJumping = false;
        }
        else
        {
            gravityMult = 1;
        }

        if (PlayerState.Instance.forceDefaultGravityState.IsOn)
            body.gravityScale = gravityScale;
        else
            body.gravityScale = gravityScale * gravityMult;

        if (PlayerState.Instance.freezePositionState.IsOn)
            velocity.y = 0;

        body.velocity = velocity;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            desireJump = true;
            isPressingJump = true;
            lastPressJumpTime = Time.time;
            //Reset the buffer
            jumpBufferCounter01 = 0;
        }

        if (context.canceled)
        {
            isPressingJump = false;
            isJumping = false;
        }
    }

    public void AddAirbornJump()
    {
        airbornJumpCount++;
    }

    void TryJump()
    {
        if (forceFullJump)
            return;

        if (ground.isGrounded || (coyoteTimeCounter01 > 0 && coyoteTimeCounter01 < 1))
        {
            Jump(0);
        }
        else if(!ground.isGrounded && airbornJumpCount > 0)
        {
            airbornJumpCount--;
            ForceJump(0);
        }
    }

    public void ForceJump(float jumpHeightModifier)
    {
        //Update velocity
        velocity = body.velocity;
        Jump(jumpHeightModifier);
        body.velocity = velocity;

        forceFullJump = true;
        freezeGroundDetectionToken.SetOn(true);
        Invoke("ResetGroundDetection", 0.2f);
    }

    void ResetGroundDetection()
    {
        freezeGroundDetectionToken.SetOn(false);
    }

    void Jump(float jumpHeightModifier)
    {
        jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * gravityScale * (jumpHeight + jumpHeightModifier));

        if (velocity.y > 0f)
        {
            //Make sure we don't add multiple jump speed
            jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0);
        }
        else if (velocity.y < 0)
        {
            //Cancel the falling velocity
            jumpSpeed += Mathf.Abs(body.velocity.y);
        }

        isJumping = true;
        desireJump = false;
        snapFullJump = false;

        velocity.y += jumpSpeed;

        onJump.Invoke();

        //Audio
        SFXManager.PlaySound(GlobalSFX.Jump);
    }

    private void OnGroundedStateChanged(bool isGrounded)
    {
        //landing
        if (isGrounded)
        {
            isJumping = false;
            forceFullJump = false;
            snapFullJump = false;
            airbornJumpCount = 0;
        }
    }
}
