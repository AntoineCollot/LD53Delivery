using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSmoke : MonoBehaviour
{
    public GroundCaster ground;
    public ParticleSystem walkParticles;
    public ParticleSystem landParticles;
    ParticleSystem.EmissionModule walkEmission;
    AudioSource audioSource;
    float baseVolume;
    PlatformerCharacterMovement movement;

    // Start is called before the first frame update
    void Awake()
    {
        walkEmission = walkParticles.emission;
        audioSource = GetComponent<AudioSource>();
        movement = GetComponentInParent<PlatformerCharacterMovement>();
        baseVolume = audioSource.volume;
    }

    private void OnEnable()
    {
        ground.onGroundedStateChanged.AddListener(OnGroundedStateChanged);
        //init
        OnGroundedStateChanged(ground.isGrounded);
    }

    private void OnDisable()
    {
        if (ground != null)
        {
            ground.onGroundedStateChanged.RemoveListener(OnGroundedStateChanged);
        }
    }

    private void Update()
    {
        audioSource.volume = Mathf.Lerp(0, baseVolume, Mathf.Abs(movement.NormalizedHorizontalSpeed)); 
    }

    private void OnGroundedStateChanged(bool isGrounded)
    {
        walkEmission.enabled = isGrounded;
        audioSource.mute = !isGrounded;

        if (isGrounded)
        {
            landParticles.Play();
        }
    }

}
