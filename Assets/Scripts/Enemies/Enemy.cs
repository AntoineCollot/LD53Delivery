using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected Animator anim;
    protected Rigidbody2D body;
    protected HorizontalDirection facingDirection = HorizontalDirection.Backward;
    protected AudioSource audioSource;

    [Header("Kill")]
    public GameObject deathFXPrefab;
    public bool isDead { get; protected set; }
    float lastHitTime;
    const float KILL_FORCE = 5;
    const float TIME_BEFORE_EXPLODE = 0.5f;
    

    SpriteRenderer[] renderers;

    protected virtual void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();

        anim = GetComponentInChildren<Animator>();
        body = GetComponent<Rigidbody2D>();
        audioSource =GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        if (GameManager.Instance.gameHasStarted)
            OnGameStart();
        else
            GameManager.Instance.onGameStart.AddListener(OnGameStart);
    }

    protected virtual void OnGameStart()
    {
        GameManager.Instance.onGameStart.RemoveListener(OnGameStart);
    }

    protected virtual void Update()
    {
        if(isDead && Time.time > lastHitTime + TIME_BEFORE_EXPLODE)
        {
            Explode();
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform == PlayerState.Instance.transform)
        {
            PlayerDeath.Instance.Kill();
        }
    }

    public void SetFacingDirection(HorizontalDirection dir)
    {
        facingDirection = dir;
        if (dir == HorizontalDirection.Backward)
            anim.transform.localScale = Vector3.one;
        else
            anim.transform.localScale = new Vector3(-1, 1, 1);
    }

    public virtual void Kill(HorizontalDirection dir, float bonusEjectForce = 0)
    {
        SetFacingDirection(dir);
        anim.SetBool("IsDead", true);
        if(!isDead)
            SFXManager.PlaySound(GlobalSFX.Hit);
        isDead = true;

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.material.SetFloat("_HitTime", Time.time);
        }

        body.velocity = (dir.ToVector2() + Vector2.up) * (KILL_FORCE+ bonusEjectForce);
        lastHitTime = Time.time;

        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Explode()
    {
        GameObject deathFX = Instantiate(deathFXPrefab, transform.position+Vector3.up * 0.3f, Quaternion.identity, null);
        Destroy(deathFX, 2);
        SFXManager.PlaySound(GlobalSFX.EnemyDeath);

        Destroy(gameObject);
    }
}
