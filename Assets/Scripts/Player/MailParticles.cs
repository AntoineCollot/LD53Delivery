using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailParticles : MonoBehaviour
{
    Rigidbody2D body;
    ParticleSystem particles;
    ParticleSystem.EmissionModule emission;
    Vector2 lastVelocity;
    float baseEmission;

    // Start is called before the first frame update
    void Awake()
    {
        body = GetComponentInParent<Rigidbody2D>();
        particles = GetComponentInParent<ParticleSystem>();
        emission = particles.emission;
        baseEmission = emission.rateOverTimeMultiplier;
    }

    // Update is called once per frame
    void Update()
    {
        float velocityChange = Mathf.Abs(body.velocity.x - lastVelocity.x) + Mathf.Abs(body.velocity.y - lastVelocity.y);
        emission.rateOverTimeMultiplier = baseEmission * velocityChange;

        lastVelocity = body.velocity;
    }
}
