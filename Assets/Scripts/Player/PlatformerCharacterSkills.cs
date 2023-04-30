using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlatformerCharacterSkills : PlayerController
{
    public LayerMask enemiesLayer;
    public ParticleSystem cooldownReadyFX;

    [Header("Dash")]
    public float dashSpeed = 20;
    public float dashDuration = 0.3f;
    CompositeStateToken isSlidingToken;

    [Header("Slide")]
    [Range(0, 90)] public float slideShootAngle = 20;
    public float slideShootRange = 3;
    public Transform slideShootHolder;
    public ParticleSystem slideExplosionFX;
    public float slideCooldown = 2;
    float lastSlideTime;
    bool canSlide = true;
    bool hasAirbornSlide = false;

    [Header("UpwardKick")]
    public float upwardKickSpeed = 30;
    [Range(0, 90)] public float upwardKickAngle = 45;
    public float upwardKickDuration = 0.3f;
    const float UPKICK_HITBOX_ANGLE = 35;
    const float UPKICK_HITBOX_RADIUS = 0.5f;
    CompositeStateToken isKickingToken;
    const float UPKICK_BUFFER_AFTER_SLIDE = 0.5f;

    [Header("Dive")]
    public float diveSpeed = 30;
    CompositeStateToken isDivingToken;
    public GameObject landingFXPrefab;
    public float landingDamageRange;
    const int ENEMY_LAYER = 9;
    float lastJumpTime;
    public float minDiveTimeAfterJump = 0.25f;
    bool canDive = false;

    Vector2 translateVelocity;

    [Header("Components")]
    Rigidbody2D body;
    GroundCaster ground;
    PlatformerCharacterMovement movement;
    PlatformerCharacterJump jump;

    //Events
    public UnityEvent onSlide = new UnityEvent();
    public UnityEvent onUpKick = new UnityEvent();
    public UnityEvent onDive = new UnityEvent();
    public bool IsSliding => isSlidingToken.IsOn;
    public bool IsDiving => isDivingToken.IsOn;
    public bool IsKicking => isKickingToken.IsOn;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        body = GetComponent<Rigidbody2D>();
        ground = GetComponentInChildren<GroundCaster>();
        movement = GetComponent<PlatformerCharacterMovement>();
        jump = GetComponent<PlatformerCharacterJump>();
    }

    // Start is called before the first frame update
    void Start()
    {
        isSlidingToken = new CompositeStateToken();
        isDivingToken = new CompositeStateToken();
        isKickingToken = new CompositeStateToken();
        PlayerState.Instance.freezeInputsState.Add(isSlidingToken);
        PlayerState.Instance.freezeInputsState.Add(isDivingToken);
        PlayerState.Instance.freezeInputsState.Add(isKickingToken);

        jump.onJump.AddListener(OnJump);
        ground.onGroundedStateChanged.AddListener(OnGroundedStateChanged);
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
        if (IsSliding || IsDiving || IsKicking)
        {
            body.velocity = translateVelocity;
        }
    }

    private void Update()
    {
        if (!canDive && Time.time > lastJumpTime + minDiveTimeAfterJump && !ground.isGrounded)
        {
            canDive = true;
            cooldownReadyFX.Emit(1);
        }

        if (!canSlide && Time.time > lastSlideTime + slideCooldown)
        {
            canSlide = true;
            cooldownReadyFX.Play();
        }
    }

    private void OnSkill(InputAction.CallbackContext ctx)
    {
        if (IsDiving || IsKicking)
        {
            return;
        }
        if (IsSliding)
        {
            //Stop slide
            ForceEndSlide();
            UpwardKick(movement.facingDirection);
            return;
        }
        Vector2 input = inputs.Gameplay.SkillDirection.ReadValue<Vector2>();

        if (!IsSliding && Time.time < lastSlideTime + UPKICK_BUFFER_AFTER_SLIDE && input.y >= -0.1f)
        {
            UpwardKick(movement.facingDirection);
            return;
        }

        if(ground.isGrounded && input.magnitude <0.3f)
        {
            Slide(movement.facingDirection);
            return;
        }

        switch (input.ToDirection())
        {
            case Direction.Up:
                //UpwardKick(HorizontalDirection.Forward);
                Dive();
                break;
            case Direction.Right:
            default:
                Slide(HorizontalDirection.Forward);
                break;
            case Direction.Left:
                Slide(HorizontalDirection.Backward);
                break;
            case Direction.Down:
                Dive();
                break;
        }
    }

    public void Slide(HorizontalDirection direction)
    {
        if (!ground.isGrounded && !hasAirbornSlide)
            return;
        else if (ground.isGrounded && Time.time <= lastSlideTime + slideCooldown)
            return;

        hasAirbornSlide = false;
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
        isSlidingToken.SetOn(true);
        lastSlideTime = Time.time;
        canSlide = false;

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

        while (t < 1)
        {
            t += Time.deltaTime / dashDuration;

            int hitCount = Physics2D.Raycast(slideShootHolder.position, shootDir, contactFilter, hits, slideShootRange);

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].transform.TryGetComponent(out Enemy enemy))
                {
                    enemy.Kill(movement.facingDirection);
                    OnEnemyKill();
                }
            }

            yield return null;
        }

        slideExplosionFX.Stop();
        slideShootHolder.gameObject.SetActive(false);
        isSlidingToken.SetOn(false);

        //Clamp speed to max speed
        body.velocity = translateVelocity.normalized * movement.maxSpeed;
    }

    private void OnGroundedStateChanged(bool isGrounded)
    {
        if (!isGrounded)
            hasAirbornSlide = true;
    }

    void ForceEndSlide()
    {
        StopAllCoroutines();

        slideExplosionFX.Stop();
        slideShootHolder.gameObject.SetActive(false);
        isSlidingToken.SetOn(false);

        //Clamp speed to max speed
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

        isKickingToken.SetOn(true);
        onUpKick.Invoke();
        StartCoroutine(UpKickAnim(!ground.isGrounded));
    }

    IEnumerator UpKickAnim(bool airborn)
    {
        float t = 0;

        float hitBoxAngle = UPKICK_HITBOX_ANGLE * Mathf.Deg2Rad;
        Vector3 hitboxDir = new Vector3(Mathf.Cos(hitBoxAngle), Mathf.Sin(hitBoxAngle), 0);
        if (movement.facingDirection == HorizontalDirection.Backward)
        {
            hitboxDir.x *= -1;
        }

        while (t < 1)
        {
            if(airborn)
                t += Time.deltaTime / (upwardKickDuration*0.5f);
            else
                t += Time.deltaTime / upwardKickDuration;

            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position + hitboxDir, UPKICK_HITBOX_RADIUS, enemiesLayer);

            foreach (Collider2D col in cols)
            {
                if (col.transform.TryGetComponent(out Enemy enemy))
                {
                    enemy.Kill(movement.facingDirection, 5);
                    OnEnemyKill();
                }
            }

            yield return null;
        }

        isKickingToken.SetOn(false);

        //Clamp speed to max speed plus a bit
        body.velocity = translateVelocity.normalized * movement.maxSpeed * 1.2f;
    }

    public void Dive()
    {
        if (!canDive || ground.isGrounded)
            return;
        StartCoroutine(DiveAnim());
        onDive.Invoke();
    }

    private void OnJump()
    {
        canDive = !canDive;
        lastJumpTime = Time.time;
    }

    IEnumerator DiveAnim()
    {
        isDivingToken.SetOn(true);
        Physics2D.IgnoreLayerCollision(ENEMY_LAYER, gameObject.layer, true);
        translateVelocity = Vector2.down * diveSpeed;

        while (!ground.isGrounded)
        {
            yield return null;
        }

        GameObject landingFX = Instantiate(landingFXPrefab, transform.position, Quaternion.identity, null);
        Destroy(landingFX, 2);

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, landingDamageRange, enemiesLayer);
        foreach (Collider2D col in cols)
        {
            if (col.TryGetComponent(out Enemy enemy))
            {
                HorizontalDirection dir = HorizontalDirection.Forward;
                if (enemy.transform.position.x < transform.position.x)
                    dir = HorizontalDirection.Backward;

                enemy.Kill(dir);
                OnEnemyKill();
            }
        }

        yield return new WaitForSeconds(0.25f);

        isDivingToken.SetOn(false);
        Physics2D.IgnoreLayerCollision(ENEMY_LAYER, gameObject.layer, false);
    }

    void OnEnemyKill()
    {
        if (!ground.isGrounded)
        {
            hasAirbornSlide = true;
            jump.AddAirbornJump();
        }

        ScoreSystem.Instance.AddKill();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Upward kick
        Gizmos.color = Color.red;
        float length = upwardKickSpeed * upwardKickDuration;
        Gizmos.DrawRay(transform.position, new Vector2(Mathf.Cos(upwardKickAngle * Mathf.Deg2Rad), Mathf.Sin(upwardKickAngle * Mathf.Deg2Rad)) * length);
        float hitBoxAngle = UPKICK_HITBOX_ANGLE * Mathf.Deg2Rad;
        Vector3 hitboxDir = new Vector3(Mathf.Cos(hitBoxAngle), Mathf.Sin(hitBoxAngle), 0);
        Gizmos.DrawWireSphere(transform.position + hitboxDir, UPKICK_HITBOX_RADIUS);

        //Dash
        Gizmos.color = Color.blue;
        length = dashDuration * dashSpeed;
        Gizmos.DrawRay(transform.position, Vector3.right * length);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(slideShootHolder.position, new Vector2(Mathf.Cos(slideShootAngle * Mathf.Deg2Rad), Mathf.Sin(slideShootAngle * Mathf.Deg2Rad)) * slideShootRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, landingDamageRange);
    }
#endif
}
