using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EagleEnemy : Enemy
{
    [Header("Fire")]
    public float baseFireInterval;
    public float baseProjectileSpeed;
    public Rigidbody2D projectilePrefab;
    public Transform fireOrigin;
    [Range(0, 90)] public float maxAngle = 70;

    public float ProjectileSpeed => baseProjectileSpeed * LevelManager.Instance.levelConstants.eagleProjectileSpeedMult;

    private void Start()
    {
        StartCoroutine(FireLoop());
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            body.velocity = Vector3.zero;
        }
    }

    IEnumerator FireLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2.0f));
        while (GameManager.Instance.GameIsPlaying && !isDead)
        {
            yield return new WaitForSeconds(baseFireInterval * LevelManager.Instance.levelConstants.eagleFireIntervalMult);
            StartCoroutine(Fire());
        }
    }

    IEnumerator Fire()
    {
        float angleToPlayer = Vector2.Angle(((PlayerState.Instance.transform.position + Vector3.up * 0.3f) - fireOrigin.position).normalized, facingDirection.ToVector2());
        if (Mathf.Abs(angleToPlayer) >= maxAngle)
            yield break;

        anim.SetTrigger("Fire");

        yield return new WaitForSeconds(0.35f);

        Vector2 dir = (PlayerState.Instance.transform.position + Vector3.up * 0.3f) - fireOrigin.position;
        dir.Normalize();

        Rigidbody2D projectile = Instantiate(projectilePrefab, fireOrigin.position, Quaternion.identity, null);
        projectile.velocity = dir * ProjectileSpeed;
    }

    public override void Kill(HorizontalDirection dir, float bonusEjectForce = 0)
    {
        base.Kill(dir, bonusEjectForce);

        body.gravityScale = 1;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        float angle = maxAngle * Mathf.Deg2Rad;
        Gizmos.DrawRay(transform.position, new Vector2(-Mathf.Cos(angle), Mathf.Sin(angle)));
        Gizmos.DrawRay(transform.position, new Vector2(-Mathf.Cos(angle), -Mathf.Sin(angle)));
    }
#endif
}
