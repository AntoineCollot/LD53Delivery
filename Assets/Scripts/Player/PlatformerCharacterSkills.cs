using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerCharacterSkills : PlayerController
{
    [Header("Dash")]
    public float dashSpeed = 20;
    public float dashDuration = 0.3f;
    CompositeStateToken isDashingToken;

    [Header("UpwardKick")]
    public float upwardKickSpeed = 30;
    [Range(0, 90)] public float upwardKickAngle = 45;
    public float upwardKickDuration = 0.3f;

    [Header("DownwardKick")]
    public float downwardKickSpeed = 30;

    Vector2 translateVelocity;

    [Header("Components")]
    Rigidbody2D body;
    GroundCaster ground;
    PlatformerCharacterMovement movement;


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        body = GetComponent<Rigidbody2D>();
        ground = GetComponentInChildren<GroundCaster>();
    }

    // Start is called before the first frame update
    void Start()
    {
        isDashingToken = new CompositeStateToken();
        PlayerState.Instance.freezeInputsState.Add(isDashingToken);

        movement = GetComponent<PlatformerCharacterMovement>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        inputs.Gameplay.Skill.performed += OnSkill;
    }


    protected override void OnDisable()
    {
        base.OnDisable();

        inputs.Gameplay.Skill.performed -= OnSkill;
    }

    private void FixedUpdate()
    {
        if (isDashingToken.IsOn)
        {
            body.velocity = translateVelocity;
        }
    }

    private void OnSkill(InputAction.CallbackContext ctx)
    {
        CancelInvoke();

        Vector2 input = inputs.Gameplay.SkillDirection.ReadValue<Vector2>();
        print(input.ToDirection());
        switch (input.ToDirection())
        {
            case Direction.Up:
                UpwardKick(HorizontalDirection.Forward);
                break;
            case Direction.Right:
                Dash(HorizontalDirection.Forward);
                break;
            case Direction.Left:
                Dash(HorizontalDirection.Backward);
                break;
            case Direction.Down:
                DownWardKick();
                break;
            default:
                break;
        }
    }

    public void Dash(HorizontalDirection direction)
    {
        movement.SetFacingDirection(direction);

        switch (direction)
        {
            case HorizontalDirection.Forward:
            default:
                translateVelocity = Vector2.right;
                break;
            case HorizontalDirection.Backward:
                translateVelocity = Vector2.left;
                break;
        }

        translateVelocity *= dashSpeed;

        isDashingToken.SetOn(true);
        Invoke("EndDashing", dashDuration);
    }

    void EndDashing()
    {
        isDashingToken.SetOn(false);

        //Set speed to max speed
        body.velocity = translateVelocity.normalized * movement.maxSpeed;
    }

    public void UpwardKick(HorizontalDirection direction)
    {
        movement.SetFacingDirection(direction);

        float angle = upwardKickAngle * Mathf.Deg2Rad;
        translateVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        switch (direction)
        {
            case HorizontalDirection.Forward:
            default:
                break;
            case HorizontalDirection.Backward:
                translateVelocity.x *= -1;
                break;
        }
        translateVelocity.Normalize();
        translateVelocity *= upwardKickSpeed;

        isDashingToken.SetOn(true);
        Invoke("EndDashing", upwardKickDuration);
    }

    public void UpwardKick2(HorizontalDirection direction)
    {
        movement.SetFacingDirection(direction);

        Vector2 upwardKickVelocity = Vector2.up;
        switch (direction)
        {
            case HorizontalDirection.Forward:
            default:
                upwardKickVelocity.x = 1;
                break;
            case HorizontalDirection.Backward:
                upwardKickVelocity.x = -1;
                break;
        }
        upwardKickVelocity.Normalize();
        upwardKickVelocity *= upwardKickSpeed;

        body.velocity = upwardKickVelocity;
    }

    public void DownWardKick()
    {
        StartCoroutine(DownwardKickC());
    }

    IEnumerator DownwardKickC()
    {
        isDashingToken.SetOn(true);
        translateVelocity = Vector2.down * downwardKickSpeed;

        while (!ground.isGrounded)
        {
            yield return null;
        }

        isDashingToken.SetOn(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Upward kick
        Gizmos.color = Color.red;
        float length = upwardKickSpeed * upwardKickDuration;
        Gizmos.DrawRay(transform.position, new Vector2(Mathf.Cos(upwardKickAngle *Mathf.Deg2Rad), Mathf.Sin(upwardKickAngle * Mathf.Deg2Rad)) * length);

        //Dash
        Gizmos.color = Color.blue;
        length = dashDuration * dashSpeed;
        Gizmos.DrawRay(transform.position, Vector3.right * length);
    }
#endif
}
