using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatEnemy : Enemy
{
    [Header("Settings")]
    public float baseRollInterval = 3;
    public float baseRollDuration = 3;
    public float baseRollSpeed = 3;
    HorizontalDirection currentDirection = HorizontalDirection.Backward;

    [Header("Navigation")]
    public Transform obstacleDetectionOrigin;
    public LayerMask levelLayer;
    public float obstacleDetectionRange = 0.5f;

    public bool isRolling { get; private set; }
    public float RollSpeed => baseRollSpeed * LevelManager.Instance.levelConstants.blackCatRollSpeedMult;


    private void Start()
    {
        StartCoroutine(Roll());
    }

    protected override void Update()
    {
        base.Update();

        if(isDead) return;

        RaycastHit2D forwardHit = Physics2D.Raycast(obstacleDetectionOrigin.position, currentDirection.ToVector2(), obstacleDetectionRange, levelLayer);
        if(forwardHit.transform != null)
        {
            TurnAround();
        }

        //Vector2 downOrigin = (Vector2)obstacleDetectionOrigin.position + currentDirection.ToVector2() * obstacleDetectionRange;
        //RaycastHit2D downwardHit = Physics2D.Raycast(downOrigin, Vector2.down, obstacleDetectionOrigin.localPosition.y - 0.2f, levelLayer);
        //if(downwardHit.transform != null)
        //{
        //    TurnAround();
        //    return;
        //}
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        Vector2 direction = currentDirection.ToVector2();
        Vector2 velocity = body.velocity;

        if (isRolling)
            velocity.x = RollSpeed * direction.x;
        else
            velocity.x = 0;

        body.velocity = velocity;
    }

    IEnumerator Roll()
    {
        //Random startup so cats are desync
        yield return new WaitForSeconds(Random.Range(0f, 3.0f));

        while(GameManager.Instance.GameIsPlaying && !isDead)
        {
            yield return new WaitForSeconds(baseRollInterval * LevelManager.Instance.levelConstants.blackCatRollIntervalMult);

            anim.SetBool("IsRolling", true);
            isRolling = true;

            yield return new WaitForSeconds(baseRollDuration * LevelManager.Instance.levelConstants.blackCatRollDurationMult);

            isRolling = false;
            anim.SetBool("IsRolling", false);
        }
    }

    public void TurnAround()
    {
        if (currentDirection == HorizontalDirection.Forward)
            currentDirection = HorizontalDirection.Backward;
        else
            currentDirection = HorizontalDirection.Forward;

        SetFacingDirection(currentDirection);
    }

   

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(obstacleDetectionOrigin.position, currentDirection.ToVector2() * obstacleDetectionRange);
    }
#endif
}
