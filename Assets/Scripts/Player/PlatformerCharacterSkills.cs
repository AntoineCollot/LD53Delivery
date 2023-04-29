using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlatformerCharacterSkills : PlayerController
{
    public LayerMask enemiesLayer;

    [Header("Dash")]
    public float dashSpeed = 20;
    public float dashDuration = 0.3f;
    CompositeStateToken isDashingToken;

    [Header("Slide")]
    [Range(0, 90)] public float slideShootAngle = 20;
    public float slideShootRange = 3;
    public Transform slideShootHolder;
    public ParticleSystem slideExplosionFX;

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

    //Events
    public UnityEvent onSlide = new UnityEvent();
    public UnityEvent onUpKick = new UnityEvent();
    public bool IsDashing => isDashingToken.IsOn;

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
        if (IsDashing)
        {
            body.velocity = translateVelocity;
        }
    }

    private void OnSkill(InputAction.CallbackContext ctx)
    {
        CancelInvoke();

        Vector2 input = inputs.Gameplay.SkillDirection.ReadValue<Vector2>();
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

        onSlide.Invoke();
        StartCoroutine(SlideAnim());
    }

    IEnumerator SlideAnim()
    {
        float t = 0;
        RaycastHit2D[] hits = new RaycastHit2D[5];
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.layerMask = enemiesLayer;
        Vector2 shootDir = new Vector2(Mathf.Cos(slideShootAngle * Mathf.Deg2Rad), Mathf.Sin(slideShootAngle * Mathf.Deg2Rad));
        if (movement.facingDirection == HorizontalDirection.Backward)
        {
            shootDir.x *= -1;
        }

        slideShootHolder.gameObject.SetActive(true);
        slideExplosionFX.Play();

        while (t<1)
        {
            t += Time.deltaTime / dashDuration;

            int hitCount = Physics2D.Raycast(slideShootHolder.position, shootDir, contactFilter, hits,slideShootRange);

            for (int i = 0; i < hitCount; i++)
            {
                if(hits[i].transform.TryGetComponent(out Enemy enemy))
                {
                    enemy.Kill(movement.facingDirection);
                }
            }

            yield return null;
        }

        slideExplosionFX.Stop();
        slideShootHolder.gameObject.SetActive(false);
        isDashingToken.SetOn(false);

        //Clamp speed to max speed
        body.velocity = translateVelocity.normalized * movement.maxSpeed;
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

        onUpKick.Invoke();
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
        onUpKick.Invoke();
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

        Gizmos.color = Color.red;
        Gizmos.DrawRay(slideShootHolder.position, new Vector2(Mathf.Cos(slideShootAngle * Mathf.Deg2Rad), Mathf.Sin(slideShootAngle * Mathf.Deg2Rad)) * slideShootRange);
    }
#endif
}
