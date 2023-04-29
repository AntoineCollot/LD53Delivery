using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSmoke : MonoBehaviour
{
    public GroundCaster ground;
    public ParticleSystem walkParticles;
    public Transform landParticles;
    ParticleSystem.EmissionModule walkEmission;
    List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    [Header("Particle Colors")]
    [SerializeField] Color defaultColor;
    [SerializeField] Color woodColor;
    [SerializeField] Color plantColor;
    [SerializeField] Color coilColor;
    [SerializeField] Color usbColor;

    // Start is called before the first frame update
    void Awake()
    {
        walkEmission = walkParticles.emission;
        landParticles.SetParent(null, true);

        ParticleSystem[] landSystems = landParticles.GetComponentsInChildren<ParticleSystem>();
        particleSystems.Add(walkParticles);
        foreach (var p in landSystems)
            particleSystems.Add(p);

    }

    private void OnEnable()
    {
        ground.onGroundedTypeChanged.AddListener(OnGroundedTypeChanged);
        ground.onGroundedStateChanged.AddListener(OnGroundedStateChanged);
        //init
        OnGroundedStateChanged(ground.isGrounded);
    }

    private void OnDisable()
    {
        if (ground != null)
        {
            ground.onGroundedTypeChanged.RemoveListener(OnGroundedTypeChanged);
            ground.onGroundedStateChanged.RemoveListener(OnGroundedStateChanged);
        }
    }

    void OnGroundedTypeChanged(GroundType newGroundType)
    {
        Color particuleCol;

        switch (ground.groundType)
        {
            default:
                particuleCol = defaultColor;
                break;
            case GroundType.Wood:
                particuleCol = woodColor;
                break;
            case GroundType.Plant:
                particuleCol = plantColor;
                break;
            case GroundType.Coil:
                particuleCol = coilColor;
                break;
            case GroundType.USB:
                particuleCol = usbColor;
                break;
        }

        foreach (ParticleSystem p in particleSystems)
        {
            ParticleSystem.MainModule main = p.main;
            main.startColor = particuleCol;
        }
    }

    private void OnGroundedStateChanged(bool isGrounded)
    {
        walkEmission.enabled = isGrounded;

        if (isGrounded)
        {
            landParticles.GetComponentInChildren<ParticleSystem>().Play();
            StartCoroutine(LandSmokeFollowPosition(0.2f));
        }
    }

    IEnumerator LandSmokeFollowPosition(float time)
    {
        float t = 0;
        float yPos = transform.position.y;

        while(t<1)
        {
            t += Time.deltaTime / time;

            Vector3 pos = transform.position;
            pos.y = yPos;
            landParticles.position =pos;

            yield return null;
        }
    }

}
