using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : PlayerController
{
    PlatformerCharacterMovement movement;
    GroundCaster ground;
    float currentYPos;
    float lastGroundedHeight;
    public float maxHeightDiffBeforeSnap = 3;
    bool followCharacter;

    [Header("Camera Offset")]
    [Range(0, 20)] public float maxOffset = 5;
    Vector2 inputOffset;
    Vector2 currentOffset;
    Vector2 refOffset;
    const float OFFSET_SMOOTH = 0.1f;

    protected override void Awake()
    {
        base.Awake();

        movement = GetComponentInParent<PlatformerCharacterMovement>();
        ground = movement.GetComponentInChildren<GroundCaster>();
        ground.onGroundedStateChanged.AddListener(OnGroundStateChanged);

        inputs.Gameplay.MoveCamera.performed += MoveCameraPerformed;
        inputs.Gameplay.MoveCamera.canceled += MoveCameraPerformed;
    }

    private void MoveCameraPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        inputOffset = ctx.ReadValue<Vector2>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Follow the character if we are too far from the last grounded pos
        float distToLastGroundedHeight = Mathf.Abs(movement.transform.position.y - lastGroundedHeight);
        if (distToLastGroundedHeight > maxHeightDiffBeforeSnap)
        {
            followCharacter = true;
        }

        Vector3 targetPos = movement.transform.position;
        if (followCharacter)
            currentYPos = movement.transform.position.y;
        targetPos.y = currentYPos;

        currentOffset = Vector2.SmoothDamp(currentOffset, inputOffset * maxOffset, ref refOffset, OFFSET_SMOOTH);
        transform.position = targetPos + new Vector3(currentOffset.x, currentOffset.y, 0);
    }

    private void OnGroundStateChanged(bool isGrounded)
    {
        if (isGrounded)
        {
            lastGroundedHeight = movement.transform.position.y;
            followCharacter = true;
        }
        else
        {
            followCharacter = false;
        }
    }
}
