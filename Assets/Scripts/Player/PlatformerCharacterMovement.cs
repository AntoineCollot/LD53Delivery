using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlatformerCharacterMovement : PlayerController
{
    [Header("Grounded")]
    [Range(1, 10)] public float maxSpeed = 4;
    [SerializeField, Range(1, 80)] float maxAcceleration = 20;
    [SerializeField, Range(1, 80)] float maxDeceleration = 20;
    [SerializeField, Range(1, 80)] float maxTurnSpeed = 20;

    [Header("Airborn")]
    [SerializeField, Range(1, 80)] float maxAirAcceleration = 10;
    [SerializeField, Range(1, 80)] float maxAirDeceleration = 10;
    [SerializeField, Range(1, 80)] float maxAirTurnSpeed = 5;

    float acceleration;
    float deceleration;
    public float directionInput { get; private set; }
    Vector2 desiredVelocity;
    float turnSpeed;
    float maxSpeedChange;
    Vector2 velocity;

    public enum MovementState { Idle, Acceleration, MaxSpeed, Deceleration, Turning }
    public MovementState movementState { get; private set; }

    //Facing
    public HorizontalDirection facingDirection { get; private set; }
    public HorizontalDirEvent onFacingChanged = new HorizontalDirEvent();

    public class SpawnEvent : UnityEvent<Vector3> { }
    public SpawnEvent onSpawnAtPosition = new SpawnEvent();

    [Header("Components")]
    Rigidbody2D body;
    GroundCaster ground;

    public bool IsGrounded => ground.isGrounded;
    public float NormalizedHorizontalSpeed => velocity.x / maxSpeed;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        body = GetComponent<Rigidbody2D>();
        ground = GetComponentInChildren<GroundCaster>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        inputs.Gameplay.Move.performed += OnMovement;
        inputs.Gameplay.Move.canceled += OnMovement;
    }


    protected override void OnDisable()
    {
        base.OnDisable();

        inputs.Gameplay.Move.performed -= OnMovement;
        inputs.Gameplay.Move.canceled -= OnMovement;
    }

    // Update is called once per frame
    void Update()
    {
        float currentDirectionInput = directionInput;
        //Check if we should freeze the inputs
        if (PlayerState.Instance.freezeInputsState.IsOn)
        {
            currentDirectionInput = 0;
        }

        desiredVelocity = new Vector2(currentDirectionInput, 0f) * maxSpeed;

        if ((movementState == MovementState.Turning || movementState == MovementState.Acceleration) && ground.isGrounded)
        {
            if (currentDirectionInput > 0)
            {
                SetFacingDirection(HorizontalDirection.Forward);
            }
            else if (currentDirectionInput < 0)
            {
                SetFacingDirection(HorizontalDirection.Backward);
            }
        }
    }

    private void FixedUpdate()
    {
        velocity = body.velocity;

        float currentDirectionInput = directionInput;
        //Check if we should freeze the inputs
        if (PlayerState.Instance.freezeInputsState.IsOn)
        {
            currentDirectionInput = 0;
        }

        //Set the correct settings for grounded or airborn
        if (IsGrounded)
        {
            acceleration = maxAcceleration;
            deceleration = maxDeceleration;
            turnSpeed = maxTurnSpeed;
        }
        else
        {
            acceleration = maxAirAcceleration;
            deceleration = maxAirDeceleration;
            turnSpeed = maxAirTurnSpeed;
        }

        //Movement state
        movementState = MovementState.Idle;

        if (currentDirectionInput != 0)
        {
            if (Mathf.Abs(velocity.x) > 0.01f && Mathf.Sign(currentDirectionInput) != Mathf.Sign(velocity.x))
            {
                movementState = MovementState.Turning;
            }
            else if (Mathf.Abs(desiredVelocity.x) > Mathf.Abs(velocity.x))
            {
                movementState = MovementState.Acceleration;
            }
            else
            {
                movementState = MovementState.MaxSpeed;
            }
        }
        else if (Mathf.Abs(desiredVelocity.x) < Mathf.Abs(velocity.x))
        {
            movementState = MovementState.Deceleration;
        }

        //Speed change update based on movement state
        switch (movementState)
        {
            case MovementState.Idle:
            case MovementState.MaxSpeed:
            case MovementState.Acceleration:
                maxSpeedChange = acceleration * Time.fixedDeltaTime;
                break;
            case MovementState.Deceleration:
                maxSpeedChange = deceleration * Time.fixedDeltaTime;
                break;
            case MovementState.Turning:
                maxSpeedChange = turnSpeed * Time.fixedDeltaTime;
                break;
        }

        //Update velocity
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.y = body.velocity.y;

        if (PlayerState.Instance.freezePositionState.IsOn)
            velocity.x = 0;

        body.velocity = velocity;
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        directionInput = context.ReadValue<float>();
    }

    public void SetFacingDirection(HorizontalDirection dir)
    {
        facingDirection = dir;
        onFacingChanged.Invoke(facingDirection);
    }

    public void TranslateToPosition(Vector3 translatePosition, float time)
    {
        translatePosition.z = 0;
        StartCoroutine(TranslateToPositionC(translatePosition, time));
    }

    IEnumerator TranslateToPositionC(Vector3 translatePosition, float time)
    {
        float t = 0;
        Vector3 startPosition = body.position;

        while (t < 1)
        {
            t += Time.deltaTime / time;

            body.position = Curves.QuadEaseInOut(startPosition, translatePosition, Mathf.Clamp01(t));

            yield return null;
        }
    }

    public void SpawnAtPosition(Vector3 spawnPosition)
    {
        SetFacingDirection(HorizontalDirection.Forward);

        spawnPosition.z = 0;
        body.position = spawnPosition;

        onSpawnAtPosition.Invoke(spawnPosition);
    }

}
