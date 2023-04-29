using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimations : MonoBehaviour
{
    Animator anim;
    PlatformerCharacterMovement movement;
    PlatformerCharacterJump jump;
    PlatformerCharacterSkills skills;
    public GroundCaster ground;

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
        movement = GetComponentInParent<PlatformerCharacterMovement>();
        jump = GetComponentInParent<PlatformerCharacterJump>();
        skills = GetComponentInParent<PlatformerCharacterSkills>();
    }

    private void Start()
    {
        ground.onGroundedStateChanged.AddListener(OnGroundStateChanged);
        jump.onJump.AddListener(OnJump);
        skills.onSlide.AddListener(OnSlide);
        skills.onUpKick.AddListener(OnUpKick);

        movement.onFacingChanged.AddListener(OnFacingChanged);

        OnGroundStateChanged(ground.isGrounded);
    }

    void Update()
    {
        anim.SetBool("IsRunning", Mathf.Abs(movement.NormalizedHorizontalSpeed) > 0.2f);
    }

    private void OnFacingChanged(HorizontalDirection facing)
    {
        switch (facing)
        {
            case HorizontalDirection.Forward:
            default:
                transform.localScale = Vector3.one;
                break;
            case HorizontalDirection.Backward:
                transform.localScale = new Vector3(-1,1,1);
                break;
        }
    }

    private void OnGroundStateChanged(bool isGrounded)
    {
        anim.SetBool("IsGrounded", isGrounded);
    }

    private void OnSlide()
    {
        anim.SetBool("IsSliding", true);
        StartCoroutine(WatchSliding());
    }

    private void OnUpKick()
    {
        anim.SetTrigger("UpKick");
    }

    IEnumerator WatchSliding()
    {
        while (skills.IsDashing)
            yield return true;

        anim.SetBool("IsSliding", false);
    }

    private void OnJump()
    {
        anim.SetTrigger("Jump");
    }

}
