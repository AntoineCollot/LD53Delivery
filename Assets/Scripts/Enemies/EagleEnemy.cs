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

    public float ProjectileSpeed => baseProjectileSpeed * LevelManager.Instance.levelConstants.eagleProjectileSpeedMult;

    private void Start()
    {
        StartCoroutine(FireLoop());
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
        anim.SetTrigger("Fire");

        yield return new WaitForSeconds(0.35f);

        Vector2 dir = (PlayerState.Instance.transform.position + Vector3.up * 0.3f) - fireOrigin.position;
        dir.Normalize();

        Rigidbody2D projectile = Instantiate(projectilePrefab, fireOrigin.position, Quaternion.identity, null);
        projectile.velocity = dir * ProjectileSpeed;
    }
}
