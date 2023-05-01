using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GroundType { Stone, Wood, Plant, Coil,USB }
public class GroundCaster : MonoBehaviour
{
    [SerializeField, Range(0, 0.5f)] float castDistance = 0.1f;
    [SerializeField, Range(0, 1)] float castRadius = 0.3f;
    [SerializeField] LayerMask groundLayers;

    float lastUpdateTime;
    RaycastHit2D[] circleCastHits = new RaycastHit2D[10];
    bool lastGroundedState = false;
    Rigidbody2D body;

    //type
    public GroundType groundType { get; private set; }
    GroundType lastGroundType;
    public class GroundedTypeEvent : UnityEvent<GroundType> { }
    public GroundedTypeEvent onGroundedTypeChanged = new GroundedTypeEvent();

    //State
    public class GroundedEvent : UnityEvent<bool> { }
    public GroundedEvent onGroundedStateChanged = new GroundedEvent();

    public bool isGrounded { get; private set; }

    private void Awake()
    {
        body = GetComponentInParent<Rigidbody2D>();
    }

    public void CastGround()
    {
        //Don't cast multiple times per fixed frame
        if (lastUpdateTime >= Time.fixedTime || !GameManager.Instance.gameHasStarted)
            return;

        lastUpdateTime = Time.fixedTime;

        if (PlayerState.Instance.freezeGroundDetectionState.IsOn)
            isGrounded = false;
        //Can't be grounded when going upward
        else if (!lastGroundedState && body.velocity.y > 0.01f)
        {
            isGrounded = false;
        }
        else
        {
            //Get hit with the left only as we only need this info once.
            RaycastHit2D leftHit;
            leftHit = Physics2D.Raycast(transform.position - Vector3.left * castRadius, Vector2.down, castDistance, groundLayers);
            bool left = leftHit.transform != null;
            bool right = Physics2D.Raycast(transform.position - Vector3.right * castRadius, Vector2.down, castDistance, groundLayers);
            // int objectHitCount = Physics2D.CircleCastNonAlloc(transform.position, castRadius, Vector2.down, circleCastHits, castDistance, groundLayers);
            isGrounded = left || right;

            //Type
            if (left)
            {
                if (System.Enum.TryParse(typeof(GroundType), leftHit.collider.tag, out object type))
                {
                    groundType = (GroundType)type;
                    if(groundType != lastGroundType)
                    {
                        lastGroundType = groundType;
                        onGroundedTypeChanged.Invoke(groundType);
                    }    
                }
            }
        }

        //Event
        if (lastGroundedState != isGrounded)
        {
            onGroundedStateChanged.Invoke(isGrounded);
            if (isGrounded)
                SFXManager.PlaySound(GlobalSFX.Land);
        }

        lastGroundedState = isGrounded;
    }

    private void FixedUpdate()
    {
        CastGround();
    }

    private void OnDrawGizmosSelected()
    {
        if (isGrounded)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.blue;
        //Vector3 castTargetPos = transform.position + Vector3.down * castDistance;
        //Gizmos.DrawLine(transform.position, castTargetPos);
        //Gizmos.DrawWireSphere(castTargetPos, castRadius);

        Gizmos.DrawRay(transform.position - Vector3.left * castRadius, Vector2.down * castDistance);
        Gizmos.DrawRay(transform.position - Vector3.right * castRadius, Vector2.down * castDistance);
    }
}
