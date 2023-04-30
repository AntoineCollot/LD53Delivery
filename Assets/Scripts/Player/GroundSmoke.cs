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

    // Start is called before the first frame update
    void Awake()
    {
        walkEmission = walkParticles.emission;
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

    private void OnGroundedStateChanged(bool isGrounded)
    {
        walkEmission.enabled = isGrounded;

        if (isGrounded)
        {
            landParticles.Play();
        }
    }

}
